using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PortalRenderFeatureSwitcher : MonoBehaviour
{
    [Header("URP Renderer")]
    public UniversalRendererData rendererData;

    [Header("Render Features")]
    public ScriptableRendererFeature stencilToOtherWorld1;
    public ScriptableRendererFeature stencilToOtherWorld2;
    public ScriptableRendererFeature stencilToThisWorld1;
    public ScriptableRendererFeature stencilToThisWorld2;

    private bool inOtherWorld = false;

    public void EnterPortal()
    {
        // FIRST: switch "This World" rendering
        stencilToThisWorld1.SetActive(true);
        stencilToThisWorld2.SetActive(false);
    }

    public void ExitPortal()
    {
        // SECOND: switch "Other World" rendering
        inOtherWorld = !inOtherWorld;

        stencilToOtherWorld1.SetActive(!inOtherWorld);
        stencilToOtherWorld2.SetActive(inOtherWorld);
    }
}
