using System.Collections.Generic;
using GameObjects;

namespace Managers
{
    public class DeskManager : Manager
    {
        public List<string> OrderList = new();
        
        private List<Furniture> _furnitureList;
        private List<Furniture> _freeFurnitureList;
        
        public IReadOnlyList<Furniture> freeDesks => _freeFurnitureList;
        
        public override void Init()
        {
            GameManager.Instance.RegisterManager(this);
            _furnitureList = new();
            _freeFurnitureList = new();
        }

        public void AddNewFurniture(Furniture furniture)
        {
            _furnitureList.Add(furniture);
            _freeFurnitureList.Add(furniture);
        }
        
        public void RemoveFurniture(Furniture furniture)
        {
            _furnitureList.Remove(furniture);
            _freeFurnitureList.Remove(furniture);
        }

        public void AddFreeFurniture(Furniture furniture)
        {
            if (!_freeFurnitureList.Contains(furniture))
            {
                _freeFurnitureList.Add(furniture);
            }
        }

        public void RemoveFreeFurniture(Furniture furniture)
        {
            _freeFurnitureList.Remove(furniture);
        }

        public int FindLevelUp(int level)
        {
            // Delegate to FurnitureManager if it exists
            var furnitureManager = GameManager.Instance.GetManager<FurnitureManager>();
            if (furnitureManager != null)
            {
                return furnitureManager.FindLevelUp(level);
            }
            return -1;
        }
    }
}