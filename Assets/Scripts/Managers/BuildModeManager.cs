using Buliding;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Managers
{
    public class BuildModeManager : Manager
    {
        [SerializeField] private UnityEngine.InputSystem.PlayerInput playerInputComponent;

        private CinemachineVirtualCamera _buildCamera;

        private readonly bool _buildMode = false;

        private UnityEngine.Camera _camera;
        private ChooseBuilding _currentBuildingChoice;
        private Vector3 _dragOffset;
        private bool _isHolding;
        private Vector2 _mouseInput;

        private void Start()
        {
            _camera = UnityEngine.Camera.main;
            RegisterInputs();
        }

        private void Update()
        {
            if (_buildMode)
            {
                _buildCamera.Priority = 20;
                if (_isHolding) HandleBuildingInput();
            }
            else
            {
                _buildCamera.Priority = -99;
            }
        }


        public override void Init()
        {
            GameManager.Instance.RegisterManager(this);
        }

        private void RegisterInputs()
        {
            if (playerInputComponent)
            {
                playerInputComponent.actions["Choose"].started += OnChooseStarted;
                playerInputComponent.actions["Choose"].canceled += OnChooseCanceled;
                playerInputComponent.actions["Drag"].performed += ctx => _mouseInput = ctx.ReadValue<Vector2>();
            }
        }

        private void OnChooseStarted(InputAction.CallbackContext ctx)
        {
            var ray = _camera.ScreenPointToRay(_mouseInput);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                var choice = hit.collider.GetComponent<ChooseBuilding>();
                if (choice)
                {
                    _currentBuildingChoice = choice;
                    choice.NowChoose = true;
                    _dragOffset = hit.point - _buildCamera.transform.position;
                }
            }

            _isHolding = true;
        }

        private void OnChooseCanceled(InputAction.CallbackContext ctx)
        {
            _isHolding = false;
            _currentBuildingChoice.NowChoose = false;
            _currentBuildingChoice = null;
        }

        private void HandleBuildingInput()
        {
            if (_isHolding)
            {
                var mouseWorldPos = _camera.ScreenToWorldPoint(Input.mousePosition);
                mouseWorldPos.z = _currentBuildingChoice.transform.position.z;
                var targetPosition = mouseWorldPos - _dragOffset;

                _currentBuildingChoice.MoveTo(targetPosition);
            }
        }
    }
}