using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("Portal Settings")]
    public string portalTag = "Portal";

    void Awake()
    {
        gameObject.tag = portalTag;
        if (!TryGetComponent<Collider>(out _))
        {
            Debug.LogWarning("Portal needs a Collider for trigger detection!");
        }
    }
}