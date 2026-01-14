using UnityEngine;

public class PortalTrigger : MonoBehaviour
{
    public PortalRenderFeatureSwitcher switcher;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;
        Debug.Log("Enter");
        switcher.BeginPortalCrossing();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;
        Debug.Log("Exit");
        switcher.CompletePortalCrossing();
    }
}