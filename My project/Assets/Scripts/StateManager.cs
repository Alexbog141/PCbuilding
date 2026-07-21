using UnityEngine;

public class StateManager : MonoBehaviour
{
    // Переменные для деталей (корпус выпилили)
    public string installedCpu = "";
    public string installedGpu = "";
    public string installedMotherboard = "";
    public string installedRam = "";
    public string installedPsu = "";
    public string installedCooler = "";
    public string installedStorage = "";

    public bool isThermalPasteApplied = false;

    private BuildManager buildManager;

    private void Start()
    {
        buildManager = FindAnyObjectByType<BuildManager>();
    }

    public string GeneratePromptForAI()
    {
        // 1. Описываем роль и правила для модели
        string prompt = "Task: Evaluate PC assembly state. Provide a clear status verdict.\n";
        prompt += "Rule: If critical hardware (Motherboard, CPU, RAM, PSU) is missing, mark system as INCOMPLETE.\n\n";

        // 2. Передаем состояние деталей
        prompt += "Current Hardware:\n";
        prompt += "- Motherboard: " + (string.IsNullOrEmpty(installedMotherboard) ? "NOT INSTALLED" : installedMotherboard) + "\n";
        prompt += "- CPU: " + (string.IsNullOrEmpty(installedCpu) ? "NOT INSTALLED" : installedCpu) + "\n";
        prompt += "- RAM: " + (string.IsNullOrEmpty(installedRam) ? "NOT INSTALLED" : installedRam) + "\n";
        prompt += "- GPU: " + (string.IsNullOrEmpty(installedGpu) ? "NOT INSTALLED (Integrated graphics)" : installedGpu) + "\n";
        prompt += "- Power Supply: " + (string.IsNullOrEmpty(installedPsu) ? "NOT INSTALLED" : installedPsu) + "\n";
        prompt += "- Cooler: " + (string.IsNullOrEmpty(installedCooler) ? "NOT INSTALLED" : installedCooler) + "\n";
        prompt += "- Thermal Paste: " + (isThermalPasteApplied ? "Applied" : "Missing") + "\n";

        if (buildManager != null)
        {
            prompt += "- 24-pin Power Cable: " + (buildManager.isCable24PinConnected ? "Connected" : "Disconnected") + "\n";
            prompt += "- CPU Power Cable: " + (buildManager.isCable8PinCpuConnected ? "Connected" : "Disconnected") + "\n";
        }

        // 3. Задаем начало ответа (рельсы для генерации)
        bool hasCoreParts = !string.IsNullOrEmpty(installedMotherboard) &&
                            !string.IsNullOrEmpty(installedCpu) &&
                            !string.IsNullOrEmpty(installedRam) &&
                            !string.IsNullOrEmpty(installedPsu);

        if (!hasCoreParts)
        {
            // Если не хватает базы — наталкиваем модель на статус ошибки
            prompt += "\nSystem Diagnosis: Cannot boot! Critical components are missing.";
        }
        else
        {
            // Если база есть — наталкиваем модель на полноценный вердикт
            prompt += "\nSystem Diagnosis: Hardware setup checked. All required components are installed.";
        }

        return prompt;
    }

    [ContextMenu("Test Generate Prompt")]
    public void TestPrompt()
    {
        if (buildManager == null) buildManager = FindAnyObjectByType<BuildManager>();
        Debug.Log("--- ГОТОВЫЙ ЗАПРОС ДЛЯ ИИ ---\n" + GeneratePromptForAI());
    }
}