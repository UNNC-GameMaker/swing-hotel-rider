using Customer.States;
using GameObjects;
using Managers;
using UnityEngine;

namespace Customer
{
    public class Costumer : MonoBehaviour
    {
        // Configuration
        [SerializeField] private Color defaultColor;

        [SerializeField] private Color waitColor;

        [SerializeField] private float eatingSpeed = 30;

        [SerializeField] private float orderSpeed = 10;

        [SerializeField] private float maxWaitOrderTime = 30;

        [SerializeField] private float maxWaitTime = 30;

        [SerializeField] private float upSpeed = 10f;

        [SerializeField] private float upOffset = 0.5f;
        public BuildingGridManager BuildingGridManager { get; private set; }
        public FurnitureManager FurnitureManager { get; private set; }
        public TextureManager TextureManager { get; private set; }

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
        public CustomerState stateReference = CustomerState.FindDesk;
        public Color DefaultColor => defaultColor;
        public Color WaitColor => waitColor;
        public float EatingSpeed => eatingSpeed;
        public float OrderSpeed => orderSpeed;
        public float MaxWaitOrderTime => maxWaitOrderTime;
        public float MaxWaitTime => maxWaitTime;
        public float UpSpeed => upSpeed;
        public float UpOffset => upOffset;

        // Runtime Data
        public int OrderCount { get; set; } = 0;
        public string NowOrder { get; set; }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                BuildingGridManager = GameManager.Instance.GetManager<BuildingGridManager>();
                FurnitureManager = GameManager.Instance.GetManager<FurnitureManager>();
                TextureManager = GameManager.Instance.GetManager<TextureManager>();
            }

            RandomMove = GetComponent<RandomMove>();
            HorizontalMovement = GetComponent<HorizontalMovement2D>();
            Think = GetComponent<Think>();

            if (Think == null) UnityEngine.Debug.LogError($"Think component missing on Customer {name}!");

            UnityEngine.Debug.Log("Customer: to FindDeskState");
            ChangeState(new FindDeskState(this));
        }

        private void Update()
        {
            CurrentState?.ExecuteState();
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
                var targetPos = new Vector3(HorizontalMovement.TargetX, transform.position.y, transform.position.z);
                Gizmos.DrawLine(transform.position, targetPos);
                Gizmos.DrawWireSphere(targetPos, 0.3f);
            }
        }

        public void ChangeState(ICustomerState newState)
        {
            CurrentState?.ExitState();
            CurrentState = newState;
            CurrentState?.EnterState();
        }

        public bool IsInRestaurant()
        {
            var colliders = Physics2D.OverlapCircleAll(transform.position, 0.1f);
            foreach (var _collider in colliders)
                if (_collider.gameObject.CompareTag("RestaurantArea"))
                    return true;

            return false;
        }

        public bool CheckFood(string foodType)
        {
            var colliders = Physics2D.OverlapCircleAll(transform.position, 1f);
            foreach (var _collider in colliders)
            {
                if (_collider.gameObject.CompareTag("Grabbable"))
                {
                    var food = _collider.GetComponent<Food>();
                    if (food != null && food.FoodType == foodType)
                    {
                        DestroyImmediate(_collider.gameObject);
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsPickedUpByPlayer()
        {
            var parent = transform.parent;
            while (parent != null)
            {
                if (parent.name == "Player") return true;
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
}