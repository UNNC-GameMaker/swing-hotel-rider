using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelect : MonoBehaviour
{
    [Header("Optional: if empty, use Build Settings order")]
    [SerializeField] private string[] sceneNames;

    public void LoadLevel(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("LevelSelect.LoadLevel called with empty scene name.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    public void LoadLevelByIndex(int buildIndex)
    {
        if (buildIndex < 0 || buildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning($"Invalid build index: {buildIndex}");
            return;
        }

        SceneManager.LoadScene(buildIndex);
    }

    public void LoadLevelFromList(int listIndex)
    {
        if (sceneNames == null || sceneNames.Length == 0)
        {
            Debug.LogWarning("LevelSelect.sceneNames is empty.");
            return;
        }

        if (listIndex < 0 || listIndex >= sceneNames.Length)
        {
            Debug.LogWarning($"Invalid list index: {listIndex}");
            return;
        }

        LoadLevel(sceneNames[listIndex]);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
