using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class InteractableObject : MonoBehaviour
{
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public bool isHeld;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
}