using UnityEngine;

public class StateManager : MonoBehaviour
{
    // Переменные для хранения названий установленных деталей
    public string installedCpu = "";
    public string installedGpu = "";
    public string installedMotherboard = "";
    public string installedRam = "";
    public string installedPsu = "";
    public string installedCooler = "";
    public string installedStorage = "";
    public string installedCase = "";

    // Переменная для термопасты (по умолчанию не намазана)
    public bool isThermalPasteApplied = false;

    private BuildManager buildManager;

    private void Start()
    {
        // Находим BuildManager на сцене при старте
        buildManager = FindAnyObjectByType<BuildManager>();
    }

    // Метод генерации итогового текста для нейронки (промпта)
    public string GeneratePromptForAI()
    {
        // Системный промпт — задаем роль ИИ
        string prompt = "Ты саркастичный, но профессиональный эксперт по сборке ПК. Твоя задача — оценить конфигурацию компьютера, которую собрал пользователь. Оцени баланс сборки, проверь совместимость (если хватает данных) и укажи на ошибки. Если сборка хорошая — скупо похвали, если плохая — разнеси по фактам.\n\n";

        prompt += "Конфигурация ПК, которую собрал игрок:\n";

        // Добавляем в текст только те детали, которые игрок реально поставил
        if (!string.IsNullOrEmpty(installedCpu)) prompt += "- Процессор: " + installedCpu + "\n";
        if (!string.IsNullOrEmpty(installedMotherboard)) prompt += "- Материнская плата: " + installedMotherboard + "\n";
        if (!string.IsNullOrEmpty(installedRam)) prompt += "- Оперативная память: " + installedRam + "\n";
        if (!string.IsNullOrEmpty(installedGpu)) prompt += "- Видеокарта: " + installedGpu + "\n";
        if (!string.IsNullOrEmpty(installedPsu)) prompt += "- Блок питания: " + installedPsu + "\n";
        if (!string.IsNullOrEmpty(installedCooler)) prompt += "- Охлаждение: " + installedCooler + "\n";
        if (!string.IsNullOrEmpty(installedStorage)) prompt += "- Накопитель: " + installedStorage + "\n";
        if (!string.IsNullOrEmpty(installedCase)) prompt += "- Корпус: " + installedCase + "\n";

        // Проверка состояния пасты
        prompt += "\nТермопаста: " + (isThermalPasteApplied ? "Намазана" : "Отсутствует (забыл намазать)") + "\n";

        // --- ДОБАВЛЕНИЕ ПРОВЕРКИ КАБЕЛЕЙ ДЛЯ ИИ ---
        prompt += "\nПодключение кабелей питания:\n";
        if (buildManager != null)
        {
            prompt += "- Кабель 24-pin материнки: " + (buildManager.isCable24PinConnected ? "Подключен" : "НЕ подключен!") + "\n";
            prompt += "- Кабель 8-pin процессора: " + (buildManager.isCable8PinCpuConnected ? "Подключен" : "НЕ подключен!") + "\n";
            prompt += "- Кабель 8-pin видеокарты: " + (buildManager.isCable8PinGpuConnected ? "Подключен" : "НЕ подключен (если GPU установлена)") + "\n";
        }
        else
        {
            prompt += "- Данные о кабелях недоступны (ошибка BuildManager)\n";
        }

        return prompt;
    }

    [ContextMenu("Test Generate Prompt")]
    public void TestPrompt()
    {
        // Перед тестом пытаемся найти менеджер, если в редакторе не нажали Play
        if (buildManager == null) buildManager = FindAnyObjectByType<BuildManager>();
        Debug.Log("--- ГОТОВЫЙ ЗАПРОС ДЛЯ ИИ ---\n" + GeneratePromptForAI());
    }
}