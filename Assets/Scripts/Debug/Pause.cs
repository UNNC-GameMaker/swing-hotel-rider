using System;
using Input;
using Managers;
using UnityEngine;

namespace Debug
{
    public class Pause : MonoBehaviour, IInputListener
    {
        [SerializeField] private string quitButton = "Cancel";

        private bool _pauseConsumed;

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                var inputManager = GameManager.Instance.GetManager<InputManager>();
                if (inputManager != null) inputManager.RegisterListener(this);
            }
        }
        
        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                var inputManager = GameManager.Instance.GetManager<InputManager>();
                if (inputManager != null) inputManager.UnregisterListener(this);
            }
        }

        public void OnInputEvent(InputEvents inputEvent, InputState state)
        {
            if (inputEvent != InputEvents.Exit && inputEvent != InputEvents.Pause) return;

            if (state == InputState.Canceled)
            {
                _pauseConsumed = false;
                return;
            }

            if (_pauseConsumed) return;
            _pauseConsumed = true;
            GameManager.Instance.LevelPause();
        }

        public void OnInputAxis(InputAxis axis, Vector2 value)
        {
        }

        public int InputPriority => 100;
        public bool IsInputEnabled => true;
    }
}
