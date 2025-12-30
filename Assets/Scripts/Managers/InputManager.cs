using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    /// <summary>
    /// Custom input manager using Unity's legacy Input system.
    /// Supports multiple control schemes (Keyboard, Gamepad) and broadcasts events through InputEvents.
    /// This is more reliable across different platforms than the new Input System.
    /// </summary>
    public class InputManager : Manager
    {   
        
        public override void Init()
        {
            GameManager.Instance.RegisterManager(this);
        }
        
        #region Input ListenerList
        private static readonly List<IInputListener> InputListeners = new List<IInputListener>();

        public void RegisterListener(IInputListener listener)
        {
            InputListeners.Add(listener);
        }

        public void UnregisterListener(IInputListener listener)
        {
            InputListeners.Remove(listener);
        }

        #endregion
        
        #region Control Schemes

        public enum ControlScheme
        {
            Keyboard,
            Gamepad,
            Auto // Automatically detects which control scheme is being used
        }

        [Header("Control Scheme")]
        [SerializeField] private ControlScheme currentScheme = ControlScheme.Auto;

        #endregion

        #region Input Settings

        [Header("Keyboard Settings")]
        [SerializeField] private KeyCode jumpKey = KeyCode.Space;
        [SerializeField] private KeyCode releaseKey = KeyCode.Mouse0;
        [SerializeField] private KeyCode downKey = KeyCode.S;
        [SerializeField] private KeyCode grabKey = KeyCode.F;
[SerializeField] private KeyCode pauseKey = KeyCode.Escape;

        [Header("Axis Names (Project Settings → Input Manager)")]
        [SerializeField] private string horizontalAxis = "Horizontal";
        [SerializeField] private string verticalAxis = "Vertical";
        [SerializeField] private string mouseXAxis = "Mouse X";
        [SerializeField] private string mouseYAxis = "Mouse Y";

        [Header("Gamepad Settings")]
        [SerializeField] private KeyCode gamepadJumpButton = KeyCode.JoystickButton0; // A button
        [SerializeField] private KeyCode gamepadFireButton = KeyCode.JoystickButton1; // B button
        [SerializeField] private KeyCode gamepadGrabButton = KeyCode.JoystickButton2; // X button
[SerializeField] private KeyCode gamepadPauseButton = KeyCode.JoystickButton7; // Start button
        [SerializeField] private string gamepadHorizontalAxis = "Horizontal";
        [SerializeField] private string gamepadVerticalAxis = "Vertical";
        [SerializeField] private string gamepadLookXAxis = "Mouse X";
        [SerializeField] private string gamepadLookYAxis = "Mouse Y";

        [Header("Input Thresholds")]
        [SerializeField] private float axisDeadzone = 0.01f;
        [SerializeField] private float downInputThreshold = -0.3f; // How far down axis needs to be to trigger "down"

        #endregion

        #region Cached Input State

        private Vector2 _moveInput;
        private Vector2 _lookInput;
        private bool _jumpHeld;
        private bool _releaseHeld;
        private bool _downHeld;
        private bool _grabHeld;
private bool _pauseHeld;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Initialize current state
            _moveInput = Vector2.zero;
            _lookInput = Vector2.zero;
            _jumpHeld = false;
            _releaseHeld = false;
            _downHeld = false;
            _grabHeld = false;
            _pauseHeld = false;
            
            // Initialize previous state to prevent first-frame false positives
            _prevMoveInput = Vector2.zero;
            _prevLookInput = Vector2.zero;
            _prevJumpHeld = false;
            _prevReleaseHeld = false;
            _prevDownHeld = false;
            _prevGrabHeld = false;
            _prevPauseHeld = false;
            
            Debug.Log("InputManager: Initialized with " + currentScheme + " control scheme");
        }

        private void Update()
        {
            // Auto-detect control scheme if set to Auto
            if (currentScheme == ControlScheme.Auto)
            {
                DetectControlScheme();
            }

            // Read input based on current scheme
            ReadInput();
            
            // Broadcast events
            BroadcastEvents();
        }

        private void OnDestroy()
        {
            InputListeners.Clear();
        }

        #endregion

        #region Input Reading

        /// <summary>
        /// Auto-detect which control scheme the player is currently using
        /// </summary>
        private void DetectControlScheme()
        {
            // Check for gamepad input first (more specific)
            if (UnityEngine.Input.GetKey(gamepadJumpButton) ||
                UnityEngine.Input.GetKey(gamepadFireButton) ||
                UnityEngine.Input.GetKey(gamepadGrabButton) ||
                UnityEngine.Input.GetKey(gamepadPauseButton))
            {
                // Gamepad detected
                if (currentScheme != ControlScheme.Gamepad)
                {
                    currentScheme = ControlScheme.Gamepad;
                    Debug.Log("InputManager: Switched to Gamepad control scheme");
                }
                return;
            }

            // Check for keyboard/mouse input
            if (UnityEngine.Input.GetKey(jumpKey) ||
                UnityEngine.Input.GetKey(releaseKey) ||
                UnityEngine.Input.GetKey(downKey) ||
                UnityEngine.Input.GetKey(grabKey) ||
                UnityEngine.Input.GetKey(pauseKey))
            {
                // Keyboard detected
                if (currentScheme != ControlScheme.Keyboard)
                {
                    currentScheme = ControlScheme.Keyboard;
                    Debug.Log("InputManager: Switched to Keyboard control scheme");
                }
            }
        }

        /// <summary>
        /// Read all input based on current control scheme
        /// </summary>
        private void ReadInput()
        {
            switch (currentScheme)
            {
                case ControlScheme.Keyboard:
                case ControlScheme.Auto:
                    ReadKeyboardInput();
                    break;

                case ControlScheme.Gamepad:
                    ReadGamepadInput();
                    break;
            }
        }

        /// <summary>
        /// Read keyboard and mouse input
        /// </summary>
        private void ReadKeyboardInput()
        {
            // Movement (WASD / Arrow keys)
            float horizontal = UnityEngine.Input.GetAxisRaw(horizontalAxis);
            float vertical = UnityEngine.Input.GetAxisRaw(verticalAxis);
            _moveInput = new Vector2(horizontal, vertical);

            // Apply deadzone
            if (_moveInput.magnitude < axisDeadzone)
            {
                _moveInput = Vector2.zero;
            }

            // Mouse
            float mouseX = UnityEngine.Input.GetAxis(mouseXAxis);
            float mouseY = UnityEngine.Input.GetAxis(mouseYAxis);
            _lookInput = new Vector2(mouseX, mouseY);

            // Buttons
            _jumpHeld = UnityEngine.Input.GetKey(jumpKey);
            _releaseHeld = UnityEngine.Input.GetKey(releaseKey);
            _downHeld = UnityEngine.Input.GetKey(downKey) || _moveInput.y < downInputThreshold;
            _grabHeld = UnityEngine.Input.GetKey(grabKey);
            _pauseHeld = UnityEngine.Input.GetKey(pauseKey);
        }

        /// <summary>
        /// Read gamepad input
        /// </summary>
        private void ReadGamepadInput()
        {
            // Movement (Left stick)
            float horizontal = UnityEngine.Input.GetAxisRaw(gamepadHorizontalAxis);
            float vertical = UnityEngine.Input.GetAxisRaw(gamepadVerticalAxis);
            _moveInput = new Vector2(horizontal, vertical);

            // Apply deadzone
            if (_moveInput.magnitude < axisDeadzone)
            {
                _moveInput = Vector2.zero;
            }

            // Look (Right stick)
            float lookX = UnityEngine.Input.GetAxis(gamepadLookXAxis);
            float lookY = UnityEngine.Input.GetAxis(gamepadLookYAxis);
            _lookInput = new Vector2(lookX, lookY);

            // Apply deadzone
            if (_lookInput.magnitude < axisDeadzone)
            {
                _lookInput = Vector2.zero;
            }

            // Buttons
            _jumpHeld = UnityEngine.Input.GetKey(gamepadJumpButton);
            _releaseHeld = UnityEngine.Input.GetKey(gamepadFireButton);
            _downHeld = _moveInput.y < downInputThreshold;
            _grabHeld = UnityEngine.Input.GetKey(gamepadGrabButton);
            _pauseHeld = UnityEngine.Input.GetKey(gamepadPauseButton);
        }

        #endregion

        #region Event Broadcasting

        // Previous frame state for detecting started/canceled events
        private Vector2 _prevMoveInput;
        private Vector2 _prevLookInput;
        private bool _prevJumpHeld;
        private bool _prevReleaseHeld;
        private bool _prevDownHeld;
        private bool _prevGrabHeld;
        private bool _prevPauseHeld;

        /// <summary>
        /// Broadcast input events based on current input state
        /// </summary>
        private void BroadcastEvents()
        {
            // Check all button inputs and broadcast their events
            CheckAndBroadcastButton(_jumpHeld, _prevJumpHeld, Input.InputEvents.Jump);
            CheckAndBroadcastButton(_releaseHeld, _prevReleaseHeld, Input.InputEvents.Release);
            CheckAndBroadcastButton(_downHeld, _prevDownHeld, Input.InputEvents.Crouch);
            CheckAndBroadcastButton(_grabHeld, _prevGrabHeld, Input.InputEvents.Grab);
            CheckAndBroadcastButton(_pauseHeld, _prevPauseHeld, Input.InputEvents.Pause);

            // Broadcast all axis events
            BroadcastAxisEvent(Input.InputAxis.MoveAxis, _moveInput);
            BroadcastAxisEvent(Input.InputAxis.CameraAxis, _lookInput);

            // Update previous state for next frame
            _prevJumpHeld = _jumpHeld;
            _prevReleaseHeld = _releaseHeld;
            _prevDownHeld = _downHeld;
            _prevGrabHeld = _grabHeld;
            _prevPauseHeld = _pauseHeld;
            _prevMoveInput = _moveInput;
            _prevLookInput = _lookInput;
        }

        /// <summary>
        /// Check a button's state change and broadcast the appropriate event
        /// </summary>
        private void CheckAndBroadcastButton(bool current, bool previous, Input.InputEvents eventType)
        {
            if (current && !previous)
            {
                // Button just pressed
                BroadcastButtonEvent(eventType, Input.InputState.Started);
            }
            else if (current)
            {
                // Button held down
                BroadcastButtonEvent(eventType, Input.InputState.Performed);
            }
            else if (previous)
            {
                // Button released
                BroadcastButtonEvent(eventType, Input.InputState.Canceled);
            }
        }

        /// <summary>
        /// Broadcast a button event to all listeners
        /// </summary>
        private void BroadcastButtonEvent(Input.InputEvents inputEvent, Input.InputState state)
        {

            if (inputEvent == Input.InputEvents.Jump)
            {
                Debug.Log("JUMP");
            }

            for (int i = InputListeners.Count - 1; i >= 0; i--)
            {
                var listener = InputListeners[i];
                if (listener != null && listener.IsInputEnabled)
                {
                    listener.OnInputEvent(inputEvent, state);
                }
            }
        }

        /// <summary>
        /// Broadcast an axis event to all listeners
        /// </summary>
        private void BroadcastAxisEvent(Input.InputAxis axis, Vector2 value)
        {
            for (int i = InputListeners.Count - 1; i >= 0; i--)
            {
                var listener = InputListeners[i];
                if (listener != null && listener.IsInputEnabled)
                {
                    listener.OnInputAxis(axis, value);
                }
            }
        }

        #endregion

        #region Public API
        /// <summary>
        /// Manually set the control scheme
        /// </summary>
        public void SetControlScheme(ControlScheme scheme)
        {
            currentScheme = scheme;
            Debug.Log("InputManager: Control scheme changed to " + scheme);
        }

        /// <summary>
        /// Get the current control scheme
        /// </summary>
        public ControlScheme GetControlScheme() => currentScheme;

        #endregion
    }
}

