using Managers;
using UnityEngine;

namespace UI
{
    public class PauseMenuProxy : MonoBehaviour
    {
        public void Resume()
        {
            if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
        }

        public void ReturnToSelect()
        {
            if (GameManager.Instance != null) GameManager.Instance.ReturnToSelectScene();
        }
    }
}
