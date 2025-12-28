using Input;
using Managers;
using UnityEngine;

public class Quit : MonoBehaviour, IInputListener
{
    [SerializeField] [Tooltip("InputManager 里配置的退出按钮名")]
    private string quitButton = "Cancel"; // 默认使用旧输入系统内置的 Cancel（常为 Escape）
    

    public void OnInputEvent(InputEvents inputEvent, InputState state)
    {
        if (inputEvent == InputEvents.Exit && (state == InputState.Started || state == InputState.Performed))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
        }
    }

    public void OnInputAxis(InputAxis axis, Vector2 value)
    {
    }

    public int InputPriority => 0;
    public bool IsInputEnabled => true;
}