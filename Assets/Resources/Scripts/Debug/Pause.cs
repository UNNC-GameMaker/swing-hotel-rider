using Input;
using Managers;
using UnityEngine;

public class Pause : MonoBehaviour, IInputListener
{
    [SerializeField]
    private string quitButton = "Cancel"; 
    
    private void Start()
    {
        if (GameManager.Instance != null)
        {
            var inputManager = GameManager.Instance.GetManager<InputManager>();
            if (inputManager != null)
            {
                inputManager.RegisterListener(this);
            }
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            var inputManager = GameManager.Instance.GetManager<InputManager>();
            if (inputManager != null)
            {
                inputManager.UnregisterListener(this);
            }
        }
    }

    public void OnInputEvent(InputEvents inputEvent, InputState state)
    {
        if ((inputEvent == InputEvents.Exit || inputEvent == InputEvents.Pause) && (state == InputState.Started || state == InputState.Performed))
        {
            GameManager.Instance.LevelPause();
        }
    }

    public void OnInputAxis(InputAxis axis, Vector2 value)
    {
    }

    public int InputPriority => 100;
    public bool IsInputEnabled => true;
}