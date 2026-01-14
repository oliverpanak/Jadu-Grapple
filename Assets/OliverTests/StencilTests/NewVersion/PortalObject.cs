using UnityEngine;

public class PortalObject : MonoBehaviour
{
    public bool isTransferring;
    public GameObject proxyInstance;

    public int CurrentStencilLayer =>
        gameObject.layer;

    public int OtherStencilLayer =>
        gameObject.layer == LayerMask.NameToLayer("StencilLayer1")
            ? LayerMask.NameToLayer("StencilLayer2")
            : LayerMask.NameToLayer("StencilLayer1");
}