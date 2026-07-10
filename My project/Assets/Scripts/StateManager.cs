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

        // Проверка состояния пасты (чтобы ИИ мог поругать, если игрок забыл её нанести)
        prompt += "\nТермопаста: " + (isThermalPasteApplied ? "Намазана" : "Отсутствует (забыл намазать)") + "\n";

        return prompt;
    }

    // Тестовый вызов: по нажатию на пробел выводим готовый текст в консоль
    // Убираем кривой Update и делаем удобную кнопку в инспекторе
    [ContextMenu("Test Generate Prompt")]
    public void TestPrompt()
    {
        Debug.Log("--- ГОТОВЫЙ ЗАПРОС ДЛЯ ИИ ---\n" + GeneratePromptForAI());
    }
}