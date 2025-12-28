using UnityEditor;
using UnityEngine;

public class Quit : MonoBehaviour
{
    [SerializeField] [Tooltip("InputManager 里配置的退出按钮名")]
    private string quitButton = "Cancel"; // 默认使用旧输入系统内置的 Cancel（常为 Escape）

    private void Update()
    {
        if (Input.GetButtonDown(quitButton))
        {
            Debug.Log("Quit function called - exiting play mode");

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}