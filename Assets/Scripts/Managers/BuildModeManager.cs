using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Managers;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildModeManager : Manager
{
    [SerializeField]
    private UnityEngine.InputSystem.PlayerInput playerInputComponent;
    
    private bool _buildMode = false;
    private bool _isHolding;
    
    private CinemachineVirtualCamera _buildCamera;
    
    private Camera _camera;
    private Vector3 _dragOffset;
    private Vector2 _mouseInput;
    private ChooseBuilding _currentBuildingChoice;
    
    
    
    public override void Init()
    {
        
        GameManager.Instance.RegisterManager(this);
    }

    void Start()
    {
        _camera = Camera.main;
        RegisterInputs();
    }

    void Update()
    {
        if (_buildMode)
        {
            _buildCamera.Priority = 20;
            if (_isHolding)
            {
                HandleBuildingInput();
            }
        }
        else
        {
            _buildCamera.Priority = -99;
            
        }
    }

    void RegisterInputs()
    {
        if (playerInputComponent)
        {
            playerInputComponent.actions["Choose"].started += OnChooseStarted;
            playerInputComponent.actions["Choose"].canceled += OnChooseCanceled;
            playerInputComponent.actions["Drag"].performed += ctx => _mouseInput = ctx.ReadValue<Vector2>();
        }
    }

    void OnChooseStarted(InputAction.CallbackContext ctx)
    {
        Ray ray = _camera.ScreenPointToRay(_mouseInput);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            ChooseBuilding choice = hit.collider.GetComponent<ChooseBuilding>();
            if (choice)
            {
                _currentBuildingChoice =  choice;
                choice.NowChoose = true;
                _dragOffset = hit.point - _buildCamera.transform.position;
            }
        }
        _isHolding = true;  
    }

    void OnChooseCanceled(InputAction.CallbackContext ctx)
    {
        _isHolding = false;
        _currentBuildingChoice.NowChoose = false;
        _currentBuildingChoice = null;
    }

    void HandleBuildingInput()
    {
        if (_isHolding)
        {
            Vector3 mouseWorldPos = _camera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = _currentBuildingChoice.transform.position.z;
            Vector3 targetPosition = mouseWorldPos - _dragOffset;
            
            _currentBuildingChoice.MoveTo(targetPosition);
        }
    }
}
