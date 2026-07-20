using UnityEngine;

public class PSUConnectorBinder : MonoBehaviour
{
    [Header("Порты этого конкретного БП")]
    public Transform port24Pin;
    public Transform port8PinCpu;
    public Transform port8PinGpu;

    private void OnEnable()
    {
        BindCablesToThisPSU();
    }

    public void BindCablesToThisPSU()
    {
        SimpleCable[] cables = FindObjectsByType<SimpleCable>();

        foreach (var cable in cables)
        {
            if (cable.cableID == "connector_24pin" && port24Pin != null)
            {
                cable.SetStartPort(port24Pin);
            }
            else if (cable.cableID == "connector_8pin_cpu" && port8PinCpu != null)
            {
                cable.SetStartPort(port8PinCpu);
            }
            else if (cable.cableID == "connector_8pin_gpu" && port8PinGpu != null)
            {
                cable.SetStartPort(port8PinGpu);
            }
        }
    }
}