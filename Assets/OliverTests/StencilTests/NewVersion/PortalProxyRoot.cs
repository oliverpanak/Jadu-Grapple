using UnityEngine;

public class PortalProxyRoot : MonoBehaviour
{
    public static Transform Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = transform;
        transform.localScale = Vector3.one;
    }
}