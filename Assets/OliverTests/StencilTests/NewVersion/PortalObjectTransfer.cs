using UnityEngine;

public class PortalObjectTransfer : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var portalObj = other.GetComponent<PortalObject>();
        if (!portalObj || portalObj.isTransferring)
            return;

        portalObj.isTransferring = true;

        // Create proxy
        portalObj.proxyInstance = Instantiate(other.gameObject);
        portalObj.proxyInstance.name = other.gameObject.name + "_Proxy";

        // Set proxy layer
        SetLayerRecursively(
            portalObj.proxyInstance,
            portalObj.OtherStencilLayer
        );

        // Disable physics on proxy
        DisablePhysics(portalObj.proxyInstance);

        // Sync animations
        CopyAnimator(other.gameObject, portalObj.proxyInstance);
    }

    private void OnTriggerExit(Collider other)
    {
        var portalObj = other.GetComponent<PortalObject>();
        if (!portalObj || !portalObj.isTransferring)
            return;

        // Finalize transfer
        SetLayerRecursively(
            other.gameObject,
            portalObj.OtherStencilLayer
        );

        Destroy(portalObj.proxyInstance);
        portalObj.proxyInstance = null;
        portalObj.isTransferring = false;
    }

    private void Update()
    {
        // Sync all active proxies
        foreach (var obj in FindObjectsOfType<PortalObject>())
        {
            if (!obj.isTransferring || obj.proxyInstance == null)
                continue;

            SyncTransform(obj.transform, obj.proxyInstance.transform);
        }
    }

    private void SyncTransform(Transform source, Transform target)
    {
        target.position = source.position;
        target.rotation = source.rotation;
        target.localScale = source.localScale;
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    private void DisablePhysics(GameObject obj)
    {
        foreach (var rb in obj.GetComponentsInChildren<Rigidbody>())
            rb.isKinematic = true;

        foreach (var col in obj.GetComponentsInChildren<Collider>())
            col.enabled = false;
    }

    private void CopyAnimator(GameObject source, GameObject target)
    {
        var srcAnim = source.GetComponent<Animator>();
        var dstAnim = target.GetComponent<Animator>();

        if (!srcAnim || !dstAnim)
            return;

        dstAnim.runtimeAnimatorController = srcAnim.runtimeAnimatorController;
        dstAnim.avatar = srcAnim.avatar;
        dstAnim.applyRootMotion = srcAnim.applyRootMotion;
    }
}
