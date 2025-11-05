using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


public class LobbyUI : MonoBehaviour
{
    public SceneAsset grappleScene;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadGrappleScene()
    {
        SceneManager.LoadScene(grappleScene.name);
    }
}
