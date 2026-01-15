using UnityEngine;

public class ObjectInteractionController : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public PlayerWorldState playerWorldState;

    [Header("Interaction")]
    public float grabDistance = 1.0f;
    public float throwForce = 5f;
    public float maxRayDistance = 5f;

    private InteractableObject heldObject;
    private Transform originalParent;

    private void Update()
    {
        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
            TryGrabObject();

        else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            ReleaseObject();
    }

    // =========================
    // GRAB
    // =========================

    private void TryGrabObject()
    {
        if (heldObject != null)
            return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, maxRayDistance))
            return;

        InteractableObject interactable = hit.collider.GetComponentInParent<InteractableObject>();
        if (!interactable || interactable.isHeld)
            return;

        PortalObject portalObject = interactable.GetComponent<PortalObject>();
        if (!portalObject)
            return;

        // WORLD CHECK (CRITICAL)
        if (!IsObjectInPlayersWorld(portalObject))
            return;

        Grab(interactable);
    }

    private void Grab(InteractableObject interactable)
    {
        heldObject = interactable;
        heldObject.isHeld = true;

        Rigidbody rb = heldObject.rb;
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        originalParent = heldObject.transform.parent;

        heldObject.transform.SetParent(playerCamera.transform, worldPositionStays: true);
        heldObject.transform.localPosition = Vector3.forward * grabDistance;
        heldObject.transform.localRotation = Quaternion.identity;
    }

    // =========================
    // RELEASE
    // =========================

    private void ReleaseObject()
    {
        if (!heldObject)
            return;

        Rigidbody rb = heldObject.rb;

        heldObject.transform.SetParent(originalParent, worldPositionStays: true);

        rb.isKinematic = false;
        rb.linearVelocity = playerCamera.transform.forward * throwForce;

        heldObject.isHeld = false;
        heldObject = null;
    }

    // =========================
    // WORLD FILTER
    // =========================

    private bool IsObjectInPlayersWorld(PortalObject portalObject)
    {
        int realLayer = LayerMask.NameToLayer("StencilLayer1");
        int virtualLayer = LayerMask.NameToLayer("StencilLayer2");

        if (playerWorldState.CurrentWorld == PlayerWorldState.PlayerWorld.InRealWorld)
            return portalObject.gameObject.layer == realLayer;

        return portalObject.gameObject.layer == virtualLayer;
    }
}
