using System.Collections.Generic;
using GameObjects;
using UnityEngine;

namespace Managers
{
    public class FurnitureManager : Manager
    {
        public static FurnitureManager Instance { get; private set; }
        
        private List<Furniture> _furnitures = new List<Furniture>();
        private List<Furniture> _freeFurnitures = new List<Furniture>();

        private readonly List<(int, int)> _levelUp = new List<(int, int)>();

        public List<string> orderList = new List<string>();
        
        // Public properties to access the lists
        public IReadOnlyList<Furniture> Furnitures => _furnitures;
        public IReadOnlyList<Furniture> FreeFurnitures => _freeFurnitures;

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
            Debug.Log("[FurnitureManager] Init");
            GameManager.Instance.RegisterManager(this);
        }

    public void AddFurniture(Furniture furniture)
    {
        _furnitures.Add(furniture);
        _freeFurnitures.Add(furniture);
    }

    public void AddFreeFurniture(Furniture furniture)
    {
        _freeFurnitures.Add(furniture);
    }

    public void RemoveFreeFurniture(Furniture furniture)
    {
        _freeFurnitures.Remove(furniture);
    }

    public void RemoveFurniture(Furniture furniture)
    {
        _furnitures.Remove(furniture);
        _freeFurnitures.Remove(furniture);
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