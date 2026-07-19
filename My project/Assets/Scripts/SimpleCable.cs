using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SimpleCable : MonoBehaviour
{
    public Transform startPort; // Точка, откуда торчит провод из БП
    public Transform movingConnector; // Сам пластиковый коннектор, который таскаем руками
    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
    }

    void Update()
    {
        if (startPort != null && movingConnector != null)
        {
            // Рисуем прямую линию между портом БП и коннектором
            lineRenderer.SetPosition(0, startPort.position);
            lineRenderer.SetPosition(1, movingConnector.position);
        }
    }
}