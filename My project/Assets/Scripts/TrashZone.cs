using UnityEngine;

public class TrashZone : MonoBehaviour
{
    // Метод срабатывает автоматически, когда коллайдер детали пересекает эту зону
    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, есть ли компонент PcPart на влетевшем объекте или его родителях
        PcPart part = other.GetComponentInParent<PcPart>();

        if (part != null)
        {
            Debug.Log($"[Утилизатор] Деталь {part.type} с ID {part.id} возвращена в пул объектов.");

            // Гасим скорость физического тела, чтобы деталь не летела по инерции в инвизе
            Rigidbody rb = part.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // Выключаем объект (возвращаем в пул инвиза)
            part.gameObject.SetActive(false);
        }
    }
}