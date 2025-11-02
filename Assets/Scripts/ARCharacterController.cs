using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
    public GameObject grappleProjectilePrefab; // small hook prefab
    public float projectileSpeed = 10f;

    [Header("Movement Settings")]
    public float moveSpeed = 1.5f;
    public float grappleSpeed = 3f;
    public float stopDistance = 0.05f;
    public float rotationSpeed = 5f;
    public float ropeTension = 10f;
    public float ropeDamping = 5f;

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

    void Start()
    {
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
        if (!isMoving && !isGrappling && !isProjectileFlying)
            UpdateTargetIndicators();

        HandleMovement();
        HandleGrapple();
    }

    // 🔹 Update target indicators and button states
    void UpdateTargetIndicators()
    {
        Ray ray = new Ray(arCamera.transform.position, arCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            bool isHorizontal = Vector3.Dot(hit.normal, Vector3.up) > 0.8f;
            bool isVertical = Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up)) < 0.3f;

            // Update move target
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

            // Update grapple target
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

            // Reticle color logic
            if (reticleUI != null)
            {
                if (isHorizontal || isVertical)
                    reticleUI.color = validTargetColor;
                else
                    reticleUI.color = invalidTargetColor;
            }
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
        if (!moveButton.interactable || isGrappling) return;

        moveTarget = moveTargetIndicator.transform.position;
        isMoving = true;
    }

    // 🔹 Grapple button pressed
    void OnGrappleButtonPressed()
    {
        if (!grappleButton.interactable) return;

        if (!isGrappling && !isProjectileFlying)
        {
            grappleTarget = grappleTargetIndicator.transform.position;
            StartCoroutine(ShootGrappleProjectile());
        }
        else if (isGrappling)
        {
            // Release grapple
            isGrappling = false;
            if (grappleLine != null)
                grappleLine.enabled = false;
        }
    }

    // 🔹 Launch the grapple projectile
    IEnumerator ShootGrappleProjectile()
    {
        isProjectileFlying = true;

        // Spawn projectile
        if (grappleProjectilePrefab)
        {
            currentProjectile = Instantiate(grappleProjectilePrefab, transform.position, Quaternion.identity);
        }

        // Fly toward target
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

        // Snap to target
        if (currentProjectile)
            currentProjectile.transform.position = grappleTarget;

        // Begin grapple pull
        isProjectileFlying = false;
        isGrappling = true;
        currentRopeEnd = transform.position;
    }

    // 🔹 Handle normal movement
    void HandleMovement()
    {
        if (!isMoving) return;

        Vector3 direction = (moveTarget - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        // Face direction
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }

        if (Vector3.Distance(transform.position, moveTarget) < stopDistance)
            isMoving = false;
    }

    // 🔹 Handle grappling
    void HandleGrapple()
    {
        if (!isGrappling) return;

        Vector3 direction = (grappleTarget - transform.position).normalized;
        transform.position = Vector3.MoveTowards(transform.position, grappleTarget, grappleSpeed * Time.deltaTime);

        // Face grapple direction
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }

        // Rope spring simulation
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
