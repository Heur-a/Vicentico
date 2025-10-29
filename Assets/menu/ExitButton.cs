using UnityEngine;
using UnityEngine.InputSystem;

public class QuitApp : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ExitGame();
        }
    }
    
    void ExitGame()
    {
        Debug.Log("Quitting");
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
