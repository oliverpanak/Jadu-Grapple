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
    public LineRenderer grappleLine;
    public Image reticleUI;

    [Header("Target Indicators")]
    public GameObject moveTargetIndicator;
    public GameObject grappleTargetIndicator;

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

        moveButton.onClick.AddListener(OnMoveButtonPressed);
        grappleButton.onClick.AddListener(OnGrappleButtonPressed);

        moveTargetIndicator.SetActive(false);
        grappleTargetIndicator.SetActive(false);

        if (grappleLine != null)
        {
            grappleLine.enabled = false;
            grappleLine.positionCount = 2;
        }
    }

    void Update()
    {
        if (!isProjectileFlying)
            UpdateTargetIndicators();

        HandleMovement();
        HandleGrapple();
    }

    // 🔹 Target indicators + UI state
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

        // Interrupt any current movement or grapple
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

        // Interrupt any walk or existing grapple
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

        // Projectile flies toward target
        while (currentProjectile && Vector3.Distance(currentProjectile.transform.position, grappleTarget) > 0.1f)
        {
            currentProjectile.transform.position = Vector3.MoveTowards(
                currentProjectile.transform.position,
                grappleTarget,
                projectileSpeed * Time.deltaTime
            );

            if (grappleLine != null)
            {
                grappleLine.enabled = true;
                grappleLine.SetPosition(0, transform.position);
                grappleLine.SetPosition(1, currentProjectile.transform.position);
            }

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

    // 🔹 Hold grapple for a few seconds, then release
    IEnumerator GrappleDuration()
    {
        // Move toward grapple point
        while (Vector3.Distance(transform.position, grappleTarget) > stopDistance)
        {
            transform.position = Vector3.MoveTowards(transform.position, grappleTarget, grappleSpeed * Time.deltaTime);
            yield return null;
        }

        // Hold for a few seconds before letting go
        yield return new WaitForSeconds(grappleHoldTime);
        EndGrapple();
    }

    // 🔹 Cancel any ongoing grapple or projectile
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

    // 🔹 Horizontal movement logic
    void HandleMovement()
    {
        if (!isMoving) return;

        Vector3 direction = (moveTarget - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        if (Vector3.Distance(transform.position, moveTarget) < stopDistance)
            isMoving = false;
    }

    // 🔹 Grapple rope simulation
    void HandleGrapple()
    {
        if (!isGrappling) return;

        if (grappleLine != null)
        {
            Vector3 displacement = grappleTarget - currentRopeEnd;
            Vector3 springForce = displacement * ropeTension;
            ropeVelocity += springForce * Time.deltaTime;
            ropeVelocity *= Mathf.Exp(-ropeDamping * Time.deltaTime);
            currentRopeEnd += ropeVelocity * Time.deltaTime;

            grappleLine.SetPosition(0, transform.position);
            grappleLine.SetPosition(1, currentRopeEnd);
        }
    }
}
