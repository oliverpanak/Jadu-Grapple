using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PortalWorldSwitcher : MonoBehaviour
{
    [Header("Player Settings")]
    public string playerTag = "Player";

    [Header("Renderer Features")]
    public ScriptableRendererFeature stencilToOtherWorld1;
    public ScriptableRendererFeature stencilToOtherWorld2;
    public ScriptableRendererFeature stencilToThisWorld1;
    public ScriptableRendererFeature stencilToThisWorld2;

    private bool playerInWorld1 = true;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log("Player entered portal trigger: " + other.name);
            FlipWorldState();
        }
    }

    void FlipWorldState()
    {
        playerInWorld1 = !playerInWorld1;

        if (playerInWorld1)
        {
            stencilToOtherWorld1.SetActive(true);
            stencilToThisWorld2.SetActive(true);

            stencilToOtherWorld2.SetActive(false);
            stencilToThisWorld1.SetActive(false);
        }
        else
        {
            stencilToOtherWorld1.SetActive(false);
            stencilToThisWorld2.SetActive(false);

            stencilToOtherWorld2.SetActive(true);
            stencilToThisWorld1.SetActive(true);
        }

        Debug.Log("World flipped. Player now in world1: " + playerInWorld1);
    }
}