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
        public GameState gameState;

        public int nowCostumerOrderCount;

        public List<DayData> dayData = new();

        public int currentDay; // current chapter?

        public float tiltMax = 8f;

        public int skipTo;

        public List<Manager> managers = new();

        private readonly Dictionary<Type, Manager> _managerMap = new();

        private int _customerFailCounter;
        private int _customerSuccessCounter;

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            DontDestroyOnLoad(gameObject); // persist across scenes
            gameState = GameState.Playing;
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
            // Optimization: Try dictionary lookup first for exact type matches
            if (_managerMap.TryGetValue(typeof(T), out var cachedManager)) return cachedManager as T;

            // Fallback to list search for inheritance/interfaces
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

                // Add to dictionary for O(1) lookup
                var type = manager.GetType();
                if (!_managerMap.ContainsKey(type)) _managerMap[type] = manager;

                UnityEngine.Debug.Log($"[GameManager] Registered: {manager.GetType().Name}");
            }
        }

        public String GetCurrentChapter()
        {
            if (gameState != GameState.Playing)
            {
                return "";
            }
            return "Chapter" + (currentDay < 10 ? "0" + currentDay : currentDay.ToString());
        }

        public void CostumerSuccess()
        {
            UnityEngine.Debug.Log("[GameManager] Customer completed orders successfully!");
            _customerSuccessCounter++;
        }

        public void CostumerFail()
        {
            UnityEngine.Debug.Log("[GameManager] Customer left unhappy!");
            _customerFailCounter++;
        }

        public void LevelStart()
        {
            gameState = GameState.Playing;
        }

        public void LevelFail()
        {
            gameState = GameState.GameOver;
            GetManager<LevelTimer>().StopTimer();
        }

        public void LevelPass()
        {
            gameState = GameState.GameOver;
            GetManager<LevelTimer>().StopTimer();
            if (gameWinUI != null) gameWinUI.SetActive(true);
            Time.timeScale = 0f;
        }

        public void LevelPause()
        {
            if (gameState == GameState.Paused)
            {
                // Resume
                gameState = GameState.Playing;
                Time.timeScale = 1f;
                if (pauseUI != null) pauseUI.SetActive(false);

                // Resume timer
                var timer = GetManager<LevelTimer>();
                if (timer != null && timer.RemainingTime > 0) timer.StartTimer(timer.RemainingTime);
            }
            else if (gameState == GameState.Playing)
            {
                // Pause
                gameState = GameState.Paused;
                Time.timeScale = 0f;
                if (pauseUI != null) pauseUI.SetActive(true);
                GetManager<LevelTimer>().StopTimer();
            }
        }

        public Dictionary<string, object> GetLevelResult()
        {
            return new Dictionary<string, object>
            {
                { "Success", _customerSuccessCounter },
                { "Fail", _customerFailCounter }
            };
        }

        public void ResetLevel()
        {
            _customerFailCounter = 0;
            _customerSuccessCounter = 0;
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