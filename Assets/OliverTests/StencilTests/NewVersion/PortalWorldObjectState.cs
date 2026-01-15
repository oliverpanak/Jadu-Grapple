using UnityEngine;

public class PortalWorldObjectState : MonoBehaviour
{
    public enum ObjectWorld
    {
        RealWorld,
        VirtualWorld
    }

    public ObjectWorld objectWorld;

    private PortalInteractable interactable;

    private void Awake()
    {
        interactable = GetComponent<PortalInteractable>();
    }

    private void OnEnable()
    {
        PortalWorldEvents.OnWorldChanged += OnWorldChanged;
    }

    private void OnDisable()
    {
        PortalWorldEvents.OnWorldChanged -= OnWorldChanged;
    }

    private void OnWorldChanged(bool inRealWorld)
    {
        bool objectIsInPlayerWorld =
            (inRealWorld && objectWorld == ObjectWorld.RealWorld) ||
            (!inRealWorld && objectWorld == ObjectWorld.VirtualWorld);

        interactable.SetInteractable(objectIsInPlayerWorld);
    }
}