using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PortalInteractable : MonoBehaviour
{
    [HideInInspector] public Rigidbody rb;
    private Collider[] colliders;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>();
    }

    public void SetInteractable(bool interactable)
    {
        rb.isKinematic = !interactable;

        foreach (var col in colliders)
            col.enabled = interactable;
    }
}