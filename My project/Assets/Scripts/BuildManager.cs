using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine.InputSystem;

public class BuildManager : MonoBehaviour
{
    public Database db;
    public StateManager stateManager;

    [Header("Связь с ИИ")]
    public AIController aiController;

    [Header("Статус кабелей")]
    public bool isCable24PinConnected = false;
    public bool isCable8PinCpuConnected = false;
    public bool isCable8PinGpuConnected = false;

    private List<PartType> installedParts = new List<PartType>();

    // --- МЕТОД ДЛЯ РЕГИСТРАЦИИ ПОДКЛЮЧЕНИЯ КАБЕЛЕЙ ---
    public void OnCableConnected(string cableID)
    {
        if (cableID == "connector_24pin")
        {
            isCable24PinConnected = true;
            Debug.Log("<color=green>[BuildManager]:</color> Кабель 24-pin успешно подключен!");
        }
        else if (cableID == "connector_8pin")
        {
            // Так как у нас два кабеля 8-pin (на проц и видюху), 
            // сначала подключаем процессорный, потом видеокарту
            if (!isCable8PinCpuConnected)
            {
                isCable8PinCpuConnected = true;
                Debug.Log("<color=green>[BuildManager]:</color> Кабель питания CPU (8-pin) подключен!");
            }
            else
            {
                isCable8PinGpuConnected = true;
                Debug.Log("<color=green>[BuildManager]:</color> Кабель питания GPU (8-pin) подключен!");
            }
        }

        // Тут можно сразу передавать инфу в StateManager, если у него есть под это поля,
        // чтобы ИИ тоже видел статус проводов
    }

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

    public bool RequestInstallation(PcPart part, int motherboardId)
    {
        if (!CheckBuildOrder(part.type))
        {
            Debug.LogWarning($"<color=red>[BuildManager]:</color> Нарушен порядок! Рано ставить {part.type}.");
            return false;
        }

        if (part.type == PartType.CPU)
        {
            MotherboardData currentMotherboard = null;
            CpuData targetCpu = null;

            foreach (var mb in db.motherboard)
            {
                if (mb.id == motherboardId) { currentMotherboard = mb; break; }
            }

            foreach (var cpu in db.cpu)
            {
                if (cpu.id == part.id) { targetCpu = cpu; break; }
            }

            if (currentMotherboard != null && targetCpu != null)
            {
                if (currentMotherboard.socketId != targetCpu.socketId)
                {
                    Debug.LogError($"<color=red>[BuildManager]:</color> Процессор (Сокет {targetCpu.socketId}) не подходит к материнке (Сокет {currentMotherboard.socketId})!");
                    return false;
                }
            }
        }

        OnPartInstalled(part);
        return true;
    }

    private bool CheckBuildOrder(PartType newPartType)
    {
        if (newPartType == PartType.Motherboard) return installedParts.Count == 0;
        if (newPartType == PartType.CPU) return installedParts.Contains(PartType.Motherboard);
        if (newPartType == PartType.RAM) return installedParts.Contains(PartType.CPU);
        if (newPartType == PartType.Cooler) return installedParts.Contains(PartType.RAM);
        if (newPartType == PartType.GPU) return installedParts.Contains(PartType.Cooler);

        return true;
    }

    public void FinishBuildAndCheckAI()
    {
        if (aiController == null) { Debug.LogError("AIController не назначен!"); return; }
        if (stateManager == null) { Debug.LogError("StateManager не назначен!"); return; }

        // Жесткая проверка: если кабели не подключены, ИИ даже не опрашиваем
        if (!isCable24PinConnected || !isCable8PinCpuConnected)
        {
            Debug.LogWarning("<color=orange>[BuildManager]:</color> Нельзя запустить ПК! Не подключены основные кабели питания.");
            return;
        }

        string prompt = stateManager.GeneratePromptForAI();
        aiController.CheckCompatibility(prompt, OnAIResponseReceived);
    }

    private void OnAIResponseReceived(string finalAnswer)
    {
        Debug.Log($"<color=yellow>Оценка от ИИ:</color> {finalAnswer}");
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
        {
            Debug.Log("Отправляю запрос к ИИ по кнопке Enter...");
            FinishBuildAndCheckAI();
        }
    }
}