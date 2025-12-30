using Input;
using Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour, IInputListener
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private string selectSceneName = "select";

    private bool _isPaused;

    private void Start()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GetManager<InputManager>().RegisterListener(this);
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GetManager<InputManager>().UnregisterListener(this);
        }
    }

    public void OnInputEvent(InputEvents inputEvent, InputState state)
    {
        if (inputEvent == InputEvents.Pause && state == InputState.Started)
        {
            TogglePause();
        }
    }

    public void OnInputAxis(InputAxis axis, Vector2 value)
    {
    }

    public int InputPriority => 10;
    public bool IsInputEnabled => true;

    public void TogglePause()
    {
        SetPaused(!_isPaused);
    }

    public void Resume()
    {
        SetPaused(false);
    }

    public void Pause()
    {
        SetPaused(true);
    }

    public void ReturnToSelect()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(selectSceneName);
    }

    private void SetPaused(bool paused)
    {
        _isPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
        if (pausePanel != null)
        {
            pausePanel.SetActive(paused);
        }
    }
}
