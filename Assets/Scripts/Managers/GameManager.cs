using System;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    [DefaultExecutionOrder(-100)]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        public GameObject gameOverUI;
        public GameObject gameWinUI;
        public GameObject pauseUI;


        public int nowCostumerOrderCount;

        public List<DayData> dayData = new();

        public int currentDay;

        public float tiltMax = 8f;

        public int skipTo;

        public List<Manager> managers = new();
        private Dictionary<Type, Manager> _managerMap = new();

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: persist across scenes

            // Auto-find and initialize all managers in the scene
            InitializeManagers();
        }

        /// <summary>
        ///     Automatically finds all Manager components in children and initializes them
        /// </summary>
        private void InitializeManagers()
        {
            var foundManagers = GetComponentsInChildren<Manager>();
            foreach (var manager in foundManagers)
                if (!managers.Contains(manager))
                {
                    manager.Init();
                    Debug.Log($"[GameManager] Initialized: {manager.GetType().Name}");
                }
        }

        public T GetManager<T>() where T : Manager
        {
            // Optimization: Try dictionary lookup first for exact type matches
            if (_managerMap.TryGetValue(typeof(T), out var cachedManager))
            {
                return cachedManager as T;
            }

            // Fallback to list search for inheritance/interfaces
            var m = managers.Find(x => x is T);
            if (m == null)
            {
                Debug.LogError($"[GameManager] Manager {typeof(T).Name} not found");
                return null;
            }

            return m as T;
        }

        public void RegisterManager(Manager manager)
        {
            if (!managers.Contains(manager))
            {
                managers.Add(manager);
                
                // Add to dictionary for O(1) lookup
                var type = manager.GetType();
                if (!_managerMap.ContainsKey(type))
                {
                    _managerMap[type] = manager;
                }
                
                Debug.Log($"[GameManager] Registered: {manager.GetType().Name}");
            }
        }

        public void CostumerSuccess()
        {
            Debug.Log("[GameManager] Customer completed orders successfully!");
            // TODO: Add score, money, or other rewards here
        }

        public void CostumerFail()
        {
            Debug.Log("[GameManager] Customer left unhappy!");
            // TODO: Add penalty or failure tracking here
        }
    }


    [Serializable]
    public class DayData
    {
        public int costumerCount;
        public float costumerIntervalMax;
        public float costumerIntervalMin;

        public GameObject newBuilding;
        public int costumerOrderCount;

        public string[] newFoods;
    }
}