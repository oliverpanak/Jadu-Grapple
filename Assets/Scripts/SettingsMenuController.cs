using UnityEngine;
using UnityEngine.UI;
using TMPro; // ✅ TextMeshPro namespace

public class SettingsMenuController : MonoBehaviour
{
    [Header("Character Reference")]
    public ARCharacterController characterController;

    [Header("UI Elements")]
    public GameObject settingsPanel;
    
    public Slider grappleSpeedSlider;
    public TMP_Text grappleSpeedValueText;

    public Slider maxDistanceSlider;
    public TMP_Text maxDistanceValueText;

    public Slider holdTimeSlider;
    public TMP_Text holdTimeValueText;

    public Slider stopDistanceSlider;
    public TMP_Text stopDistanceValueText;

    public Toggle physicsToggle;
    
    public Toggle momentumAfterGrappleToggle;

    public Slider moveSpeedSlider;
    public TMP_Text moveSpeedValueText;

    private void Start()
    {
        settingsPanel.SetActive(false);
        
        // if (characterController == null)
        // {
        //     Debug.LogError("⚠️ SettingsMenuController: CharacterController reference not set!");
        //     return;
        // }

        // Initialize value texts based on slider defaults
        UpdateAllValueTexts();

        // Subscribe to slider & toggle events
        if (grappleSpeedSlider) grappleSpeedSlider.onValueChanged.AddListener(OnGrappleSpeedChanged);
        if (maxDistanceSlider) maxDistanceSlider.onValueChanged.AddListener(OnMaxDistanceChanged);
        if (holdTimeSlider) holdTimeSlider.onValueChanged.AddListener(OnHoldTimeChanged);
        if (stopDistanceSlider) stopDistanceSlider.onValueChanged.AddListener(OnStopDistanceChanged);
        if (moveSpeedSlider) moveSpeedSlider.onValueChanged.AddListener(OnMoveSpeedChanged);
        if (physicsToggle) physicsToggle.onValueChanged.AddListener(OnPhysicsToggled);
        if (momentumAfterGrappleToggle) momentumAfterGrappleToggle.onValueChanged.AddListener(MomentumAfterGrappleToggled);

        // // Apply the initial settings to the character
        // ApplyAllSettings();
    }

    private void UpdateAllValueTexts()
    {
        if (grappleSpeedSlider && grappleSpeedValueText)
            grappleSpeedValueText.text = $"{grappleSpeedSlider.value:F1}";

        if (maxDistanceSlider && maxDistanceValueText)
            maxDistanceValueText.text = $"{maxDistanceSlider.value:F1}";

        if (holdTimeSlider && holdTimeValueText)
            holdTimeValueText.text = $"{holdTimeSlider.value:F1}s";

        if (stopDistanceSlider && stopDistanceValueText)
            stopDistanceValueText.text = $"{stopDistanceSlider.value:F2}m";

        if (moveSpeedSlider && moveSpeedValueText)
            moveSpeedValueText.text = $"{moveSpeedSlider.value:F1}";
    }

    private void OnGrappleSpeedChanged(float value)
    {
        if (characterController)
            characterController.grappleSpeed = value;

        if (grappleSpeedValueText)
            grappleSpeedValueText.text = $"{value:F1}";
    }

    private void OnMaxDistanceChanged(float value)
    {
        if (characterController)
            characterController.maxGrappleDistance = value;
        
        if (maxDistanceValueText)
            maxDistanceValueText.text = $"{value:F1}";
    }

    private void OnHoldTimeChanged(float value)
    {
        if (characterController)
            characterController.grappleHoldTime = value;

        if (holdTimeValueText)
            holdTimeValueText.text = $"{value:F1}s";
    }

    private void OnStopDistanceChanged(float value)
    {
        if (characterController)
            characterController.stopDistance = value;

        if (stopDistanceValueText)
            stopDistanceValueText.text = $"{value:F2}m";
    }

    private void OnMoveSpeedChanged(float value)
    {
        if (characterController)
            characterController.moveSpeed = value;

        if (moveSpeedValueText)
            moveSpeedValueText.text = $"{value:F1}";
    }

    private void OnPhysicsToggled(bool isOn)
    {
        if (characterController)
            characterController.usePhysicsMovement = isOn;
    }
    
    private void MomentumAfterGrappleToggled(bool isOn)
    {
        if (characterController)
            characterController.retainMomentumAfterGrapple = isOn;
    }
}
