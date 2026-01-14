using UnityEngine;

public class PortalProxySync : MonoBehaviour
{
    public Transform source;

    private void LateUpdate()
    {
        if (!source)
        {
            Destroy(gameObject);
            return;
        }

        // World position & rotation
        transform.position = source.position;
        transform.rotation = source.rotation;

        // WORLD SCALE FIX
        transform.localScale = source.lossyScale;
    }
}