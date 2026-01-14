using UnityEngine;

public class PortalTrigger : MonoBehaviour
{
    public PortalRenderFeatureSwitcher renderSwitcher;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        renderSwitcher.EnterPortal();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        renderSwitcher.ExitPortal();
    }
}