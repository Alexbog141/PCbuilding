using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class BuildTester : MonoBehaviour
{
    // Заглушка для списка установленных деталей
    private List<string> installedParts = new List<string>();

    void Start()
    {
        RunTests();
    }

    private void RunTests()
    {
        Debug.Log("--- ЗАПУСК ТЕСТОВ ПАЙПЛАЙНА ---");

        // Тест 1: Попытка поставить CPU до материнской платы
        bool test1Result = CheckBuildOrder("CPU");
        Debug.Log(test1Result == false
            ? "<color=green>Тест 1 пройден:</color> Установка CPU без Motherboard заблокирована."
            : "<color=red>Тест 1 провален:</color> Ошибка логики.");

        // Устанавливаем материнскую плату для следующего теста
        installedParts.Add("Motherboard");

        // Тест 2: Попытка поставить CPU после материнской платы
        bool test2Result = CheckBuildOrder("CPU");
        Debug.Log(test2Result == true
            ? "<color=green>Тест 2 пройден:</color> Установка CPU разрешена."
            : "<color=red>Тест 2 провален:</color> Ошибка логики.");

        // Тест 3: Фильтрация ответа от нейросети
        string rawAIResponse = "DIGITAL_CORE=0x1E9A8AC6ED7EC4AF3AB2BD88AA10\nI'm not sure if I can get the CPU to work with this, but it's a good idea";
        string cleanResponse = CleanAIOutput(rawAIResponse);

        Debug.Log($"<color=cyan>[ИИ Вердикт (до фильтра)]:</color> {rawAIResponse}");
        Debug.Log($"<color=green>[Тестер получил ответ от ИИ (после фильтра)]:</color> {cleanResponse}");
    }

    // Метод очистки мусора от LLM
    private string CleanAIOutput(string rawText)
    {
        // Ищем строку DIGITAL_CORE= и любой 16-ричный код после нее, затем удаляем
        string cleanedText = Regex.Replace(rawText, @"DIGITAL_CORE=0x[a-fA-F0-9]+", "");

        // Убираем пустые строки и лишние пробелы по краям
        return cleanedText.Trim();
    }

    // Упрощенная логика проверки из твоего бэкенда
    private bool CheckBuildOrder(string newPartType)
    {
        if (newPartType == "Motherboard")
            return installedParts.Count == 0;

        if (newPartType == "CPU")
            return installedParts.Contains("Motherboard");

        return true;
    }
}