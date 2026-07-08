using UnityEngine;

// Подключаем библиотеку для изменения интерфейса самого Unity
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

public enum PartType
{
    CPU, GPU, Motherboard, RAM, PSU, Cooler, Storage, Case
}

public class PcPart : MonoBehaviour
{
    public PartType type;
    public int id;

    public void LogConnection(string realPartName)
    {
        Debug.Log("Коннект прошел как надо! Воткнули: " + type + ". Реальное название из базы: " + realPartName);
    }
}

// --- ИЗМЕНЕНИЕ ИНТЕРФЕЙСА UNITY ---
#if UNITY_EDITOR
[CustomEditor(typeof(PcPart))]
public class PcPartEditor : Editor
{
    private string[] options;
    private int[] ids;

    public override void OnInspectorGUI()
    {
        PcPart part = (PcPart)target;

        // Загружаем данные при первом клике на болванку
        if (options == null) LoadJsonData(part.type);

        // Отрисовка выбора типа детали
        EditorGUI.BeginChangeCheck();
        PartType newType = (PartType)EditorGUILayout.EnumPopup("Type", part.type);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(part, "Change Type");
            part.type = newType;
            LoadJsonData(part.type);

            // Жестко перезаписываем ID на первый из нового списка
            if (ids != null && ids.Length > 0) part.id = ids[0];
        }

        // Отрисовка выпадающего списка с ID и названиями
        if (options != null && options.Length > 0)
        {
            int currentIndex = 0;
            for (int i = 0; i < ids.Length; i++)
            {
                if (ids[i] == part.id) currentIndex = i;
            }

            EditorGUI.BeginChangeCheck();
            currentIndex = EditorGUILayout.Popup("ID (из базы)", currentIndex, options);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(part, "Change ID");
                part.id = ids[currentIndex];
            }
        }
        else
        {
            // Если файла почему-то нет, оставляем обычный ввод цифр
            part.id = EditorGUILayout.IntField("ID (Ручной ввод)", part.id);
        }
    }

    // Читаем файлы напрямую в обход старта игры
    private void LoadJsonData(PartType type)
    {
        string fileName = "";
        switch (type)
        {
            case PartType.CPU: fileName = "cpu.json"; break;
            case PartType.GPU: fileName = "gpu.json"; break;
            case PartType.Motherboard: fileName = "motherboard.json"; break;
            case PartType.RAM: fileName = "ram.json"; break;
            case PartType.PSU: fileName = "psu.json"; break;
            case PartType.Cooler: fileName = "coolers.json"; break;
            case PartType.Storage: fileName = "storage.json"; break;
            case PartType.Case: fileName = "cases.json"; break;
        }

        string path = Path.Combine(Application.streamingAssetsPath, fileName);
        if (!File.Exists(path)) { options = null; return; }

        string json = File.ReadAllText(path);

        // Парсим конкретный массив, чтобы вытащить оттуда имена для красивого списка
        switch (type)
        {
            case PartType.CPU: var c = JsonUtility.FromJson<CpuList>(json).items; ids = new int[c.Length]; options = new string[c.Length]; for (int i = 0; i < c.Length; i++) { ids[i] = c[i].id; options[i] = c[i].id + " - " + c[i].name; } break;
            case PartType.GPU: var g = JsonUtility.FromJson<GpuList>(json).items; ids = new int[g.Length]; options = new string[g.Length]; for (int i = 0; i < g.Length; i++) { ids[i] = g[i].id; options[i] = g[i].id + " - " + g[i].name; } break;
            case PartType.Motherboard: var m = JsonUtility.FromJson<MotherboardList>(json).items; ids = new int[m.Length]; options = new string[m.Length]; for (int i = 0; i < m.Length; i++) { ids[i] = m[i].id; options[i] = m[i].id + " - " + m[i].name; } break;
            case PartType.RAM: var r = JsonUtility.FromJson<RamList>(json).items; ids = new int[r.Length]; options = new string[r.Length]; for (int i = 0; i < r.Length; i++) { ids[i] = r[i].id; options[i] = r[i].id + " - " + r[i].name; } break;
            case PartType.PSU: var p = JsonUtility.FromJson<PsuList>(json).items; ids = new int[p.Length]; options = new string[p.Length]; for (int i = 0; i < p.Length; i++) { ids[i] = p[i].id; options[i] = p[i].id + " - " + p[i].name; } break;
            case PartType.Cooler: var co = JsonUtility.FromJson<CoolerList>(json).items; ids = new int[co.Length]; options = new string[co.Length]; for (int i = 0; i < co.Length; i++) { ids[i] = co[i].id; options[i] = co[i].id + " - " + co[i].name; } break;
            case PartType.Storage: var s = JsonUtility.FromJson<StorageList>(json).items; ids = new int[s.Length]; options = new string[s.Length]; for (int i = 0; i < s.Length; i++) { ids[i] = s[i].id; options[i] = s[i].id + " - " + s[i].name; } break;
            case PartType.Case: var ca = JsonUtility.FromJson<CaseList>(json).items; ids = new int[ca.Length]; options = new string[ca.Length]; for (int i = 0; i < ca.Length; i++) { ids[i] = ca[i].id; options[i] = ca[i].id + " - " + ca[i].name; } break;
        }
    }
}
#endif