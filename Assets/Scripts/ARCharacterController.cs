using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class ARCharacterController : MonoBehaviour
{
    [Header("References")]
    public Camera arCamera;
    public Button moveButton;
    public Button grappleButton;
    public Button jumpButton;
    public Image reticleUI;

    [Header("Prefabs for Visuals")]
    public GameObject moveTargetIndicatorPrefab;
    public GameObject grappleTargetIndicatorPrefab;
    public Material grappleLineMaterial;

    [Header("Grapple Projectile")]
    public GameObject grappleProjectilePrefab;
    public float projectileSpeed = 10f;

    [Header("Movement Settings")]
    public float moveSpeed = 1.5f;
    public float grappleSpeed = 3f;
    public float stopDistance = 0.05f;
    public float grappleHoldTime = 3f;
    public float maxGrappleDistance = 2000000f;
    public bool usePhysicsMovement = false;
    public bool retainMomentumAfterGrapple = false;

    [Header("Physics Movement Settings")]
    public float movementForce = 10f;
    public float maxVelocity = 2f;

    [Header("Gravity Boots Settings")]
    public bool gravityBootsEnabled = false; // ✅ toggle for gravity boots
    public float gravityStrength = 9.81f;
    private Vector3 currentGravity = Vector3.down;
    private Vector3 targetUp = Vector3.up;

    [Header("Jump Settings")]
    public float jumpForce = 5f;
    private bool isGrounded = true;

    [Header("Rope Settings")]
    public float ropeTension = 10f;
    public float ropeDamping = 5f;

    [Header("UI Colors")]
    public Color validTargetColor = Color.green;
    public Color invalidTargetColor = Color.red;

    private GameObject moveTargetIndicator;
    private GameObject grappleTargetIndicator;
    private LineRenderer grappleLine;

    private Vector3 moveTarget;
    private Vector3 grappleTarget;
    private bool isMoving = false;
    private bool isGrappling = false;
    private bool isProjectileFlying = false;

    private Vector3 ropeVelocity = Vector3.zero;
    private Vector3 currentRopeEnd;
    private GameObject currentProjectile;
    private Rigidbody rb;
    private Coroutine grappleRoutine;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // We control gravity manually

        // Instantiate indicators
        if (moveTargetIndicatorPrefab)
        {
            moveTargetIndicator = Instantiate(moveTargetIndicatorPrefab);
            moveTargetIndicator.SetActive(false);
        }

        if (grappleTargetIndicatorPrefab)
        {
            grappleTargetIndicator = Instantiate(grappleTargetIndicatorPrefab);
            grappleTargetIndicator.SetActive(false);
        }

        // Create line renderer
        GameObject grappleLineObj = new GameObject("GrappleLine");
        grappleLineObj.transform.parent = transform;
        grappleLine = grappleLineObj.AddComponent<LineRenderer>();
        grappleLine.positionCount = 2;
        grappleLine.enabled = false;
        grappleLine.widthMultiplier = 0.02f;
        if (grappleLineMaterial)
            grappleLine.material = grappleLineMaterial;

        // Hook up UI buttons
        if (moveButton) moveButton.onClick.AddListener(OnMoveButtonPressed);
        if (grappleButton) grappleButton.onClick.AddListener(OnGrappleButtonPressed);
        if (jumpButton) jumpButton.onClick.AddListener(OnJumpButtonPressed);
    }

    void Update()
    {
        if (!isProjectileFlying)
            UpdateTargetIndicators();

        HandleMovement();
        HandleGrapple();
        HandleGravityAndRotation();
    }

    // ---------------- Gravity Boots System ----------------

    private void HandleGravityAndRotation()
    {
        if (gravityBootsEnabled)
        {
            rb.AddForce(currentGravity * gravityStrength, ForceMode.Acceleration);

            // Smoothly rotate to align "up" opposite to gravity direction
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -currentGravity) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
        else
        {
            // Standard downward gravity
            rb.AddForce(Vector3.down * gravityStrength, ForceMode.Acceleration);

            // Keep upright orientation
            Quaternion upright = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, upright, Time.deltaTime * 5f);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        isGrounded = true;
        if (!gravityBootsEnabled) return;

        // Get average contact normal
        Vector3 averageNormal = Vector3.zero;
        foreach (ContactPoint contact in collision.contacts)
            averageNormal += contact.normal;
        averageNormal.Normalize();

        // Switch gravity direction to be inverse of surface normal
        currentGravity = -averageNormal;
        targetUp = averageNormal;
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }

    // ---------------- Jump System ----------------

    private void OnJumpButtonPressed()
    {
        if (!isGrounded) return; // Only jump if on surface

        rb.isKinematic = false;
        isGrounded = false;

        Vector3 jumpDir = gravityBootsEnabled ? -currentGravity.normalized : Vector3.up;
        rb.AddForce(jumpDir * jumpForce, ForceMode.VelocityChange);
    }

    // ---------------- Movement + Grappling ----------------

    void UpdateTargetIndicators()
    {
        Ray ray = new Ray(arCamera.transform.position, arCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            bool isHorizontal = Vector3.Dot(hit.normal, Vector3.up) > 0.8f;
            bool isVertical = Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up)) < 0.3f;
            float distance = Vector3.Distance(transform.position, hit.point);

            // Move
            if (isHorizontal && distance <= maxGrappleDistance)
            {
                moveTargetIndicator.SetActive(true);
                moveTargetIndicator.transform.position = hit.point;
                moveButton.interactable = true;
            }
            else
            {
                moveTargetIndicator.SetActive(false);
                moveButton.interactable = false;
            }

            // Grapple
            if (isVertical && distance <= maxGrappleDistance)
            {
                grappleTargetIndicator.SetActive(true);
                grappleTargetIndicator.transform.position = hit.point;
                grappleButton.interactable = true;
            }
            else
            {
                grappleTargetIndicator.SetActive(false);
                grappleButton.interactable = false;
            }

            if (reticleUI)
                reticleUI.color = (isHorizontal || isVertical) && distance <= maxGrappleDistance ? validTargetColor : invalidTargetColor;
        }
        else
        {
            moveTargetIndicator.SetActive(false);
            grappleTargetIndicator.SetActive(false);
            moveButton.interactable = false;
            grappleButton.interactable = false;
            if (reticleUI) reticleUI.color = invalidTargetColor;
        }
    }

    void OnMoveButtonPressed()
    {
        if (!moveButton.interactable) return;

        CancelCurrentGrapple();
        isMoving = true;
        moveTarget = moveTargetIndicator.transform.position;
        rb.isKinematic = false;
    }

    void OnGrappleButtonPressed()
    {
        if (!grappleButton.interactable) return;

        CancelCurrentGrapple();
        isMoving = false;
        grappleTarget = grappleTargetIndicator.transform.position;

        float distance = Vector3.Distance(transform.position, grappleTarget);
        if (distance > maxGrappleDistance)
        {
            Debug.Log("❌ Grapple target out of range.");
            return;
        }

        grappleRoutine = StartCoroutine(ShootGrappleProjectile());
    }

    IEnumerator ShootGrappleProjectile()
    {
        isProjectileFlying = true;
        isGrappling = false;

        if (grappleProjectilePrefab)
            currentProjectile = Instantiate(grappleProjectilePrefab, transform.position, Quaternion.identity);

        while (currentProjectile && Vector3.Distance(currentProjectile.transform.position, grappleTarget) > 0.1f)
        {
            currentProjectile.transform.position = Vector3.MoveTowards(
                currentProjectile.transform.position,
                grappleTarget,
                projectileSpeed * Time.deltaTime
            );

            UpdateLineRenderer(currentProjectile.transform.position);
            yield return null;
        }

        if (currentProjectile)
            currentProjectile.transform.position = grappleTarget;

        isProjectileFlying = false;
        isGrappling = true;
        currentRopeEnd = transform.position;
        rb.isKinematic = true;

        grappleRoutine = StartCoroutine(GrappleDuration());
    }

    IEnumerator GrappleDuration()
    {
        Vector3 lastVelocity = Vector3.zero;

        while (Vector3.Distance(transform.position, grappleTarget) > stopDistance)
        {
            Vector3 prevPos = transform.position;
            transform.position = Vector3.MoveTowards(transform.position, grappleTarget, grappleSpeed * Time.deltaTime);
            lastVelocity = (transform.position - prevPos) / Time.deltaTime;
            UpdateLineRenderer(grappleTarget);
            yield return null;
        }

        yield return new WaitForSeconds(grappleHoldTime);

        if (retainMomentumAfterGrapple && rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = lastVelocity;
        }

        EndGrapple();
    }

    void CancelCurrentGrapple()
    {
        if (grappleRoutine != null)
            StopCoroutine(grappleRoutine);

        if (currentProjectile)
            Destroy(currentProjectile);

        if (grappleLine)
            grappleLine.enabled = false;

        rb.isKinematic = false;
        ropeVelocity = Vector3.zero;
        isGrappling = false;
        isProjectileFlying = false;
    }

    void EndGrapple()
    {
        isGrappling = false;
        if (!retainMomentumAfterGrapple)
            rb.isKinematic = false;

        ropeVelocity = Vector3.zero;

        if (grappleLine)
            grappleLine.enabled = false;

        if (currentProjectile)
            Destroy(currentProjectile);
    }

    void HandleMovement()
    {
        if (!isMoving) return;

        Vector3 direction = (moveTarget - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, moveTarget);

        if (usePhysicsMovement)
        {
            rb.AddForce(direction * movementForce, ForceMode.Acceleration);

            if (rb.linearVelocity.magnitude > maxVelocity)
                rb.linearVelocity = rb.linearVelocity.normalized * maxVelocity;
        }
        else
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
        }

        if (distance < stopDistance)
        {
            isMoving = false;
            if (usePhysicsMovement)
                rb.linearVelocity = Vector3.zero;
        }
    }

    void HandleGrapple()
    {
        if (!isGrappling) return;

        Vector3 displacement = grappleTarget - currentRopeEnd;
        Vector3 springForce = displacement * ropeTension;

        ropeVelocity += springForce * Time.deltaTime;
        ropeVelocity *= Mathf.Exp(-ropeDamping * Time.deltaTime);
        currentRopeEnd += ropeVelocity * Time.deltaTime;

        UpdateLineRenderer(currentRopeEnd);
    }

    void UpdateLineRenderer(Vector3 ropeEnd)
    {
        if (!grappleLine) return;
        grappleLine.enabled = true;
        grappleLine.SetPosition(0, transform.position);
        grappleLine.SetPosition(1, ropeEnd);
    }
}
