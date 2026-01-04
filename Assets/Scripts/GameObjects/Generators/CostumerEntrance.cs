using System.Collections;
using System.Collections.Generic;
using Customer;
using Managers;
using UnityEngine;

namespace GameObjects.Generators
{
    public class CostumerEntrance : MonoBehaviour
    {
        private Vector3 _doorPosition; // initial pos = doorX, doorY - 6
        private Vector3 _initPosition;
        private FurnitureManager _furnitureManager;
        private float _timer = 0f;
        
        // customers that waiting on scaffold
        private List<GameObject> _waitingCostumers = new List<GameObject>();
        
        [SerializeField] private GameObject customerPrefab;
        [SerializeField] private int scaffoldCount = 6;
        [SerializeField] private float scaffoldSpacing = 1.5f;
        [SerializeField] private float moveSpeed = 2f;

        void Awake()
        {
            _doorPosition = gameObject.transform.position;
            _initPosition = gameObject.transform.position;
            _initPosition.y -= (scaffoldCount * scaffoldSpacing);
            _furnitureManager = GameManager.Instance.GetManager<FurnitureManager>();
        }

        void Start()
        {
            for (int i = 0; i < scaffoldCount; i++)
            {
                SpawnCustomer();

            }
        }

        void Update()
        {
            _timer += Time.deltaTime;
            if (_timer > 1f){
                TryActivateCustomer();
                _timer = 0f;
            }
        }
        

        // spawn customer 
        public bool SpawnCustomer()
        {
            if(_waitingCostumers.Count >= scaffoldCount) { return false; }
            
            var customer = Instantiate(customerPrefab, _initPosition, Quaternion.identity);
            
            // Disable Costumer component to prevent logic from starting, but keep GameObject active for visibility
            var costumerComp = customer.GetComponent<Costumer>();
            if (costumerComp != null) costumerComp.enabled = false;
            
            var rb = customer.GetComponent<Rigidbody2D>();
            if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;
            
            _waitingCostumers.Add(customer);
            
            UpdateScaffoldPositions();
            
            return true;
        }

        private void TryActivateCustomer()
        {   
            if (_furnitureManager.GetFreeDeskCount() > 0)
            {
                UnityEngine.Debug.LogError(_furnitureManager.GetFreeDeskCount());
                GetInRestaurant();
            }
        }

        private void GetInRestaurant()
        {
            if (_waitingCostumers.Count > 0)
            {
                var customer = _waitingCostumers[0];
                _waitingCostumers.RemoveAt(0);
                MoveToEntrance(customer);
                UpdateScaffoldPositions();
            }
        }

        private void UpdateScaffoldPositions()
        {
            for (int i = 0; i < _waitingCostumers.Count; i++)
            {
                // Fill from top (closest to door) down
                // i=0 is top
                float yOffset = (i + 1) * scaffoldSpacing;
                var destPosition = new Vector3(_doorPosition.x, _doorPosition.y - yOffset, _doorPosition.z);
                MoveUpOnScaffold(_waitingCostumers[i], destPosition);
            }
        }

        private void MoveToEntrance(GameObject customer)
        {
            StartCoroutine(MoveToTarget(customer, _doorPosition, true));
        }

        private void MoveUpOnScaffold(GameObject customer, Vector3 position)
        {
            StartCoroutine(MoveToTarget(customer, position, false));
        }

        private IEnumerator MoveToTarget(GameObject customer, Vector3 target, bool enableOnArrival)
        {
            while (Vector3.Distance(customer.transform.position, target) > 0.01f)
            {
                customer.transform.position = Vector3.MoveTowards(customer.transform.position, target, moveSpeed * Time.deltaTime);
                yield return null;
            }
            customer.transform.position = target;

            if (enableOnArrival)
            {
                var costumerComp = customer.GetComponent<Costumer>();
                if (costumerComp != null) costumerComp.enabled = true;

                var rb = customer.GetComponent<Rigidbody2D>();
                if (rb != null) rb.bodyType = RigidbodyType2D.Dynamic;
            }
        }
    }
}