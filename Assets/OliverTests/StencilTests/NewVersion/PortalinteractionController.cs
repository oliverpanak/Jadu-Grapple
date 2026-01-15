using UnityEngine;
using UnityEngine.InputSystem;

public class PortalInteractionController : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;

    [Header("Interaction Settings")]
    public float grabDistance = 2.5f;
    public float holdDistance = 1.0f;
    public float throwForceMultiplier = 1.0f;

    private PortalInteractable heldObject;
    private Transform originalParent;

    private Vector3 lastFramePosition;
    private Vector3 throwVelocity;

    private void Update()
    {
        // If the held object was destroyed or disabled, drop it safely
        if (heldObject && heldObject.gameObject == null)
        {
            ForceDrop();
            return;
        }

        if (PointerDown())
            TryGrab();

        if (PointerHeld() && IsHoldingValidObject())
            UpdateHeldObjectVelocity();

        if (PointerReleased())
            Release();
    }

    // =========================
    // INPUT (UNCHANGED)
    // =========================

    private bool PointerDown()
    {
        if (Mouse.current != null)
            return Mouse.current.leftButton.wasPressedThisFrame;

        if (Touchscreen.current != null)
            return Touchscreen.current.primaryTouch.press.wasPressedThisFrame;

        return false;
    }

    private bool PointerHeld()
    {
        if (Mouse.current != null)
            return Mouse.current.leftButton.isPressed;

        if (Touchscreen.current != null)
            return Touchscreen.current.primaryTouch.press.isPressed;

        return false;
    }

    private bool PointerReleased()
    {
        if (Mouse.current != null)
            return Mouse.current.leftButton.wasReleasedThisFrame;

        if (Touchscreen.current != null)
            return Touchscreen.current.primaryTouch.press.wasReleasedThisFrame;

        return false;
    }

    // =========================
    // GRAB LOGIC
    // =========================

    private void TryGrab()
    {
        if (heldObject)
            return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, grabDistance))
            return;

        PortalInteractable interactable =
            hit.collider.GetComponentInParent<PortalInteractable>();

        if (!interactable || interactable.rb == null)
            return;

        Grab(interactable);
    }

    private void Grab(PortalInteractable interactable)
    {
        heldObject = interactable;
        originalParent = heldObject.transform.parent;

        Rigidbody rb = heldObject.rb;
        rb.isKinematic = true;

        heldObject.transform.SetParent(playerCamera.transform, true);
        heldObject.transform.localPosition = Vector3.forward * holdDistance;

        lastFramePosition = heldObject.transform.position;
        throwVelocity = Vector3.zero;
    }

    // =========================
    // HOLD / THROW
    // =========================

    private bool IsHoldingValidObject()
    {
        return heldObject != null && heldObject.transform != null;
    }

    private void UpdateHeldObjectVelocity()
    {
        // Final safety guard
        if (!IsHoldingValidObject())
        {
            ForceDrop();
            return;
        }

        Vector3 currentPosition = heldObject.transform.position;
        float dt = Mathf.Max(Time.deltaTime, 0.0001f);

        throwVelocity = (currentPosition - lastFramePosition) / dt;
        lastFramePosition = currentPosition;
    }

    private void Release()
    {
        if (!IsHoldingValidObject())
        {
            ForceDrop();
            return;
        }

        Rigidbody rb = heldObject.rb;

        heldObject.transform.SetParent(originalParent, true);

        rb.isKinematic = false;
        rb.linearVelocity = throwVelocity * throwForceMultiplier;

        heldObject = null;
    }

    private void ForceDrop()
    {
        // Emergency cleanup when object disappears
        heldObject = null;
    }
}
