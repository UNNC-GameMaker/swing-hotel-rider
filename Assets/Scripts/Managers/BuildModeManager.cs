using System;
using Building;
using Cinemachine;
using Input;
using UnityEngine;

namespace Managers
{
    public class BuildModeManager : Manager, IInputListener
    {
        private readonly bool _buildMode = false;


        private CinemachineVirtualCamera _buildCamera;

        private UnityEngine.Camera _camera;
        private ChooseBuilding _currentBuildingChoice;
        private Vector3 _dragOffset;
        private bool _isHolding;
        private Vector2 _mouseInput;

        private void Start()
        {
            _camera = UnityEngine.Camera.main;
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

        public void OnInputEvent(InputEvents inputEvent, InputState state)
        {
            throw new NotImplementedException();
        }

        public void OnInputAxis(InputAxis axis, Vector2 value)
        {
            throw new NotImplementedException();
        }

        public int InputPriority => 0;
        public bool IsInputEnabled => true;


        public override void Init()
        {
            GameManager.Instance.RegisterManager(this);
        }


        private void OnChooseStarted()
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


        private void HandleBuildingInput()
        {
            // TODO FIX THIS
            if (_isHolding)
            {
                var mouseWorldPos = _camera.ScreenToWorldPoint(new Vector3());
                mouseWorldPos.z = _currentBuildingChoice.transform.position.z;
                var targetPosition = mouseWorldPos - _dragOffset;

                _currentBuildingChoice.MoveTo(targetPosition);
            }
        }
    }
}