using UnityEngine;

public class PortalObject : MonoBehaviour
{
    [HideInInspector] public bool isTransferring;
    [HideInInspector] public GameObject proxyInstance;

    public int CurrentLayer => gameObject.layer;

    public int OtherLayer
    {
        get
        {
            int real = LayerMask.NameToLayer("StencilLayer1");
            int virtualL = LayerMask.NameToLayer("StencilLayer2");

            return gameObject.layer == real ? virtualL : real;
        }
    }
}