using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TabletSpawnManager : MonoBehaviour
{
    [Header("Ссылки на базовые системы")]
    public Database db;

    [Header("Точка появления деталей перед игроком")]
    public Transform spawnPoint;

    [Header("Ссылка на HardwarePool")]
    public Transform hardwarePool;

    [Header("Ссылка на текстовое поле экрана ИИ")]
    public TextMeshProUGUI aiDisplayText;

    [Header("Динамическое меню")]
    public GameObject buttonPrefab;
    public Transform container;

    private int buttonCount = 0;

    void Start()
    {
        GenerateAllButtons();
    }

    void GenerateAllButtons()
    {
        if (db == null || buttonPrefab == null || container == null) return;

        buttonCount = 0;
        foreach (Transform child in container) { Destroy(child.gameObject); }

        foreach (var item in db.cpu) { CreateButton("CPU", item.id, item.name); }
        foreach (var item in db.gpu) { CreateButton("GPU", item.id, item.name); }
        foreach (var item in db.ram) { CreateButton("RAM", item.id, item.name); }
        foreach (var item in db.motherboard) { CreateButton("Motherboard", item.id, item.name); }
        foreach (var item in db.psu) { CreateButton("PSU", item.id, item.name); }
        foreach (var item in db.cooler) { CreateButton("Cooler", item.id, item.name); }
        foreach (var item in db.storage) { CreateButton("Storage", item.id, item.name); }
        foreach (var item in db.cases) { CreateButton("Case", item.id, item.name); }

        // Автоматически растягиваем Content вниз, чтобы скролл работал
        RectTransform containerRt = container.GetComponent<RectTransform>();
        if (containerRt != null)
        {
            float totalHeight = (buttonCount * 65f) + 10f; // 55 высота + 10 отступ
            containerRt.sizeDelta = new Vector2(containerRt.sizeDelta.x, totalHeight);
        }
    }

    void CreateButton(string partType, int id, string partName)
    {
        GameObject newBtnObj = Instantiate(buttonPrefab, container);

        // --- ПРИНУДИТЕЛЬНАЯ МАТЕМАТИКА РАЗМЕТКИ ---
        RectTransform rt = newBtnObj.GetComponent<RectTransform>();
        if (rt != null)
        {
            // Жестко вяжем к верхнему краю
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);

            float buttonHeight = 55f;
            float spacing = 10f;

            // Считаем отступ сверху вниз
            float posY = -spacing - (buttonCount * (buttonHeight + spacing));

            // Ставим позицию и размеры (-20 по ширине — это отступы по краям)
            rt.anchoredPosition = new Vector2(0, posY);
            rt.sizeDelta = new Vector2(-20, buttonHeight);
        }
        buttonCount++;
        // ------------------------------------------

        TextMeshProUGUI btnText = newBtnObj.GetComponentInChildren<TextMeshProUGUI>();
        if (btnText != null) btnText.text = partName;

        Button btn = newBtnObj.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(() => SpawnComponentFromTablet(partType, id));
        }
    }

    public void SpawnComponentFromTablet(string stringPartType, int partId)
    {
        if (!System.Enum.TryParse(stringPartType, true, out PartType type)) return;

        string objectNameInPool = $"{type}_{partId}";
        Transform targetComponent = hardwarePool.Find(objectNameInPool);

        if (targetComponent != null)
        {
            targetComponent.position = spawnPoint.position;
            targetComponent.rotation = spawnPoint.rotation;

            Rigidbody rb = targetComponent.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            targetComponent.gameObject.SetActive(true);
            UpdateTabletScreen(type, partId);
        }
        else
        {
            if (aiDisplayText != null)
                aiDisplayText.text = $"<color=red>Ошибка:</color> Объект '{objectNameInPool}' не найден в пуле.";
        }
    }

    private void UpdateTabletScreen(PartType type, int id)
    {
        if (aiDisplayText == null) return;
        string title = "<b>[ИИ СИСТЕМА]: Анализ комплектующего...</b>\n\n";
        string content = "";

        switch (type)
        {
            case PartType.CPU:
                foreach (var item in db.cpu) if (item.id == id) { content = $"<b>Процессор:</b> {item.name}\n- Сокет ID: {item.socketId}\n- TDP: {item.tdp}W\n- Цена: {item.price} руб."; break; }
                break;
            case PartType.GPU:
                foreach (var item in db.gpu) if (item.id == id) { content = $"<b>Видеокарта:</b> {item.name}\n- VRAM: {item.vramGb}GB\n- Длина: {item.lengthMm}мм\n- TDP: {item.tdp}W\n- Цена: {item.price} руб."; break; }
                break;
            case PartType.RAM:
                foreach (var item in db.ram) if (item.id == id) { content = $"<b>ОЗУ:</b> {item.name}\n- Тип: {item.type}\n- Объем: {item.capacityGb}GB\n- Цена: {item.price} руб."; break; }
                break;
            case PartType.Motherboard:
                foreach (var item in db.motherboard) if (item.id == id) { content = $"<b>Материнская плата:</b> {item.name}\n- Сокет ID: {item.socketId}\n- Форм-фактор: {item.formFactor}\n- ОЗУ: {item.supportedRamType}\n- Цена: {item.price} руб."; break; }
                break;
            case PartType.PSU:
                foreach (var item in db.psu) if (item.id == id) { content = $"<b>Блок питания:</b> {item.name}\n- Мощность: {item.power}W\n- ATX 3.0: {(item.isAtx3 ? "Да" : "Нет")}\n- Цена: {item.price} руб."; break; }
                break;
            case PartType.Cooler:
                foreach (var item in db.cooler) if (item.id == id) { content = $"<b>Охлаждение:</b> {item.name}\n- Макс. TDP: {item.maxTdp}W\n- Цена: {item.price} руб."; break; }
                break;
            case PartType.Storage:
                foreach (var item in db.storage) if (item.id == id) { content = $"<b>Накопитель:</b> {item.name}\n- Тип: {item.type}\n- Объем: {item.capacityGb}GB\n- Цена: {item.price} руб."; break; }
                break;
            case PartType.Case:
                foreach (var item in db.cases) if (item.id == id) { content = $"<b>Корпус:</b> {item.name}\n- Макс. длина GPU: {item.maxGpuLengthMm}мм\n- Цена: {item.price} руб."; break; }
                break;
        }

        aiDisplayText.text = string.IsNullOrEmpty(content) ? title + "Данные отсутствуют." : title + content;
    }

    [Header("Связь с логикой сборки")]
    public BuildManager buildManager;

    // Этот метод мы завтра повесим на синюю кнопку "Спросить ИИ"
    public void OnAskAIButtonClicked()
    {
        if (buildManager != null)
        {
            aiDisplayText.text = "<b>[ИИ СИСТЕМА]:</b> Отправка данных эксперту...";
            buildManager.FinishBuildAndCheckAI();
        }
    }
}