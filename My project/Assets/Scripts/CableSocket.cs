using UnityEngine;

public class CableSocket : MonoBehaviour
{
    public string targetCableID; // Имя коннектора (например, "connector_24pin")
    private bool isConnected = false;
    private BuildManager buildManager;

    private void Start()
    {
        // Автоматически находим BuildManager на сцене
        buildManager = FindAnyObjectByType<BuildManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isConnected) return;

        // Если коснулись объекта с нужным именем коннектора
        if (other.name == targetCableID)
        {
            other.transform.position = transform.position;
            other.transform.rotation = transform.rotation;

            if (other.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.isKinematic = true;
            }

            isConnected = true;
            Debug.Log($"[Кабель] Подключен к порту: {name}");

            // Оповещаем BuildManager, что этот провод успешно воткнули!
            if (buildManager != null)
            {
                buildManager.OnCableConnected(targetCableID);
            }
        }
    }
}