using UnityEngine;
using UnityEngine.InputSystem;

public class ARGrabAndThrow : MonoBehaviour
{
    [Header("Interaction")]
    public string interactableTag = "Interactable";
    public float maxGrabDistance = 5f;

    [Header("Hold")]
    public float holdDistance = 0.6f;
    public float followSpeed = 25f;

    [Header("Pan")]
    public float panSpeed = 0.0025f;
    public float maxPanOffset = 0.4f;

    [Header("Throw")]
    public float throwForce = 2.0f;

    [Header("World Filtering")]
    public PortalRenderFeatureSwitcher portalWorldSwitcher;
    public string realWorldLayerName = "StencilLayer1";
    public string virtualWorldLayerName = "StencilLayer2";

    private Camera cam;
    private InteractableObject heldObject;

    // Pan state
    private Vector2 lastPointerPos;
    private Vector2 panOffset;

    private int realWorldLayer;
    private int virtualWorldLayer;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        realWorldLayer = LayerMask.NameToLayer(realWorldLayerName);
        virtualWorldLayer = LayerMask.NameToLayer(virtualWorldLayerName);
    }

    private void Update()
    {
        if (PointerDown())
        {
            lastPointerPos = GetPointerPosition();
            TryGrab();
        }
        else if (PointerHeld())
        {
            UpdatePan();
            UpdateHeldObject();
        }
        else if (PointerReleased())
        {
            ReleaseObject();
        }
    }

    // =========================
    // POINTER (INPUT SYSTEM)
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

    private Vector2 GetPointerPosition()
    {
        if (Mouse.current != null)
            return Mouse.current.position.ReadValue();

        if (Touchscreen.current != null)
            return Touchscreen.current.primaryTouch.position.ReadValue();

        return Vector2.zero;
    }

    // =========================
    // GRAB LOGIC
    // =========================

    private void TryGrab()
    {
        if (heldObject != null)
            return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        if (!Physics.Raycast(ray, out RaycastHit hit, maxGrabDistance))
            return;

        if (!hit.collider.CompareTag(interactableTag))
            return;

        InteractableObject interactable =
            hit.collider.GetComponentInParent<InteractableObject>();

        if (!interactable || interactable.isHeld)
            return;

        // 🚫 BLOCK IF NOT IN PLAYER'S WORLD
        if (!IsObjectInPlayerWorld(interactable.gameObject))
            return;

        // Optional: block mid-portal
        if (interactable.GetComponent<PortalObject>()?.isTransferring == true)
            return;

        Grab(interactable);
    }

    private bool IsObjectInPlayerWorld(GameObject obj)
    {
        if (!portalWorldSwitcher)
            return true; // fail-safe

        int objLayer = obj.layer;

        switch (portalWorldSwitcher.CurrentWorld)
        {
            case PortalRenderFeatureSwitcher.PlayerWorld.InRealWorld:
                return objLayer == realWorldLayer;

            case PortalRenderFeatureSwitcher.PlayerWorld.InVirtualWorld:
                return objLayer == virtualWorldLayer;
        }

        return false;
    }

    private void Grab(InteractableObject obj)
    {
        heldObject = obj;
        heldObject.isHeld = true;

        panOffset = Vector2.zero;

        Rigidbody rb = heldObject.rb;
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    // =========================
    // HOLD UPDATE
    // =========================

    private void UpdatePan()
    {
        if (!heldObject)
            return;

        Vector2 currentPos = GetPointerPosition();
        Vector2 delta = currentPos - lastPointerPos;
        lastPointerPos = currentPos;

        panOffset += delta * panSpeed;
        panOffset = Vector2.ClampMagnitude(panOffset, maxPanOffset);
    }

    private void UpdateHeldObject()
    {
        if (!heldObject)
            return;

        Transform t = heldObject.transform;

        Vector3 right = cam.transform.right * panOffset.x;
        Vector3 up = cam.transform.up * panOffset.y;

        Vector3 targetPos =
            cam.transform.position +
            cam.transform.forward * holdDistance +
            right +
            up;

        t.position = Vector3.Lerp(
            t.position,
            targetPos,
            Time.deltaTime * followSpeed
        );
    }

    // =========================
    // RELEASE
    // =========================

    private void ReleaseObject()
    {
        if (!heldObject)
            return;

        Rigidbody rb = heldObject.rb;

        rb.isKinematic = false;
        rb.useGravity = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(cam.transform.forward * throwForce, ForceMode.Impulse);

        heldObject.isHeld = false;
        heldObject = null;
    }
}
