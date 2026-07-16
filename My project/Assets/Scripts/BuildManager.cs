using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections; // Обязательно нужно для IEnumerator

public class BuildManager : MonoBehaviour
{
    public Database db;
    public StateManager stateManager;

    [Header("Связь с ИИ")]
    public AIController aiController;

    private List<PartType> installedParts = new List<PartType>();

    public void OnPartInstalled(PcPart installedPart)
    {
        if (!CheckBuildOrder(installedPart.type))
        {
            Debug.LogWarning($"<color=red>Ошибка сборки:</color> Рано ставить {installedPart.type}.");
            return;
        }

        switch (installedPart.type)
        {
            case PartType.CPU:
                for (int i = 0; i < db.cpu.Length; i++)
                {
                    if (db.cpu[i].id == installedPart.id)
                    {
                        stateManager.installedCpu = $"{db.cpu[i].name} (Сокет: {db.cpu[i].socketId}, TDP: {db.cpu[i].tdp}W)";
                        installedPart.LogConnection(db.cpu[i].name);
                        break;
                    }
                }
                break;

            case PartType.GPU:
                for (int i = 0; i < db.gpu.Length; i++)
                {
                    if (db.gpu[i].id == installedPart.id)
                    {
                        stateManager.installedGpu = $"{db.gpu[i].name} (VRAM: {db.gpu[i].vramGb}GB, TDP: {db.gpu[i].tdp}W)";
                        installedPart.LogConnection(db.gpu[i].name);
                        break;
                    }
                }
                break;

            case PartType.Motherboard:
                for (int i = 0; i < db.motherboard.Length; i++)
                {
                    if (db.motherboard[i].id == installedPart.id)
                    {
                        stateManager.installedMotherboard = $"{db.motherboard[i].name} (Сокет: {db.motherboard[i].socketId})";
                        installedPart.LogConnection(db.motherboard[i].name);
                        break;
                    }
                }
                break;

            case PartType.RAM:
                for (int i = 0; i < db.ram.Length; i++)
                {
                    if (db.ram[i].id == installedPart.id)
                    {
                        stateManager.installedRam = $"{db.ram[i].name} (Объем: {db.ram[i].capacityGb}GB)";
                        installedPart.LogConnection(db.ram[i].name);
                        break;
                    }
                }
                break;

            case PartType.PSU:
                for (int i = 0; i < db.psu.Length; i++)
                {
                    if (db.psu[i].id == installedPart.id)
                    {
                        stateManager.installedPsu = $"{db.psu[i].name} (Мощность: {db.psu[i].power}W)";
                        installedPart.LogConnection(db.psu[i].name);
                        break;
                    }
                }
                break;

            case PartType.Cooler:
                for (int i = 0; i < db.cooler.Length; i++)
                {
                    if (db.cooler[i].id == installedPart.id)
                    {
                        stateManager.installedCooler = $"{db.cooler[i].name} (TDP: {db.cooler[i].maxTdp}W)";
                        installedPart.LogConnection(db.cooler[i].name);
                        break;
                    }
                }
                break;
            case PartType.Storage:
                for (int i = 0; i < db.storage.Length; i++)
                {
                    if (db.storage[i].id == installedPart.id)
                    {
                        stateManager.installedStorage = $"{db.storage[i].name} (Объем: {db.storage[i].capacityGb}GB)";
                        installedPart.LogConnection(db.storage[i].name);
                        break;
                    }
                }
                break;

            case PartType.Case:
                for (int i = 0; i < db.cases.Length; i++)
                {
                    if (db.cases[i].id == installedPart.id)
                    {
                        stateManager.installedCase = $"{db.cases[i].name} (Макс. длина GPU: {db.cases[i].maxGpuLengthMm}мм)";
                        installedPart.LogConnection(db.cases[i].name);
                        break;
                    }
                }
                break;
        }

        if (!installedParts.Contains(installedPart.type))
        {
            installedParts.Add(installedPart.type);
        }
    }

    private bool CheckBuildOrder(PartType newPartType)
    {
        // 1. Материнская плата должна быть первой
        if (newPartType == PartType.Motherboard)
            return installedParts.Count == 0;

        // 2. Процессор ставится только после материнской платы
        if (newPartType == PartType.CPU)
            return installedParts.Contains(PartType.Motherboard);

        // 3. Оперативная память ставится после процессора
        if (newPartType == PartType.RAM)
            return installedParts.Contains(PartType.CPU);

        // 4. Кулер ставится после оперативной памяти
        if (newPartType == PartType.Cooler)
            return installedParts.Contains(PartType.RAM);

        // 5. Видеокарта ставится последней, после кулера
        if (newPartType == PartType.GPU)
            return installedParts.Contains(PartType.Cooler);


        return true;
    }

    public void FinishBuildAndCheckAI()
    {
        if (aiController == null) { Debug.LogError("AIController не назначен!"); return; }
        if (stateManager == null) { Debug.LogError("StateManager не назначен!"); return; }
        string prompt = $"Analyze PC build compatibility strictly based on sockets and TDP. MB: {stateManager.installedMotherboard}, CPU: {stateManager.installedCpu}, GPU: {stateManager.installedGpu}, PSU: {stateManager.installedPsu}. Verdict:";
    }

    private void OnAIResponseReceived(string finalAnswer)
    {
        Debug.Log($"<color=yellow>Оценка от ИИ:</color> {finalAnswer}");
    }

    // --- ВАШ ТЕСТОВЫЙ БЛОК ---
 
}