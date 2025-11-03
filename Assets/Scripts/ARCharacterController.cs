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
    public float ropeTension = 10f;
    public float ropeDamping = 5f;
    public float grappleHoldTime = 3f;

    [Header("UI Colors")]
    public Color validTargetColor = Color.green;
    public Color invalidTargetColor = Color.red;

    // Runtime-instantiated visuals
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

        // 🔹 Instantiate movement indicators
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

        // 🔹 Create line renderer dynamically
        GameObject grappleLineObj = new GameObject("GrappleLine");
        grappleLineObj.transform.parent = transform;
        grappleLine = grappleLineObj.AddComponent<LineRenderer>();
        grappleLine.positionCount = 2;
        grappleLine.enabled = false;
        grappleLine.widthMultiplier = 0.02f;
        if (grappleLineMaterial != null)
            grappleLine.material = grappleLineMaterial;

        // 🔹 Hook up button listeners
        if (moveButton != null) moveButton.onClick.AddListener(OnMoveButtonPressed);
        if (grappleButton != null) grappleButton.onClick.AddListener(OnGrappleButtonPressed);
    }

    void Update()
    {
        if (!isProjectileFlying)
            UpdateTargetIndicators();

        HandleMovement();
        HandleGrapple();
    }

    // 🔹 Target Indicators + UI State
    void UpdateTargetIndicators()
    {
        Ray ray = new Ray(arCamera.transform.position, arCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            bool isHorizontal = Vector3.Dot(hit.normal, Vector3.up) > 0.8f;
            bool isVertical = Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up)) < 0.3f;

            if (isHorizontal)
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

            if (isVertical)
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

            if (reticleUI != null)
                reticleUI.color = (isHorizontal || isVertical) ? validTargetColor : invalidTargetColor;
        }
        else
        {
            moveTargetIndicator.SetActive(false);
            grappleTargetIndicator.SetActive(false);
            moveButton.interactable = false;
            grappleButton.interactable = false;

            if (reticleUI != null)
                reticleUI.color = invalidTargetColor;
        }
    }

    // 🔹 Move button pressed
    void OnMoveButtonPressed()
    {
        if (!moveButton.interactable) return;

        CancelCurrentGrapple();
        isProjectileFlying = false;
        isGrappling = false;

        moveTarget = moveTargetIndicator.transform.position;
        isMoving = true;
        rb.isKinematic = false;
    }

    // 🔹 Grapple button pressed
    void OnGrappleButtonPressed()
    {
        if (!grappleButton.interactable) return;

        CancelCurrentGrapple();
        isMoving = false;

        grappleTarget = grappleTargetIndicator.transform.position;
        grappleRoutine = StartCoroutine(ShootGrappleProjectile());
    }

    // 🔹 Launch the grapple projectile
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

    // 🔹 Grapple hold logic
    IEnumerator GrappleDuration()
    {
        while (Vector3.Distance(transform.position, grappleTarget) > stopDistance)
        {
            transform.position = Vector3.MoveTowards(transform.position, grappleTarget, grappleSpeed * Time.deltaTime);
            UpdateLineRenderer(grappleTarget);
            yield return null;
        }

        yield return new WaitForSeconds(grappleHoldTime);
        EndGrapple();
    }

    // 🔹 Cancel any grapple
    void CancelCurrentGrapple()
    {
        if (grappleRoutine != null)
            StopCoroutine(grappleRoutine);

        if (currentProjectile)
            Destroy(currentProjectile);

        if (grappleLine != null)
            grappleLine.enabled = false;

        rb.isKinematic = false;
        ropeVelocity = Vector3.zero;
        isGrappling = false;
        isProjectileFlying = false;
    }

    // 🔹 End grappling normally
    void EndGrapple()
    {
        isGrappling = false;
        rb.isKinematic = false;
        ropeVelocity = Vector3.zero;

        if (grappleLine != null)
            grappleLine.enabled = false;

        if (currentProjectile)
            Destroy(currentProjectile);
    }

    // 🔹 Horizontal movement
    void HandleMovement()
    {
        if (!isMoving) return;

        Vector3 direction = (moveTarget - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        if (Vector3.Distance(transform.position, moveTarget) < stopDistance)
            isMoving = false;
    }

    // 🔹 Grapple rope visuals
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
        if (grappleLine == null) return;

        grappleLine.enabled = true;
        grappleLine.SetPosition(0, transform.position);
        grappleLine.SetPosition(1, ropeEnd);
    }
}
