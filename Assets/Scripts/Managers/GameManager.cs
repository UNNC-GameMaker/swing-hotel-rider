using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        public GameObject gameOverUI;
        public GameObject gameWinUI;
        public GameObject pauseUI;
        

        public int nowCostumerOrderCount = 0;

        public List<DayData> dayData = new();

        public int nowDay = 0;

        public float tiltMax = 8f;

        public int skipTo = 0;
        
        public List<Manager> managers = new();
        
        public T GetManager<T>() where T : Manager
        {
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
            managers.Add(manager);
        }
        
    }


    [System.Serializable]
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