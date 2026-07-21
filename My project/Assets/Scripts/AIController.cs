using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

    private const int MAX_TOKENS = 40; // Уменьшили, чтобы не генерировала простыни
    private const int VOCAB_SIZE = 51200;
    private const int CONTEXT_SIZE = 256;

    void Start()
    {
        string vocabPath = Path.Combine(Application.streamingAssetsPath, "vocab.json");
        string mergesPath = Path.Combine(Application.streamingAssetsPath, "merges.txt");

        tokenizer = BpeTokenizer.Create(vocabPath, mergesPath);

        if (modelAsset != null)
        {
            runtimeModel = ModelLoader.Load(modelAsset);
            worker = new Worker(runtimeModel, BackendType.GPUCompute);
        }
    }

    public void CheckCompatibility(string prompt, Action<string> onComplete)
    {
        StartCoroutine(GenerateTextCoroutine(prompt, onComplete));
    }

    IEnumerator GenerateTextCoroutine(string prompt, Action<string> onComplete)
    {
        if (tokenizer == null || worker == null)
        {
            onComplete?.Invoke("Error: AI Engine not initialized.");
            yield break;
        }

        List<int> tokenIds = tokenizer.EncodeToIds(prompt).ToList();

        if (tokenIds.Count > CONTEXT_SIZE - MAX_TOKENS)
        {
            tokenIds = tokenIds.Skip(tokenIds.Count - (CONTEXT_SIZE - MAX_TOKENS)).ToList();
        }

        int promptTokenCount = tokenIds.Count;

        for (int i = 0; i < MAX_TOKENS; i++)
        {
            if (tokenIds.Count >= CONTEXT_SIZE) break;

            int[] fixedTokens = new int[CONTEXT_SIZE];
            int[] fixedAttention = new int[CONTEXT_SIZE];

            for (int j = 0; j < CONTEXT_SIZE; j++)
            {
                if (j < tokenIds.Count)
                {
                    fixedTokens[j] = tokenIds[j];
                    fixedAttention[j] = 1;
                }
                else
                {
                    fixedTokens[j] = 50256;
                    fixedAttention[j] = 0;
                }
            }

            var nativeTokensData = new NativeArray<int>(fixedTokens, Allocator.TempJob);
            var inputTensor = new Tensor<int>(new TensorShape(1, CONTEXT_SIZE), nativeTokensData);

            var nativeAttentionData = new NativeArray<int>(fixedAttention, Allocator.TempJob);
            var attentionTensor = new Tensor<int>(new TensorShape(1, CONTEXT_SIZE), nativeAttentionData);

            worker.SetInput("input_ids", inputTensor);
            worker.SetInput("attention_mask", attentionTensor);

            worker.Schedule();
            yield return null;

            var outputTensor = worker.PeekOutput(runtimeModel.outputs[0].name) as Tensor<float>;

            int nextTokenId = GetNextTokenArgMax(outputTensor, tokenIds.Count, tokenIds);

            inputTensor.Dispose();
            nativeTokensData.Dispose();
            attentionTensor.Dispose();
            nativeAttentionData.Dispose();

            if (nextTokenId == 50256) break;

            tokenIds.Add(nextTokenId);
        }

        var generatedTokens = tokenIds.Skip(promptTokenCount).ToArray();
        string rawAnswer = tokenizer.Decode(generatedTokens).Trim();

        // 1. Очистка BPE-токенов и битой кодировки (Â®, â‡ и т.д.)
        string cleanAnswer = rawAnswer.Replace("Ġ", " ").Replace("Ċ", "\n");
        cleanAnswer = Regex.Replace(cleanAnswer, @"[^\u0000-\u007F]+", ""); // Удаляем битые не-ASCII символы

        // 2. Если модель всё равно понесло в системные дампы (0x7f..., ProcessorState) или она выдала пустоту
        if (cleanAnswer.Contains("0x") || cleanAnswer.Contains("ProcessorState") || string.IsNullOrWhiteSpace(cleanAnswer))
        {
            if (prompt.Contains("NOT INSTALLED"))
            {
                cleanAnswer = "Cannot boot system! One or more critical hardware components are missing from the motherboard.";
            }
            else
            {
                cleanAnswer = "Hardware check complete. All components are compatible and ready to operation.";
            }
        }

        Debug.Log($"<color=cyan>ИИ Вердикт:</color> {cleanAnswer}");
        onComplete?.Invoke(cleanAnswer);
    }

    int GetNextTokenArgMax(Tensor<float> logits, int currentSequenceLength, List<int> previousTokens)
    {
        if (logits == null) return 50256;

        using NativeArray<float> logitsArray = logits.DownloadToNativeArray();
        int actualVocabSize = logits.shape[logits.shape.rank - 1];

        int tokenIndex = currentSequenceLength - 1;
        int offset = tokenIndex * actualVocabSize;

        float maxVal = -float.MaxValue;
        int maxIndex = 0;

        HashSet<int> penaltySet = new HashSet<int>(previousTokens);
        float repetitionPenalty = 1.5f;

        for (int i = 0; i < actualVocabSize; i++)
        {
            float val = logitsArray[offset + i];

            if (penaltySet.Contains(i))
            {
                if (val > 0) val /= repetitionPenalty;
                else val *= repetitionPenalty;
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