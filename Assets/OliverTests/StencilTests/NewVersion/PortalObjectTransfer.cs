using UnityEngine;

public class PortalObjectTransfer : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PortalObject portalObject = other.GetComponent<PortalObject>();
        if (!portalObject || portalObject.isTransferring)
            return;

        BeginTransfer(portalObject);
    }

    private void OnTriggerExit(Collider other)
    {
        PortalObject portalObject = other.GetComponent<PortalObject>();
        if (!portalObject || !portalObject.isTransferring)
            return;

        CompleteTransfer(portalObject);
    }

    // =========================
    // TRANSFER LOGIC
    // =========================

    private void BeginTransfer(PortalObject portalObject)
    {
        portalObject.isTransferring = true;

        GameObject original = portalObject.gameObject;

        // Create proxy under neutral root
        GameObject proxy = Instantiate(original, PortalProxyRoot.Instance);
        proxy.name = original.name + "_PortalProxy";

        // Remove PortalObject from proxy
        Destroy(proxy.GetComponent<PortalObject>());

        // Disable physics on proxy
        DisablePhysics(proxy);

        // Set correct layer
        SetLayerRecursively(proxy, portalObject.OtherLayer);

        // Add sync component
        PortalProxySync sync = proxy.AddComponent<PortalProxySync>();
        sync.source = original.transform;

        portalObject.proxyInstance = proxy;
    }

    private void CompleteTransfer(PortalObject portalObject)
    {
        GameObject original = portalObject.gameObject;

        // Switch original to other world layer
        SetLayerRecursively(original, portalObject.OtherLayer);

        // Cleanup proxy
        if (portalObject.proxyInstance)
            Destroy(portalObject.proxyInstance);

        portalObject.proxyInstance = null;
        portalObject.isTransferring = false;
    }

    // =========================
    // HELPERS
    // =========================

    private void DisablePhysics(GameObject obj)
    {
        foreach (Rigidbody rb in obj.GetComponentsInChildren<Rigidbody>())
            rb.isKinematic = true;

        foreach (Collider col in obj.GetComponentsInChildren<Collider>())
            col.enabled = false;
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}
