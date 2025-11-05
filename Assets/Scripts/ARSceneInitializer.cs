using UnityEngine;
using UnityEngine.UI;

public class ARSceneInitializer : MonoBehaviour
{
    [Header("References")]
    public Camera arCamera;

    [Header("Prefabs")]
    public GameObject characterSetupPrefab;
    public GameObject environmentPrefab;

    [Header("UI Elements")]
    public Button placeButton;
    public SettingsMenuController settingsMenuController;

    [Header("Placement Cursor")]
    public GameObject placementCursorPrefab;
    public Color validPlacementColor = Color.green;
    public Color invalidPlacementColor = Color.red;
    public float maxPlacementDistance = 5f;
    public float placementYOffset = 0.01f;

    private GameObject placementCursor;
    private Renderer cursorRenderer;
    private bool validPlacement = false;
    private Vector3 placementPosition;

    private GameObject spawnedCharacterSetup;
    private GameObject spawnedEnvironment;

    void Start()
    {
        // 🔹 Create placement cursor
        if (placementCursorPrefab != null)
        {
            placementCursor = Instantiate(placementCursorPrefab);
            placementCursor.SetActive(false);

            cursorRenderer = placementCursor.GetComponentInChildren<Renderer>();
            if (cursorRenderer != null)
                cursorRenderer.material.color = invalidPlacementColor;
        }

        // 🔹 Setup Place button
        if (placeButton != null)
        {
            placeButton.interactable = false;
            placeButton.onClick.AddListener(OnPlaceButtonPressed);
        }
    }

    void Update()
    {
        UpdatePlacementCursor();
    }

    // 🔹 Update placement cursor and validity
    void UpdatePlacementCursor()
    {
        Ray ray = new Ray(arCamera.transform.position, arCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxPlacementDistance))
        {
            bool isHorizontalSurface = Vector3.Dot(hit.normal, Vector3.up) > 0.8f;

            if (isHorizontalSurface)
            {
                placementPosition = hit.point + Vector3.up * placementYOffset;
                placementCursor.SetActive(true);
                placementCursor.transform.position = placementPosition;
                placementCursor.transform.rotation = Quaternion.identity;

                if (cursorRenderer != null)
                    cursorRenderer.material.color = validPlacementColor;

                validPlacement = true;
                if (placeButton != null)
                    placeButton.interactable = true;
                return;
            }
        }

        // If no valid surface hit
        validPlacement = false;
        if (placementCursor != null)
            placementCursor.SetActive(false);

        if (placeButton != null)
            placeButton.interactable = false;
    }

    // 🔹 Called when the player presses the Place button
    void OnPlaceButtonPressed()
    {
        if (!validPlacement) return;

        // Spawn environment
        if (environmentPrefab != null && spawnedEnvironment == null)
        {
            spawnedEnvironment = Instantiate(environmentPrefab, placementPosition, Quaternion.identity);
        }
        
        // Hide placement UI
        if (placementCursor != null)
            placementCursor.SetActive(false);

        if (placeButton != null)
            placeButton.gameObject.SetActive(false);

        // Spawn character
        if (characterSetupPrefab != null && spawnedCharacterSetup == null)
        {
            spawnedCharacterSetup = Instantiate(characterSetupPrefab, placementPosition, Quaternion.identity);
            spawnedCharacterSetup.GetComponent<ARCharacterSetup>().characterController.arCamera = arCamera;
            settingsMenuController.characterController = spawnedCharacterSetup.GetComponent<ARCharacterSetup>().characterController;
            settingsMenuController.SyncUIWithCharacter();
            Destroy(this);
        }
    }
}
