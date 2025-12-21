using UnityEditor;
using UnityEngine;

public class Quit : MonoBehaviour
{
    private PlayerInput _playerInput;

    private void Awake()
    {
        // Initialize the PlayerInput instance
        _playerInput = new PlayerInput();
        Debug.Log("Quit script initialized");
    }

    // Update is called once per frame
    private void Update()
    {
        if (_playerInput.UI.Quit.triggered)
        {
            Debug.Log("Quit function called - exiting play mode");

#if UNITY_EDITOR
            // If running in the Unity Editor, stop play mode
            EditorApplication.isPlaying = false;
#else
            // If running as a build, quit the application
            Application.Quit();
#endif
        }
    }

    private void OnEnable()
    {
        // Enable the input actions
        _playerInput.Enable();
    }

    private void OnDisable()
    {
        // Disable the input actions to prevent memory leaks
        _playerInput.Disable();
    }
}