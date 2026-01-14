using UnityEngine;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
public class PortalRenderFeatureSwitcher : MonoBehaviour
{
    [Header("In-World Portal Stencils")]
    public ScriptableRendererFeature InRealWorldPortalStencil;
    public ScriptableRendererFeature InVirtualWorldPortalStencil;

    [Header("In-World Inverse Stencils")]
    public ScriptableRendererFeature InRealWorldInverseStencil;
    public ScriptableRendererFeature InVirtualWorldInverseStencil;

    [Header("References")]
    public Transform portalTransform;
    public Transform playerCamera;

    [Header("Look Test")]
    [Range(0f, 1f)]
    public float lookDotThreshold = 0.4f;

    private enum PlayerWorld
    {
        InRealWorld,
        InVirtualWorld
    }

    [SerializeField]
    private PlayerWorld currentWorld = PlayerWorld.InRealWorld;

    // Editor safety
    private bool initRealPortal, initVirtualPortal;
    private bool initRealInverse, initVirtualInverse;

    // Determines switch order during this crossing
    private bool switchPortalFirst;

    private void OnEnable()
    {
        CacheInitialStates();
        ApplyWorldState(currentWorld, immediate: true);
    }

    private void OnDisable()
    {
        RestoreInitialStates();
    }

#if UNITY_EDITOR
    private void OnDestroy()
    {
        RestoreInitialStates();
    }
#endif

    // =========================
    // PORTAL CROSSING
    // =========================

    public void BeginPortalCrossing()
    {
        // If NOT looking at the portal, switch portal stencil first
        switchPortalFirst = !IsPlayerLookingAtPortal();

        if (switchPortalFirst)
            SwitchPortalStencils();
        else
            SwitchInverseStencils();
    }

    public void CompletePortalCrossing()
    {
        if (switchPortalFirst)
            SwitchInverseStencils();
        else
            SwitchPortalStencils();

        currentWorld = currentWorld == PlayerWorld.InRealWorld
            ? PlayerWorld.InVirtualWorld
            : PlayerWorld.InRealWorld;
    }

    // =========================
    // STENCIL SWITCHING
    // =========================

    private void SwitchPortalStencils()
    {
        if (currentWorld == PlayerWorld.InRealWorld)
        {
            InRealWorldPortalStencil.SetActive(false);
            InVirtualWorldPortalStencil.SetActive(true);
        }
        else
        {
            InVirtualWorldPortalStencil.SetActive(false);
            InRealWorldPortalStencil.SetActive(true);
        }
    }

    private void SwitchInverseStencils()
    {
        if (currentWorld == PlayerWorld.InRealWorld)
        {
            InRealWorldInverseStencil.SetActive(false);
            InVirtualWorldInverseStencil.SetActive(true);
        }
        else
        {
            InVirtualWorldInverseStencil.SetActive(false);
            InRealWorldInverseStencil.SetActive(true);
        }
    }

    private void ApplyWorldState(PlayerWorld world, bool immediate)
    {
        if (!immediate) return;

        bool inRealWorld = world == PlayerWorld.InRealWorld;

        InRealWorldPortalStencil.SetActive(inRealWorld);
        InRealWorldInverseStencil.SetActive(inRealWorld);

        InVirtualWorldPortalStencil.SetActive(!inRealWorld);
        InVirtualWorldInverseStencil.SetActive(!inRealWorld);
    }

    // =========================
    // LOOK TEST
    // =========================

    private bool IsPlayerLookingAtPortal()
    {
        Vector3 toPortal = portalTransform.position - playerCamera.position;
        toPortal.Normalize();

        float dot = Vector3.Dot(playerCamera.forward, toPortal);
        return dot >= lookDotThreshold;
    }

    // =========================
    // EDITOR SAFETY
    // =========================

    private void CacheInitialStates()
    {
        if (!InRealWorldPortalStencil) return;

        initRealPortal = InRealWorldPortalStencil.isActive;
        initVirtualPortal = InVirtualWorldPortalStencil.isActive;
        initRealInverse = InRealWorldInverseStencil.isActive;
        initVirtualInverse = InVirtualWorldInverseStencil.isActive;
    }

    private void RestoreInitialStates()
    {
        if (!InRealWorldPortalStencil) return;

        InRealWorldPortalStencil.SetActive(initRealPortal);
        InVirtualWorldPortalStencil.SetActive(initVirtualPortal);
        InRealWorldInverseStencil.SetActive(initRealInverse);
        InVirtualWorldInverseStencil.SetActive(initVirtualInverse);
    }
}
