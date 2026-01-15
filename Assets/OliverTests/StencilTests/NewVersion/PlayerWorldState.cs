using UnityEngine;

public class PlayerWorldState : MonoBehaviour
{
    public enum PlayerWorld
    {
        InRealWorld,
        InVirtualWorld
    }

    public PlayerWorld CurrentWorld = PlayerWorld.InRealWorld;
}