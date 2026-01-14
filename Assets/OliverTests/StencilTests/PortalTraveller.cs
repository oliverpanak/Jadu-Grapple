using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PortalTraveller : MonoBehaviour
{
    [Header("Tags")]
    public string portalTag = "Portal";

    [Header("Layers")]
    public int layerWorld1;
    public int layerWorld2;

    private GameObject proxy;
    private bool isPassingThrough;

    void Awake()
    {
        layerWorld1 = LayerMask.NameToLayer("StencilLayer1");
        layerWorld2 = LayerMask.NameToLayer("StencilLayer2");
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(portalTag)) return;
        if (isPassingThrough) return;

        Debug.Log(name + " entering portal: " + other.name);
        BeginTransition();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(portalTag)) return;

        Debug.Log(name + " exiting portal: " + other.name);
        CompleteTransition();
    }

    // Public so other scripts can call if needed
    public void BeginTransition()
    {
        if (isPassingThrough) return;
        isPassingThrough = true;

        // Duplicate proxy
        proxy = Instantiate(gameObject, transform.position, transform.rotation);
        Destroy(proxy.GetComponent<PortalTraveller>()); // remove script from proxy

        int proxyLayer = gameObject.layer == layerWorld1 ? layerWorld2 : layerWorld1;
        SetLayerRecursively(proxy, proxyLayer);

        // Copy animator state
        Animator src = GetComponent<Animator>();
        Animator dst = proxy.GetComponent<Animator>();
        if (src && dst)
        {
            dst.runtimeAnimatorController = src.runtimeAnimatorController;
            dst.Play(src.GetCurrentAnimatorStateInfo(0).fullPathHash, 0,
                     src.GetCurrentAnimatorStateInfo(0).normalizedTime);
        }

        Debug.Log(name + " proxy created on layer: " + proxyLayer);
    }

    public void CompleteTransition()
    {
        if (!isPassingThrough) return;

        // Switch original layer
        int newLayer = gameObject.layer == layerWorld1 ? layerWorld2 : layerWorld1;
        SetLayerRecursively(gameObject, newLayer);

        if (proxy != null) Destroy(proxy);
        proxy = null;
        isPassingThrough = false;

        Debug.Log(name + " completed transition. Now on layer: " + newLayer);
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}
