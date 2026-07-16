using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.InferenceEngine;
using Unity.Collections;
using Microsoft.ML.Tokenizers;
using System.IO;

public class AIController : MonoBehaviour
{
    public ModelAsset modelAsset;
    private Worker worker;
    private Model runtimeModel;
    private Tokenizer tokenizer;

    private const int MAX_TOKENS = 50;
    private const int VOCAB_SIZE = 51200;
    private const int CONTEXT_SIZE = 128; // Жестко фиксируем память от утечек

    void Start()
    {
        string vocabPath = Path.Combine(Application.streamingAssetsPath, "vocab.json");
        string mergesPath = Path.Combine(Application.streamingAssetsPath, "merges.txt");

        // ТВОЙ РОДНОЙ И РАБОЧИЙ МЕТОД СОЗДАНИЯ ТОКЕНИЗАТОРА
        tokenizer = BpeTokenizer.Create(vocabPath, mergesPath);
        Debug.Log("Токенизатор заряжен! Теперь точно.");

        runtimeModel = ModelLoader.Load(modelAsset);
        worker = new Worker(runtimeModel, BackendType.GPUCompute);
    }

    // --- ПУБЛИЧНЫЙ API ДЛЯ BUILD MANAGER ---
    public void CheckCompatibility(string prompt, Action<string> onComplete)
    {
        StartCoroutine(GenerateTextCoroutine(prompt, onComplete));
    }

    IEnumerator GenerateTextCoroutine(string prompt, Action<string> onComplete)
    {
        List<int> tokenIds = tokenizer.EncodeToIds(prompt).ToList();
        int promptTokenCount = tokenIds.Count;

        Debug.Log("ИИ думает (в экономном режиме 128 токенов)...");

        for (int i = 0; i < MAX_TOKENS; i++)
        {
            if (tokenIds.Count >= CONTEXT_SIZE) break; // Защита от переполнения

            // 1. Создаем массивы фиксированного размера для тензоров
            int[] fixedTokens = new int[CONTEXT_SIZE];
            int[] fixedAttention = new int[CONTEXT_SIZE];

            for (int j = 0; j < CONTEXT_SIZE; j++)
            {
                if (j < tokenIds.Count)
                {
                    fixedTokens[j] = tokenIds[j];
                    fixedAttention[j] = 1; // Единица - реальный текст
                }
                else
                {
                    fixedTokens[j] = 50256;
                    fixedAttention[j] = 0; // Ноль - пустышка, ИИ ее игнорирует
                }
            }

            // 2. Переводим в NativeArray и Тензоры
            var nativeTokensData = new NativeArray<int>(fixedTokens, Allocator.TempJob);
            var inputTensor = new Tensor<int>(new TensorShape(1, CONTEXT_SIZE), nativeTokensData);

            var nativeAttentionData = new NativeArray<int>(fixedAttention, Allocator.TempJob);
            var attentionTensor = new Tensor<int>(new TensorShape(1, CONTEXT_SIZE), nativeAttentionData);

            // 3. Передаем ДВА входа, как того требует GPT-2
            worker.SetInput("input_ids", inputTensor);
            worker.SetInput("attention_mask", attentionTensor);

            worker.Schedule();
            yield return null;

            var outputTensor = worker.PeekOutput(runtimeModel.outputs[0].name) as Tensor<float>;

            // Теперь мы передаем весь список токенов, чтобы метод знал, кого штрафовать
            int nextTokenId = GetNextTokenArgMax(outputTensor, tokenIds.Count, tokenIds);

            // 4. ЖЕСТКАЯ ОЧИСТКА ПАМЯТИ
            inputTensor.Dispose();
            nativeTokensData.Dispose();
            attentionTensor.Dispose();
            nativeAttentionData.Dispose();

            if (nextTokenId == 50256)
                break;

            tokenIds.Add(nextTokenId);
        }

        var generatedTokens = tokenIds.Skip(promptTokenCount).ToArray();
        string finalAnswer = tokenizer.Decode(generatedTokens).Trim();
        // Очищаем ответ от артефактов GPT-2 токенизатора
        finalAnswer = finalAnswer.Replace("Ġ", " ").Replace("Ċ", "\n").Replace("ĊĊ", "\n");

        Debug.Log($"<color=cyan>ИИ Вердикт:</color> {finalAnswer}");
        onComplete?.Invoke(finalAnswer);
    }

    int GetNextTokenArgMax(Tensor<float> logits, int currentSequenceLength, List<int> previousTokens)
    {
        if (logits == null)
        {
            Debug.LogError("Тензор logits равен null!");
            return 50256;
        }

        using NativeArray<float> logitsArray = logits.DownloadToNativeArray();
        int actualVocabSize = logits.shape[logits.shape.rank - 1];

        int tokenIndex = currentSequenceLength - 1;
        int offset = tokenIndex * actualVocabSize;

        float maxVal = -float.MaxValue;
        int maxIndex = 0;

        // ДОБАВЛЕНА ЛОГИКА ШТРАФА ЗА ПОВТОРЕНИЯ

        // Перекидываем токены в HashSet для мгновенного поиска 
        HashSet<int> penaltySet = new HashSet<int>(previousTokens);

        // 1.0 = нет штрафа. 1.2 - 2.0 = диапазон для GPT-2.
        float repetitionPenalty = 1.5f;

        for (int i = 0; i < actualVocabSize; i++)
        {
            float val = logitsArray[offset + i];

            // Если слово уже было в тексте, режем его вероятность
            if (penaltySet.Contains(i))
            {
                if (val > 0)
                    val /= repetitionPenalty;
                else
                    val *= repetitionPenalty;
            }

            if (val > maxVal)
            {
                maxVal = val;
                maxIndex = i;
            }
        }
        return maxIndex;
    }

    void OnDestroy()
    {
        worker?.Dispose();
    }
}