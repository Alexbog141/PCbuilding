using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SimpleCable : MonoBehaviour
{
    public string cableID;            // "connector_24pin", "connector_8pin_cpu", "connector_8pin_gpu"
    public Transform startPort;       // Точка выхода из БП
    public Transform movingConnector; // Коннектор на столе

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
            if (lineRenderer != null) lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, startPort.position);
            lineRenderer.SetPosition(1, movingConnector.position);
        }
        else
        {
            if (lineRenderer != null) lineRenderer.enabled = false;
        }
    }

    public void SetStartPort(Transform newPort)
    {
        startPort = newPort;
    }
}