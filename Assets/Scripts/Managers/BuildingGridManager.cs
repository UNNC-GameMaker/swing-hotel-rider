using System;
using System.Collections.Generic;
using GameObjects;
using Managers;
using UnityEngine;

public class BuildingGridManager : Manager
{
    private Vector2Int _buildableSize;

    // TODO: not done yet
    private readonly List<BaseBuilding> _buildings = new();

    private bool[,] _isOccupied;


    public Vector2Int BuildableSize => _buildableSize;
    public Vector2Int GridSize { get; }

    public void Reset()
    {
        Food.ResetFoods();

        foreach (var building in _buildings)
        {
            building.ResetFurniture();
        }
    }

    public event Action OnBuilidAreaChanged;

    public void UpdateBuildArea(int x, int y, Vector2Int size)
    {
        for (var i = x; i < x + size.x; i++)
        for (var j = y; j < y + size.y; j++)
            _isOccupied[i, j] = true;
    }


    public void RegisterBuilding(BaseBuilding building)
    {
        _buildings.Add(building);
    }

    // check inbound and is empty, room should be adjacent to other room
    public bool CheckBuildable(int x, int y, Vector2Int size)
    {
        if (x < 0 || y < 0 || x + size.x > _buildableSize.x || y + size.y > _buildableSize.y) return false;

        for (var i = x; i < x + size.x; i++)
        for (var j = y; j < y + size.y; j++)
            if (_isOccupied[i, j])
                return false;

        var hasAdjacentRoom = false;

        // Check all cells around the perimeter of the building
        for (var i = x - 1; i <= x + size.x; i++)
        {
            for (var j = y - 1; j <= y + size.y; j++)
            {
                // Skip cells that are inside the building area
                if (i >= x && i < x + size.x && j >= y && j < y + size.y)
                    continue;

                if (i >= 0 && i < _buildableSize.x && j >= 0 && j < _buildableSize.y)
                    if (_isOccupied[i, j])
                    {
                        hasAdjacentRoom = true;
                        break;
                    }
            }

            if (hasAdjacentRoom) break;
        }

        return hasAdjacentRoom;
    }

    public override void Init()
    {
        _isOccupied = new bool[_buildableSize.x, _buildableSize.y];
        for (var i = 0; i < _buildableSize.x; i++)
        for (var j = 0; j < _buildableSize.y; j++)
            _isOccupied[i, j] = false;

        foreach (var building in _buildings) building.Set();
    }
}