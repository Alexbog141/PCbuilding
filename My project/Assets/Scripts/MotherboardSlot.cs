using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class MotherboardSlot : XRSocketInteractor
{
    [Header("Настройки Бэкенда (Сборка ПК)")]
    public PartType acceptableType;
    public int motherboardId;

    [Header("Кастомное выравнивание (Принудительно)")]
    [Tooltip("Если включено, скрипт намертво выставит позицию и поворот детали при вставке")]
    public bool overrideTransform = true;

    [Tooltip("Смещение детали внутри слота. Обычно (0, 0, 0)")]
    public Vector3 customPosition = Vector3.zero;

    [Tooltip("Угол поворота детали. Подберите нужный (например: 90, 0, 0 или 0, 90, 0)")]
    public Vector3 customRotation = new Vector3(90f, 0f, 0f);

    private BuildManager buildManager;

    protected override void Awake()
    {
        base.Awake();
        buildManager = FindAnyObjectByType<BuildManager>();
    }

    public override bool CanHover(IXRHoverInteractable interactable)
    {
        return base.CanHover(interactable) && IsCorrectPartType(interactable);
    }

    public override bool CanSelect(IXRSelectInteractable interactable)
    {
        return base.CanSelect(interactable) && IsCorrectPartType(interactable);
    }

    private bool IsCorrectPartType(IXRInteractable interactable)
    {
        PcPart part = interactable.transform.GetComponent<PcPart>();
        return part != null && part.type == acceptableType;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        Transform targetTransform = args.interactableObject.transform;

        // 1. Принудительно выравниваем деталь относительно центра слота
        if (overrideTransform)
        {
            targetTransform.localPosition = customPosition;
            targetTransform.localRotation = Quaternion.Euler(customRotation);
        }

        // 2. Проверка совместимости через BuildManager
        PcPart part = targetTransform.GetComponent<PcPart>();
        if (part != null && buildManager != null)
        {
            bool isCompatible = buildManager.RequestInstallation(part, motherboardId);

            if (!isCompatible)
            {
                StartCoroutine(EjectPartCoroutine(args.interactableObject));
            }
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        PcPart part = args.interactableObject.transform.GetComponent<PcPart>();
        if (part != null)
        {
            Debug.Log($"[Слот] Деталь {part.type} извлечена.");
        }
    }

    private IEnumerator EjectPartCoroutine(IXRSelectInteractable interactable)
    {
        yield return new WaitForEndOfFrame();
        interactionManager.SelectExit(this, interactable);
    }
}