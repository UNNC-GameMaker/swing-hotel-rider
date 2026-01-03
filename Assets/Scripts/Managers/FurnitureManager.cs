using System.Collections.Generic;
using GameObjects;

namespace Managers
{
    public class FurnitureManager : Manager
    {
        public enum DeskStatus
        {
            Empty,
            Desired,
            Occupied
        }
        

        private Dictionary<Furniture, DeskStatus> _furnitureList;
        private List<(int, int)> _levelUp; // that house which has a ladder
        public static FurnitureManager Instance { get; private set; }

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public override void Init()
        {
            GameManager.Instance.RegisterManager(this);
            _furnitureList = new Dictionary<Furniture, DeskStatus>();
            _levelUp = new List<(int, int)>();
        }

        public void AddNewFurniture(Furniture furniture)
        {
            _furnitureList.TryAdd(furniture, DeskStatus.Empty);
        }

        // Alias for compatibility
        public void AddFurniture(Furniture furniture)
        {
            AddNewFurniture(furniture);
        }

        public void RemoveFurniture(Furniture furniture)
        {
            _furnitureList.Remove(furniture);
        }

        // Compatibility methods for Furniture.cs
        public void AddFreeFurniture(Furniture furniture)
        {
            SetFurnitureStatus(furniture, DeskStatus.Empty);
        }

        public void BookFreeFurniture(Furniture furniture)
        {
            SetFurnitureStatus(furniture, DeskStatus.Desired);
        }

        public void RemoveFreeFurniture(Furniture furniture)
        {
            // Mark as occupied
            SetFurnitureStatus(furniture, DeskStatus.Occupied);
        }

        public DeskStatus GetFurnitureStatus(Furniture furniture)
        {
            return _furnitureList.GetValueOrDefault(furniture, DeskStatus.Empty);
        }

        private void SetFurnitureStatus(Furniture furniture, DeskStatus status)
        {
            if (_furnitureList.ContainsKey(furniture)) _furnitureList[furniture] = status;
        }

        public bool TryGetFurnitureStatus(Furniture furniture, out DeskStatus status)
        {
            return _furnitureList.TryGetValue(furniture, out status);
        }

        // get empty desk that in the same floor
        public List<Furniture> GetLocalEmptyFurnitureList(int floor)
        {
            var res = new List<Furniture>();
            foreach (var furniture in _furnitureList)
                if (furniture.Value == DeskStatus.Empty && furniture.Key.Level == floor)
                    res.Add(furniture.Key);

            return res;
        }

        public List<Furniture> GetAllEmptyFurnitureList()
        {
            var res = new List<Furniture>();
            foreach (var furniture in _furnitureList)
                if (furniture.Value == DeskStatus.Empty)
                    res.Add(furniture.Key);

            return res;
        }

        // LevelUp methods
        public void AddLevelUp(int level, int position)
        {
            _levelUp.Add((level, position));
        }

        public int FindLevelUp(int level)
        {
            foreach (var item in _levelUp)
                if (item.Item1 == level)
                    return item.Item2;

            return -1;
        }

        public void RemoveLevelUp(int level, int position)
        {
            _levelUp.Remove((level, position));
        }
    }
}