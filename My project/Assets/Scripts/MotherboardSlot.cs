using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit; // Подключаем VR сокеты

public class MotherboardSlot : MonoBehaviour
{
    [Header("Настройки слота")]
    public PartType acceptableType; // Какой тип детали принимает этот конкретный слот
    public int motherboardId;       // ID материнки, на которой находится слот (из твоей базы)

    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socket;
    private BuildManager buildManager;

    void Awake()
    {
        socket = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
        buildManager = FindFirstObjectByType<BuildManager>();

        // Подписываемся на события сокета VR
        socket.selectEntered.AddListener(OnPartSnapped);
        socket.selectExited.AddListener(OnPartRemoved);
    }

    // Срабатывает АВТОМАТИЧЕСКИ, когда деталь примагнитилась в VR
    private void OnPartSnapped(SelectEnterEventArgs args)
    {
        // Вытаскиваем скрипт PcPart с прилетевшего объекта
        PcPart part = args.interactableObject.transform.GetComponent<PcPart>();

        if (part != null)
        {
            // Отправляем в твой BuildManager на жесткую проверку порядка и сокета
            bool isCompatible = buildManager.RequestInstallation(part, motherboardId);

            if (!isCompatible)
            {
                // Если твой бэк сказал "нельзя" — принудительно выкидываем деталь из сокета
                StartCoroutine(EjectPartCoroutine());
            }
        }
    }

    // Срабатывает, если игрок вытащил деталь руками из материнки
    private void OnPartRemoved(SelectExitEventArgs args)
    {
        PcPart part = args.interactableObject.transform.GetComponent<PcPart>();
        if (part != null)
        {
            // Здесь можно добавить логику размонтирования детали из StateManager, если нужно
            Debug.Log($"Деталь {part.type} извлечена из сборки.");
        }
    }

    private System.Collections.IEnumerator EjectPartCoroutine()
    {
        yield return new WaitForEndOfFrame();
        // Заставляем VR-сокет отпустить неподходящую деталь
        socket.interactionManager.SelectExit(socket, socket.hasSelection ? socket.interactablesSelected[0] : null);
    }

    void OnDestroy()
    {
        if (socket != null)
        {
            socket.selectEntered.RemoveListener(OnPartSnapped);
            socket.selectExited.RemoveListener(OnPartRemoved);
        }
    }
}