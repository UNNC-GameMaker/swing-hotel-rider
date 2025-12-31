using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Customer;
using Managers;
using Customer.States;
using GameObjects;
using UnityEngine.Serialization;

public class Costumer : MonoBehaviour
{
    public BuildingGridManager BuildingGridManager { get; private set; }
    public FurnitureManager FurnitureManager { get; private set; }

    public RandomMove RandomMove { get; private set; }
    public HorizontalMovement2D HorizontalMovement { get; private set; }
    public Think Think { get; private set; }

    public Furniture Desk { get; set; }

    public int Level
    {
        get
        {
            if (BuildingGridManager == null) return 0;
            return Mathf.FloorToInt(transform.position.y / BuildingGridManager.GridSize.y);
        }
    }

    public ICustomerState CurrentState { get; private set; }

    // Configuration
    [SerializeField] private Color defaultColor;
    public Color DefaultColor => defaultColor;

    [SerializeField] private Color waitColor;
    public Color WaitColor => waitColor;

    [SerializeField] private float eatingSpeed = 30;
    public float EatingSpeed => eatingSpeed;

    [SerializeField] private float orderSpeed = 10;
    public float OrderSpeed => orderSpeed;

    [SerializeField] private float maxWaitOrderTime = 30;
    public float MaxWaitOrderTime => maxWaitOrderTime;

    [SerializeField] private float maxWaitTime = 30;
    public float MaxWaitTime => maxWaitTime;

    [SerializeField] private float upSpeed = 10f;
    public float UpSpeed => upSpeed;

    [SerializeField] private float upOffset = 0.5f;
    public float UpOffset => upOffset;

    // Runtime Data
    public int OrderCount { get; set; } = 0;
    public string NowOrder { get; set; }

    void Start()
    {
        if (GameManager.Instance != null)
        {
            BuildingGridManager = GameManager.Instance.GetManager<BuildingGridManager>();
        }

        if (BuildingGridManager == null)
        {
            BuildingGridManager = FindObjectOfType<BuildingGridManager>();
            if (BuildingGridManager == null)
            {
                Debug.LogError("BuildingGridManager not found! Costumer cannot function properly.");
            }
        }

        FurnitureManager = FurnitureManager.Instance;
        
        RandomMove = GetComponent<RandomMove>();
        HorizontalMovement = GetComponent<HorizontalMovement2D>();
        Think = GetComponent<Think>();
        
        if (Think == null)
        {
            Debug.LogError($"Think component missing on Customer {name}!");
        }

        Debug.Log("Customer: to FindDeskState");
        ChangeState(new FindDeskState(this));
    }

    void Update()
    {
        CurrentState?.ExecuteState();
    }

    public void ChangeState(ICustomerState newState)
    {
        CurrentState?.ExitState();
        CurrentState = newState;
        CurrentState?.EnterState();
    }

    public bool IsInRestaurant()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.1f);
        foreach (Collider2D _collider in colliders)
        {
            if (_collider.gameObject.CompareTag("RestaurantArea"))
            {
                return true;
            }
        }
        return false;
    }

    public bool CheckFood(string foodType)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.5f);
        foreach (Collider2D _collider in colliders)
        {
            if (_collider.gameObject.CompareTag("Food") && _collider.GetComponent<Food>().FoodType == foodType)
            {
                DestroyImmediate(_collider.gameObject);
                return true;
            }
        }
        return false;
    }

    public bool IsPickedUpByPlayer()
    {
        Transform parent = transform.parent;
        while (parent != null)
        {
            if (parent.name == "Player")
            {
                return true;
            }
            parent = parent.parent;
        }
        return false;
    }

    public void CostumerFail()
    {
        GameManager.Instance.CostumerFail();
        ChangeState(new LeaveState(this));
    }

    public void CostumerSuccess()
    {
        GameManager.Instance.CostumerSuccess();
        ChangeState(new LeaveState(this));
    }

    private void OnDrawGizmos()
    {
        // Draw line to Desk if assigned
        if (Desk != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, Desk.transform.position);
            Gizmos.DrawWireSphere(Desk.transform.position, 0.5f);
        }

        // Draw line to current horizontal movement target
        if (HorizontalMovement != null && HorizontalMovement.IsMoving())
        {
            Gizmos.color = Color.yellow;
            Vector3 targetPos = new Vector3(HorizontalMovement.TargetX, transform.position.y, transform.position.z);
            Gizmos.DrawLine(transform.position, targetPos);
            Gizmos.DrawWireSphere(targetPos, 0.3f);
        }
    }
}
