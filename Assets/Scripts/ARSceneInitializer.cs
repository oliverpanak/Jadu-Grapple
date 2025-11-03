using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

[RequireComponent(typeof(ARRaycastManager))]
public class ARSceneInitializer : MonoBehaviour
{
    [Header("References")]
    public Camera arCamera;
    public ARCharacterController characterController;

    [Header("Prefabs")]
    public GameObject characterPrefab;
    public GameObject environmentPrefab;

    [Header("UI Elements")]
    public Button placeButton;
    public GameObject moveButton;
    public GameObject grappleButton;

    [Header("Placement Cursor")]
    public GameObject placementCursorPrefab;
    public Color validPlacementColor = Color.green;
    public Color invalidPlacementColor = Color.red;
    public float placementYOffset = 0.01f;

    private ARRaycastManager raycastManager;
    private Pose placementPose;
    private bool placementPoseIsValid = false;
    private GameObject placementCursor;
    private Renderer cursorRenderer;

    private GameObject spawnedCharacter;
    private GameObject spawnedEnvironment;

    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
    }

    void Start()
    {
        // Instantiate the placement cursor
        placementCursor = Instantiate(placementCursorPrefab);
        placementCursor.SetActive(false);
        cursorRenderer = placementCursor.GetComponentInChildren<Renderer>();

        // Disable move/grapple buttons until placement is complete
        if (moveButton != null) moveButton.SetActive(false);
        if (grappleButton != null) grappleButton.SetActive(false);

        // Setup the Place button
        if (placeButton != null)
        {
            placeButton.interactable = false;
            placeButton.onClick.AddListener(OnPlaceButtonPressed);
        }
    }

    void Update()
    {
        UpdatePlacementPose();
        UpdatePlacementCursor();
    }

    // 🔹 Update plane raycast and find valid placement pose
    void UpdatePlacementPose()
    {
        var screenCenter = arCamera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            placementPoseIsValid = true;
            placementPose = hits[0].pose;

            // Optional: rotate placement to face the camera
            Vector3 cameraForward = arCamera.transform.forward;
            Vector3 cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            placementPose.rotation = Quaternion.LookRotation(cameraBearing);
        }
        else
        {
            placementPoseIsValid = false;
        }
    }

    // 🔹 Update cursor visual + button state
    void UpdatePlacementCursor()
    {
        if (placementPoseIsValid)
        {
            placementCursor.SetActive(true);
            placementCursor.transform.SetPositionAndRotation(
                placementPose.position + Vector3.up * placementYOffset,
                placementPose.rotation
            );

            if (cursorRenderer != null)
                cursorRenderer.material.color = validPlacementColor;

            if (placeButton != null)
                placeButton.interactable = true;
        }
        else
        {
            placementCursor.SetActive(false);
            if (placeButton != null)
                placeButton.interactable = false;
        }
    }

    // 🔹 Called when player presses the "Place" button
    void OnPlaceButtonPressed()
    {
        if (!placementPoseIsValid) return;

        Vector3 placementPosition = placementPose.position + Vector3.up * placementYOffset;

        // Spawn environment prefab
        if (environmentPrefab != null && spawnedEnvironment == null)
        {
            spawnedEnvironment = Instantiate(environmentPrefab, placementPosition, placementPose.rotation);
        }

        // Spawn character prefab
        if (characterPrefab != null && spawnedCharacter == null)
        {
            spawnedCharacter = Instantiate(characterPrefab, placementPosition, placementPose.rotation);
            characterController = spawnedCharacter.GetComponent<ARCharacterController>();
        }

        // Disable cursor and place button
        placementCursor.SetActive(false);
        if (placeButton != null)
            placeButton.gameObject.SetActive(false);

        // Enable gameplay buttons
        if (moveButton != null)
        {
            moveButton.SetActive(true);
            characterController.moveButton = moveButton.GetComponent<Button>();
        }

        if (grappleButton != null)
        {
            grappleButton.SetActive(true);
            characterController.grappleButton = grappleButton.GetComponent<Button>();
        }
        characterController.arCamera = arCamera;
    }
}
