using Cinemachine;
using Managers;
using UnityEngine;

public class BuildModeManager : Manager
{

    private CinemachineVirtualCamera _buildCamera;

    private readonly bool _buildMode = false;

    private Camera _camera;
    private ChooseBuilding _currentBuildingChoice;
    private Vector3 _dragOffset;
    private bool _isHolding;
    private Vector2 _mouseInput;

    private void Start()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        // 旧输入系统：每帧读取鼠标位置
        _mouseInput = Input.mousePosition;

        if (_buildMode)
        {
            _buildCamera.Priority = 20;

            if (Input.GetMouseButtonDown(0))
                OnChooseStarted();

            if (Input.GetMouseButtonUp(0))
                OnChooseCanceled();

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

    private void OnChooseCanceled()
    {
        _isHolding = false;
        if (_currentBuildingChoice != null)
        {
            _currentBuildingChoice.NowChoose = false;
            _currentBuildingChoice = null;
        }
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