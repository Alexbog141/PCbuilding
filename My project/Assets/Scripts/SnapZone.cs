using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SnapZone : MonoBehaviour
{
    [Header("Настройки Бэкенда")]
    public int motherboardId = 103;
    private BuildManager buildManager;

    [Header("Кабели (найдутся автоматически)")]
    public GameObject psuCables;

    private void Start()
    {
        buildManager = FindAnyObjectByType<BuildManager>();

        // АВТО-ПОИСК КАБЕЛЕЙ (даже если поле в Инспекторе пустое)
        if (psuCables == null)
        {
            psuCables = GameObject.Find("PSU_Cables");
        }

        // ПРЯЧЕМ ИХ ПРИ СТАРТЕ ИГРЫ
        if (psuCables != null)
        {
            psuCables.SetActive(false);
            Debug.Log("[SnapZone] Кабели найдены и успешно спрятаны до установки БП!");
        }
        else
        {
            Debug.LogError("[SnapZone] ОШИБКА: Объект PSU_Cables не найден на сцене! Проверь имя в иерархии!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PcPart part = other.GetComponentInParent<PcPart>();
        if (part == null) return;

        Transform targetAnchor = null;

        if (part.type == PartType.Motherboard)
        {
            GameObject moboMount = GameObject.Find("Motherboard_Mount");
            if (moboMount != null) targetAnchor = moboMount.transform;
        }
        else if (part.type == PartType.PSU)
        {
            GameObject psuAnchor = GameObject.Find("Anchor_PSU");
            if (psuAnchor != null) targetAnchor = psuAnchor.transform;
        }
        else
        {
            Transform activeMobo = FindInstalledMotherboard();
            if (activeMobo != null) targetAnchor = FindFreeAnchor(activeMobo, part.type);
        }

        if (targetAnchor != null)
        {
            ExecuteSnap(part, targetAnchor);
        }
    }

    private Transform FindInstalledMotherboard()
    {
        GameObject moboMount = GameObject.Find("Motherboard_Mount");
        if (moboMount != null && moboMount.transform.childCount > 0)
        {
            return moboMount.transform.GetChild(0);
        }
        return null;
    }

    private Transform FindFreeAnchor(Transform mobo, PartType type)
    {
        string typeName = (type == PartType.Storage) ? "SSD" : type.ToString();

        for (int i = 1; i <= 8; i++)
        {
            string suffix = (i == 1) ? "" : $"_{i}";
            string anchorName = $"Anchor_{typeName}{suffix}";

            Transform candidate = mobo.Find(anchorName);
            if (candidate != null && candidate.childCount == 0) return candidate;
        }
        return null;
    }

    private void ExecuteSnap(PcPart part, Transform anchor)
    {
        XRGrabInteractable grab = part.GetComponent<XRGrabInteractable>();
        if (grab != null) grab.enabled = false;

        Rigidbody rb = part.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
        }

        part.transform.SetParent(anchor);
        part.transform.localPosition = Vector3.zero;
        part.transform.localRotation = Quaternion.identity;

        // ВКЛЮЧАЕМ КАБЕЛИ ПРИ БП
        if (part.type == PartType.PSU && psuCables != null)
        {
            psuCables.SetActive(true);
            Debug.Log("🟢🟢🟢 КАБЕЛИ УСПЕШНО ВКЛЮЧЕНЫ! 🟢🟢🟢");
        }

        if (buildManager != null)
        {
            buildManager.RequestInstallation(part, motherboardId);
            Debug.Log($"[SnapSystem] {part.type} успешно зафиксирован в {anchor.name}!");
        }
    }
}