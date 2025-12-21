using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using UnityEngine;

public class BuildingGridManager : Manager
{
    // TODO: not done yet
    private List<BaseBuilding> _buildings = new List<BaseBuilding>();

    private bool[,] _isOccupied;

    private Vector2Int _buildableSize;
    private Vector2Int _gridSize;

    public event Action OnBuilidAreaChanged;

    
    public Vector2Int BuildableSize => _buildableSize;
    public Vector2Int GridSize => _gridSize;
    public void UpdateBuildArea(int x , int y,  Vector2Int size)
    {
        for (int i = x; i < x + size.x; i++)
        {
            for (int j = y; j < y + size.y; j++)
            {
                _isOccupied[i, j] = true;
            }
        }
    }


    public void RegisterBuilding(BaseBuilding building)
    {
        _buildings.Add(building);
        
    }

    // check inbound and is empty, room should be adjacent to other room
    public bool CheckBuildable(int x, int y, Vector2Int size)
    {
        if (x < 0 || y < 0 || x + size.x > _buildableSize.x || y + size.y > _buildableSize.y)
        {
            return false;
        }
        
        for (int i = x; i < x + size.x; i++)
        {
            for (int j = y; j < y + size.y; j++)
            {
                if (_isOccupied[i, j])
                {
                    return false;
                }
            }
        }
        
        bool hasAdjacentRoom = false;
    
        // Check all cells around the perimeter of the building
        for (int i = x - 1; i <= x + size.x; i++)
        {
            for (int j = y - 1; j <= y + size.y; j++)
            {
                // Skip cells that are inside the building area
                if (i >= x && i < x + size.x && j >= y && j < y + size.y)
                    continue;
                
                if (i >= 0 && i < _buildableSize.x && j >= 0 && j < _buildableSize.y)
                {
                    if (_isOccupied[i, j])
                    {
                        hasAdjacentRoom = true;
                        break;
                    }
                }
            }
            if (hasAdjacentRoom) break;
        }

        return hasAdjacentRoom;
    }



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Init()
    {
        _isOccupied = new bool[_buildableSize.x, _buildableSize.y];
        for (int i = 0; i < _buildableSize.x; i++)
        {
            for (int j = 0; j < _buildableSize.y; j++)
            {
                _isOccupied[i, j] = false;
            }
        }

        foreach (var building in _buildings)
        {
            building.Set();
        }
    }

    public void Reset()
    {
        
    }
}
