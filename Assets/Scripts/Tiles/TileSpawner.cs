using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles
{
    /// <summary>
    ///     Spawns GameObjects at tilemap positions based on tile types.
    ///     Automatically handles tile changes and keeps spawned objects in sync with the tilemap.
    /// </summary>
    public class TileSpawner : MonoBehaviour
    {
        #region Initialization

        /// <summary>
        ///     Builds a lookup table that maps each tile type to all its associated prefabs.
        ///     This allows multiple prefabs to be spawned for the same tile type.
        /// </summary>
        private void BuildPrefabLookupTable()
        {
            _prefabsByTileType = new Dictionary<TileBase, List<GameObject>>();

            foreach (var pair in tilePrefabPairs)
            {
                if (pair.tile == null || pair.prefab == null) continue;

                // Get or create the prefab list for this tile type
                if (!_prefabsByTileType.TryGetValue(pair.tile, out var prefabList))
                {
                    prefabList = new List<GameObject>();
                    _prefabsByTileType[pair.tile] = prefabList;
                }

                // Store an instance of the prefab (disabled) for later instantiation
                prefabList.Add(pair.prefab);
            }
        }

        #endregion

        #region Inspector Fields

        [Serializable]
        public class TilePrefabPair
        {
            [Tooltip("The tile type to match")] public TileBase tile;

            [Tooltip("The prefab to spawn at this tile's position")]
            public GameObject prefab;
        }

        [Tooltip("List of tile-to-prefab mappings. Multiple prefabs can be assigned to the same tile.")]
        public List<TilePrefabPair> tilePrefabPairs = new();

        #endregion

        #region Private Fields

        private Tilemap _tilemap;

        /// <summary>Tracks which GameObjects are spawned at each grid position</summary>
        private readonly Dictionary<Vector3Int, List<GameObject>> _spawnedObjectsByPosition = new();

        /// <summary>Maps tile types to their associated prefabs for quick lookup</summary>
        private Dictionary<TileBase, List<GameObject>> _prefabsByTileType = new();

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _tilemap = GetComponent<Tilemap>();
            BuildPrefabLookupTable();
        }

        private void OnEnable()
        {
            Tilemap.tilemapTileChanged += OnTilemapChanged;
            RespawnAllTiles();
        }

        private void OnDisable()
        {
            Tilemap.tilemapTileChanged -= OnTilemapChanged;
        }

        #endregion

        #region Tile Spawning

        /// <summary>
        ///     Clears all spawned objects and respawns them for every tile in the tilemap.
        ///     Called on initialization and when the tilemap needs to be fully refreshed.
        /// </summary>
        private void RespawnAllTiles()
        {
            ClearAllSpawnedObjects();

            // Iterate through every position in the tilemap bounds
            foreach (var gridPosition in _tilemap.cellBounds.allPositionsWithin) SpawnObjectsAtPosition(gridPosition);
        }

        /// <summary>
        ///     Callback when tiles are added, removed, or changed in the tilemap.
        ///     Updates only the affected positions.
        /// </summary>
        private void OnTilemapChanged(Tilemap tilemap, Tilemap.SyncTile[] changedTiles)
        {
            if (tilemap != _tilemap) return;

            foreach (var syncTile in changedTiles)
            {
                var gridPosition = syncTile.position;

                // Remove old spawned objects at this position
                DestroyObjectsAtPosition(gridPosition);

                // Spawn new objects based on the current tile
                SpawnObjectsAtPosition(gridPosition);
            }
        }

        /// <summary>
        ///     Spawns all prefabs associated with the tile at the given grid position.
        /// </summary>
        private void SpawnObjectsAtPosition(Vector3Int gridPosition)
        {
            var currentTile = _tilemap.GetTile(gridPosition);

            // No tile at this position, nothing to spawn
            if (currentTile == null) return;

            // Check if this tile type has any associated prefabs
            if (!_prefabsByTileType.TryGetValue(currentTile, out var prefabList) ||
                prefabList == null ||
                prefabList.Count == 0)
                return;

            // Get world position at the center of the tile
            var worldPosition = _tilemap.GetCellCenterWorld(gridPosition);

            // Instantiate all prefabs for this tile type
            var spawnedObjects = new List<GameObject>();
            foreach (var prefab in prefabList)
            {
                if (prefab == null) continue;

                var instance = Instantiate(prefab, worldPosition, Quaternion.identity, _tilemap.transform);
                spawnedObjects.Add(instance);
            }

            // Track the spawned objects by their grid position
            if (spawnedObjects.Count > 0) _spawnedObjectsByPosition[gridPosition] = spawnedObjects;
        }

        #endregion

        #region Cleanup

        /// <summary>
        ///     Destroys all objects spawned at the given grid position and removes them from tracking.
        /// </summary>
        private void DestroyObjectsAtPosition(Vector3Int gridPosition)
        {
            if (!_spawnedObjectsByPosition.TryGetValue(gridPosition, out var objects)) return;

            foreach (var obj in objects)
                if (obj != null)
                    Destroy(obj);

            _spawnedObjectsByPosition.Remove(gridPosition);
        }

        /// <summary>
        ///     Clears all spawned objects from the entire tilemap.
        /// </summary>
        private void ClearAllSpawnedObjects()
        {
            foreach (var objectList in _spawnedObjectsByPosition.Values)
            foreach (var obj in objectList)
                if (obj != null)
                    Destroy(obj);

            _spawnedObjectsByPosition.Clear();
        }

        #endregion
    }
}