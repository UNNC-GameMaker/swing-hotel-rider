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

        public int nowDay;

        public float tiltMax = 8f;

        public int skipTo;

        public List<Manager> managers = new();

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
                    UnityEngine.Debug.Log($"[GameManager] Initialized: {manager.GetType().Name}");
                }
        }

        public T GetManager<T>() where T : Manager
        {
            var m = managers.Find(x => x is T);
            if (m == null)
            {
                UnityEngine.Debug.LogError($"[GameManager] Manager {typeof(T).Name} not found");
                return null;
            }

            return m as T;
        }

        public void RegisterManager(Manager manager)
        {
            if (!managers.Contains(manager))
            {
                managers.Add(manager);
                UnityEngine.Debug.Log($"[GameManager] Registered: {manager.GetType().Name}");
            }
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