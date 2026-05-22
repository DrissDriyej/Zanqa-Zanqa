using UnityEngine;
using UnityEngine.XR;

public class XRSimulatorActivator : MonoBehaviour
{
    [SerializeField] private GameObject xrDeviceSimulator;

    void Start()
    {
        bool headsetConnected = XRSettings.isDeviceActive;
        xrDeviceSimulator.SetActive(!headsetConnected);
    }
}