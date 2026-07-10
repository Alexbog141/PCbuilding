using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public Database db;
    public StateManager stateManager;

    public void OnPartInstalled(PcPart installedPart)
    {
        switch (installedPart.type)
        {
            case PartType.CPU:
                for (int i = 0; i < db.cpu.Length; i++)
                {
                    if (db.cpu[i].id == installedPart.id) {
                        stateManager.installedCpu = db.cpu[i].name;
                        installedPart.LogConnection(db.cpu[i].name); break; }
                }
                break;

            case PartType.GPU:
                for (int i = 0; i < db.gpu.Length; i++)
                {
                    if (db.gpu[i].id == installedPart.id) {
                        stateManager.installedGpu = db.gpu[i].name;
                        installedPart.LogConnection(db.gpu[i].name); break; }
                }
                break;

            case PartType.Motherboard:
                for (int i = 0; i < db.motherboard.Length; i++)
                {
                    if (db.motherboard[i].id == installedPart.id) {
                        stateManager.installedMotherboard = db.motherboard[i].name;
                        installedPart.LogConnection(db.motherboard[i].name); break; }
                }
                break;

            case PartType.RAM:
                for (int i = 0; i < db.ram.Length; i++)
                {
                    if (db.ram[i].id == installedPart.id) {
                        stateManager.installedRam = db.ram[i].name;
                        installedPart.LogConnection(db.ram[i].name); break; }
                }
                break;

            case PartType.PSU:
                for (int i = 0; i < db.psu.Length; i++)
                {
                    if (db.psu[i].id == installedPart.id) {
                        stateManager.installedPsu = db.psu[i].name;
                        installedPart.LogConnection(db.psu[i].name); break; }
                }
                break;

            case PartType.Cooler:
                for (int i = 0; i < db.cooler.Length; i++)
                {
                    if (db.cooler[i].id == installedPart.id) {
                        stateManager.installedCooler = db.cooler[i].name;
                        installedPart.LogConnection(db.cooler[i].name); break; }
                }
                break;

            case PartType.Storage:
                for (int i = 0; i < db.storage.Length; i++)
                {
                    if (db.storage[i].id == installedPart.id) {
                        stateManager.installedStorage = db.storage[i].name;
                        installedPart.LogConnection(db.storage[i].name); break; }
                }
                break;

            case PartType.Case:
                for (int i = 0; i < db.cases.Length; i++)
                {
                    if (db.cases[i].id == installedPart.id) {
                        stateManager.installedCase = db.cases[i].name;
                        installedPart.LogConnection(db.cases[i].name); break; }
                }
                break;
        }
    }

    // Тестовый запуск
    // Тестовый запуск для ВСЕХ деталей на сцене
    // Тестовый запуск для ВСЕХ деталей на сцене
    void Start()
    {
        // Используем новый метод и жестко отключаем сортировку для скорости
        PcPart[] allTestParts = FindObjectsByType<PcPart>(FindObjectsSortMode.None);

        // Перебираем каждый и отправляем на проверку
        for (int i = 0; i < allTestParts.Length; i++)
        {
            OnPartInstalled(allTestParts[i]);
        }
    }
}