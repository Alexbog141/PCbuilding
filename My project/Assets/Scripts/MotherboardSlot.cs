using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class MotherboardSlot : XRSocketInteractor
{
    [Header("Настройки Бэкенда (Сборка ПК)")]
    public PartType acceptableType;
    public int motherboardId;

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

        PcPart part = args.interactableObject.transform.GetComponent<PcPart>();
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

    private System.Collections.IEnumerator EjectPartCoroutine(IXRSelectInteractable interactable)
    {
        yield return new WaitForEndOfFrame();
        interactionManager.SelectExit(this, interactable);
    }
}