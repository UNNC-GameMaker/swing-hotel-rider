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
    public DeskManager DeskManager { get; private set; }
    public BuildingGridManager BuildingGridManager { get; private set; }

    public RandomMove RandomMove { get; private set; }
    public HorizontalMovement2D HorizontalMovement { get; private set; }
    public Think Think { get; private set; }

    public Furniture Desk { get; set; }

    public int Level { get { return Mathf.FloorToInt(transform.position.y / BuildingGridManager.GridSize.y); } }

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
        DeskManager = GameManager.Instance.GetManager<DeskManager>();
        BuildingGridManager = GameManager.Instance.GetManager<BuildingGridManager>();
        
        RandomMove = GetComponent<RandomMove>();
        HorizontalMovement = GetComponent<HorizontalMovement2D>();
        Think = GetComponent<Think>();

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
}
