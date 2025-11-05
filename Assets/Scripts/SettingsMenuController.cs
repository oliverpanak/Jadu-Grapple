using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SettingsMenuController : MonoBehaviour
{
    public GameObject settingsPanel;
    
    [Header("Character Reference")]
    public ARCharacterController characterController;

    [Header("Grapple Settings")]
    public Slider grappleSpeedSlider;
    public TMP_Text grappleSpeedValueText;

    public Slider maxDistanceSlider;
    public TMP_Text maxDistanceValueText;

    public Slider holdTimeSlider;
    public TMP_Text holdTimeValueText;

    public Slider stopDistanceSlider;
    public TMP_Text stopDistanceValueText;

    public Toggle retainMomentumToggle;

    [Header("Movement Settings")]
    public Slider moveSpeedSlider;
    public TMP_Text moveSpeedValueText;

    public Toggle usePhysicsMovementToggle;

    [Header("Gravity Boots Settings")]
    public Toggle gravityBootsToggle;
    public Slider gravityStrengthSlider;
    public TMP_Text gravityStrengthValueText;

    [Header("Jump Settings")]
    public Slider jumpForceSlider;
    public TMP_Text jumpForceValueText;

    [Header("Momentum Toggle")]
    public Toggle kinematicToggle; // Enables/disables Rigidbody kinematic

    public void LoadLobbyScene()
    {
        SceneManager.LoadScene(0);
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
    }
    
    private void Start()
    {
        settingsPanel.SetActive(false);
        // if (characterController == null)
        // {
        //     Debug.LogError("⚠️ SettingsMenuController: CharacterController reference not set!");
        //     return;
        // }

        // Initialize UI listeners
        SetupUIListeners();

        // Sync UI values with current character settings
        // SyncUIWithCharacter();
    }

    private void SetupUIListeners()
    {
        // Grapple
        if (grappleSpeedSlider) grappleSpeedSlider.onValueChanged.AddListener(OnGrappleSpeedChanged);
        if (maxDistanceSlider) maxDistanceSlider.onValueChanged.AddListener(OnMaxDistanceChanged);
        if (holdTimeSlider) holdTimeSlider.onValueChanged.AddListener(OnHoldTimeChanged);
        if (stopDistanceSlider) stopDistanceSlider.onValueChanged.AddListener(OnStopDistanceChanged);
        if (retainMomentumToggle) retainMomentumToggle.onValueChanged.AddListener(OnRetainMomentumToggled);

        // Movement
        if (moveSpeedSlider) moveSpeedSlider.onValueChanged.AddListener(OnMoveSpeedChanged);
        if (usePhysicsMovementToggle) usePhysicsMovementToggle.onValueChanged.AddListener(OnUsePhysicsMovementToggled);
        if (kinematicToggle) kinematicToggle.onValueChanged.AddListener(OnKinematicToggled);

        // Gravity Boots
        if (gravityBootsToggle) gravityBootsToggle.onValueChanged.AddListener(OnGravityBootsToggled);
        if (gravityStrengthSlider) gravityStrengthSlider.onValueChanged.AddListener(OnGravityStrengthChanged);

        // Jump
        if (jumpForceSlider) jumpForceSlider.onValueChanged.AddListener(OnJumpForceChanged);
    }

    public void SyncUIWithCharacter()
    {
        // Grapple
        if (grappleSpeedSlider)
        {
            grappleSpeedSlider.value = characterController.grappleSpeed;
            grappleSpeedValueText.text = $"{characterController.grappleSpeed:F1}";
        }

        if (maxDistanceSlider)
        {
            maxDistanceSlider.value = characterController.maxGrappleDistance;
            maxDistanceValueText.text = $"{characterController.maxGrappleDistance:F1}";
        }

        if (holdTimeSlider)
        {
            holdTimeSlider.value = characterController.grappleHoldTime;
            holdTimeValueText.text = $"{characterController.grappleHoldTime:F1}s";
        }

        if (stopDistanceSlider)
        {
            stopDistanceSlider.value = characterController.stopDistance;
            stopDistanceValueText.text = $"{characterController.stopDistance:F2}m";
        }

        if (retainMomentumToggle)
            retainMomentumToggle.isOn = characterController.retainMomentumAfterGrapple;

        // Movement
        if (moveSpeedSlider)
        {
            moveSpeedSlider.value = characterController.moveSpeed;
            moveSpeedValueText.text = $"{characterController.moveSpeed:F1}";
        }

        if (usePhysicsMovementToggle)
            usePhysicsMovementToggle.isOn = characterController.usePhysicsMovement;

        if (kinematicToggle)
        {
            Rigidbody rb = characterController.GetComponent<Rigidbody>();
            kinematicToggle.isOn = !rb.isKinematic;
        }

        // Gravity Boots
        if (gravityBootsToggle)
            gravityBootsToggle.isOn = characterController.gravityBootsEnabled;

        if (gravityStrengthSlider)
        {
            gravityStrengthSlider.value = characterController.gravityStrength;
            gravityStrengthValueText.text = $"{characterController.gravityStrength:F1}";
        }

        // Jump
        if (jumpForceSlider)
        {
            jumpForceSlider.value = characterController.jumpForce;
            jumpForceValueText.text = $"{characterController.jumpForce:F1}";
        }
    }

    // ------------------- Listeners -------------------

    private void OnGrappleSpeedChanged(float value)
    {
        characterController.grappleSpeed = value;
        if (grappleSpeedValueText) grappleSpeedValueText.text = $"{value:F1}";
    }

    private void OnMaxDistanceChanged(float value)
    {
        characterController.maxGrappleDistance = value;
        if (maxDistanceValueText) maxDistanceValueText.text = $"{value:F1}";
    }

    private void OnHoldTimeChanged(float value)
    {
        characterController.grappleHoldTime = value;
        if (holdTimeValueText) holdTimeValueText.text = $"{value:F1}s";
    }

    private void OnStopDistanceChanged(float value)
    {
        characterController.stopDistance = value;
        if (stopDistanceValueText) stopDistanceValueText.text = $"{value:F2}m";
    }

    private void OnRetainMomentumToggled(bool isOn)
    {
        characterController.retainMomentumAfterGrapple = isOn;
    }

    private void OnMoveSpeedChanged(float value)
    {
        characterController.moveSpeed = value;
        if (moveSpeedValueText) moveSpeedValueText.text = $"{value:F1}";
    }

    private void OnUsePhysicsMovementToggled(bool isOn)
    {
        characterController.usePhysicsMovement = isOn;
    }

    private void OnKinematicToggled(bool isOn)
    {
        Rigidbody rb = characterController.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = !isOn;
    }

    private void OnGravityBootsToggled(bool isOn)
    {
        characterController.gravityBootsEnabled = isOn;
    }

    private void OnGravityStrengthChanged(float value)
    {
        characterController.gravityStrength = value;
        if (gravityStrengthValueText) gravityStrengthValueText.text = $"{value:F1}";
    }

    private void OnJumpForceChanged(float value)
    {
        characterController.jumpForce = value;
        if (jumpForceValueText) jumpForceValueText.text = $"{value:F1}";
    }
}
