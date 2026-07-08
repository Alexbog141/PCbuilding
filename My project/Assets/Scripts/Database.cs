using System.IO;
using UnityEngine;

// --- СТРУКТУРЫ ДАННЫХ ---
// (Оставил их компактными, так как это просто контейнеры для переменных)

[System.Serializable]
public class CpuData { public int id; public string name; public int socketId; public int price; public int tdp; }
[System.Serializable]
public class CpuList { public CpuData[] items; }

[System.Serializable]
public class GpuData { public int id; public string name; public int vramGb; public int tdp; public int lengthMm; public bool requiresAtx3; public int price; }
[System.Serializable]
public class GpuList { public GpuData[] items; }

[System.Serializable]
public class MotherboardData { public int id; public string name; public int socketId; public string formFactor; public string supportedRamType; public int price; }
[System.Serializable]
public class MotherboardList { public MotherboardData[] items; }

[System.Serializable]
public class RamData { public int id; public string name; public string type; public int capacityGb; public int price; }
[System.Serializable]
public class RamList { public RamData[] items; }

[System.Serializable]
public class PsuData { public int id; public string name; public int power; public bool isAtx3; public int price; }
[System.Serializable]
public class PsuList { public PsuData[] items; }

[System.Serializable]
public class CoolerData { public int id; public string name; public int maxTdp; public int price; }
[System.Serializable]
public class CoolerList { public CoolerData[] items; }

[System.Serializable]
public class StorageData { public int id; public string name; public string type; public int capacityGb; public int price; }
[System.Serializable]
public class StorageList { public StorageData[] items; }

[System.Serializable]
public class CaseData { public int id; public string name; public int maxGpuLengthMm; public int price; }
[System.Serializable]
public class CaseList { public CaseData[] items; }


// --- ОСНОВНОЙ КЛАСС БАЗЫ ДАННЫХ ---

public class Database : MonoBehaviour
{
    public CpuData[] cpu;
    public GpuData[] gpu;
    public MotherboardData[] motherboard;
    public RamData[] ram;
    public PsuData[] psu;
    public CoolerData[] cooler;
    public StorageData[] storage;
    public CaseData[] cases;

    void Awake()
    {
        // Теперь всё читается легко и понятно
        cpu = LoadJsonData<CpuList>("cpu.json").items;
        gpu = LoadJsonData<GpuList>("gpu.json").items;
        motherboard = LoadJsonData<MotherboardList>("motherboard.json").items;
        ram = LoadJsonData<RamList>("ram.json").items;
        psu = LoadJsonData<PsuList>("psu.json").items;
        cooler = LoadJsonData<CoolerList>("coolers.json").items;
        storage = LoadJsonData<StorageList>("storage.json").items;
        cases = LoadJsonData<CaseList>("cases.json").items;

        Debug.Log($"База загружена! Процов: {cpu.Length}, Видюх: {gpu.Length}, Матерей: {motherboard.Length}, ОЗУ: {ram.Length}, БП: {psu.Length}, Кулеров: {cooler.Length}, Дисков: {storage.Length}, Корпусов: {cases.Length}");
    }

    // Тот самый вспомогательный метод. Написан строго по факту.
    // <T> означает, что он может принимать любой из наших списков (CpuList, GpuList и т.д.)
    private T LoadJsonData<T>(string fileName)
    {
        // 1. Формируем путь
        string path = Path.Combine(Application.streamingAssetsPath, fileName);

        // 2. Читаем текст
        string jsonText = File.ReadAllText(path);

        // 3. Парсим и возвращаем результат
        return JsonUtility.FromJson<T>(jsonText);
    }
}