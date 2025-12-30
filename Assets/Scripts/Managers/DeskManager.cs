using System.Collections.Generic;
using GameObjects;

namespace Managers
{
    public class DeskManager : Manager
    {
        public List<string> OrderList = new();
        
        private List<Furniture> _furnitureList;
        private List<Furniture> _freeFurnitureList;
        private List<(int,int)> _levelUp; // what the fuck is a levelUp
        
        public IReadOnlyList<Furniture> FreeDesks => _freeFurnitureList;

        
        public override void Init()
        {
            GameManager.Instance.RegisterManager(this);
            _furnitureList = new();
            _freeFurnitureList = new();
            _levelUp = new();
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

        public void AddLevelUp(int level,int position)
        {
            _levelUp.Add((level,position));
        }

        public int FindLevelUp(int level)
        {
            foreach (var item in _levelUp)
            {
                if (item.Item1 == level)
                {
                    return item.Item2;
                }
            }
            return -1;
        }

        public void RemoveLevelUp(int level,int position)
        {
            _levelUp.Remove((level,position));
        }
        
        
    }
}