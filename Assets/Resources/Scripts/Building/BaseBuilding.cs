using System.Collections;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Building
{
    public class BaseBuilding : MonoBehaviour
    {
        [Header("Building Configuration")]
        public Vector2Int position;

        [SerializeField] [Tooltip("The size of the building in grid cells")]
        private Vector2Int roomSize;

        [Header("Tile References")]
        [SerializeField] [Tooltip("Tile used for room interior")]
        private TileBase roomTile;
        
        [SerializeField] [Tooltip("Tile used for solid walls")]
        private TileBase solidTile;
        
        [SerializeField] [Tooltip("Tile used for one-way platforms (bottom entrance)")]
        private TileBase oneWayPlatformTile;

        [Header("References")]
        [SerializeField] [Tooltip("The tilemap to place building tiles on")]
        private Tilemap tilemap;

        private List<Rigidbody2D> _activatedRigidbodies = new();
        private List<Rigidbody2D> _rigidbodies = new();
        private BuildingGridManager _gridManager;


        private void Awake()
        {
            // If tilemap not assigned, try to find it
            if (tilemap == null)
            {
                GameObject colliderTileObj = GameObject.Find("ColliderTile");
                if (colliderTileObj != null)
                {
                    tilemap = colliderTileObj.GetComponent<Tilemap>();
                }
            }
            
            _gridManager = GameManager.Instance.GetManager<BuildingGridManager>();
            
            // Initialize rigidbody lists
            _activatedRigidbodies = new List<Rigidbody2D>();
            _rigidbodies = new List<Rigidbody2D>(GetComponentsInChildren<Rigidbody2D>());
        }

        public virtual void ResetFurniture()
        {
            foreach (Rigidbody2D rb in _activatedRigidbodies.ToArray())
            {
                Destroy(rb.gameObject);
                _activatedRigidbodies.Remove(rb);
            }

            foreach (var rb in _rigidbodies.ToArray())
            {
                rb.gameObject.SetActive(false);
                GameObject baseFurniture = Instantiate(rb.gameObject, transform);
                Rigidbody2D rbNew = baseFurniture.GetComponent<Rigidbody2D>();
                rbNew.bodyType = RigidbodyType2D.Dynamic;
                _activatedRigidbodies.Add(rbNew);
                baseFurniture.SetActive(true);
            }
        }

        public virtual void Set(bool withoutCheck = false)
        {
            TranslatePosition();
            
            if (!CheckBuildable(withoutCheck))
            {
                UnityEngine.Debug.LogWarning($"Building at {position} is not buildable!");
                return;
            }

            // Update grid manager
            _gridManager.UpdateBuildArea(position.x, position.y, roomSize);
            
            // Place tiles on the tilemap
            PlaceTiles();
            
            // Move building to position and reset furniture
            StartCoroutine(MoveToPosition(Vector2Int.Scale(position, _gridManager.GridSize)));
            ResetFurniture();
        }

        private void TranslatePosition()
        {
            position = new Vector2Int(
                Mathf.RoundToInt(transform.position.x / _gridManager.GridSize.x),
                Mathf.RoundToInt(transform.position.y / _gridManager.GridSize.y)
            );
        }

        public bool CheckBuildable(bool withoutCheck)
        {
            if (withoutCheck) return true;
            return _gridManager.CheckBuildable(position.x, position.y, roomSize);
        }

        /// <summary>
        /// Places all tiles for the building: walls, floors, and one-way platforms
        /// </summary>
        private void PlaceTiles()
        {
            if (tilemap == null)
            {
                UnityEngine.Debug.LogError("Tilemap reference is null! Cannot place tiles.");
                return;
            }

            Vector2Int tilePosition = position * _gridManager.GridSize;

            // First pass: Place walls around the perimeter
            PlaceWalls(tilePosition);

            // Second pass: Place floor tiles inside the room
            PlaceFloor(tilePosition);

            // Third pass: Place one-way platforms at bottom entrance
            PlaceBottomEntrance(tilePosition);
        }

        /// <summary>
        /// Places solid wall tiles around the building perimeter
        /// </summary>
        private void PlaceWalls(Vector2Int tilePosition)
        {
            Vector2Int gridSize = _gridManager.GridSize;

            // Left wall (x = -1, all y from -1 to size.y)
            for (int j = -1; j <= gridSize.y; j++)
            {
                Vector3Int pos = new Vector3Int(tilePosition.x - 1, tilePosition.y + j, 0);
                if (tilemap.GetTile(pos) == null)
                {
                    tilemap.SetTile(pos, solidTile);
                }
            }

            // Right wall (x = size.x, all y from -1 to size.y)
            for (int j = -1; j <= gridSize.y; j++)
            {
                Vector3Int pos = new Vector3Int(tilePosition.x + gridSize.x, tilePosition.y + j, 0);
                if (tilemap.GetTile(pos) == null)
                {
                    tilemap.SetTile(pos, solidTile);
                }
            }

            // Bottom wall (y = -1, all x from -1 to size.x)
            for (int i = -1; i <= gridSize.x; i++)
            {
                Vector3Int pos = new Vector3Int(tilePosition.x + i, tilePosition.y - 1, 0);
                if (tilemap.GetTile(pos) == null)
                {
                    tilemap.SetTile(pos, solidTile);
                }
            }

            // Top wall (y = size.y, all x from -1 to size.x)
            for (int i = -1; i <= gridSize.x; i++)
            {
                Vector3Int pos = new Vector3Int(tilePosition.x + i, tilePosition.y + gridSize.y, 0);
                if (tilemap.GetTile(pos) == null)
                {
                    tilemap.SetTile(pos, solidTile);
                }
            }
        }

        /// <summary>
        /// Places room tiles inside the building
        /// </summary>
        private void PlaceFloor(Vector2Int tilePosition)
        {
            Vector2Int gridSize = _gridManager.GridSize;

            for (int i = 0; i < gridSize.x; i++)
            {
                for (int j = 0; j < gridSize.y; j++)
                {
                    Vector3Int pos = new Vector3Int(tilePosition.x + i, tilePosition.y + j, 0);
                    tilemap.SetTile(pos, roomTile);
                }
            }
        }

        /// <summary>
        /// Replaces bottom wall tiles with one-way platforms where room tiles exist below
        /// </summary>
        private void PlaceBottomEntrance(Vector2Int tilePosition)
        {
            Vector2Int gridSize = _gridManager.GridSize;

            for (int i = 0; i < gridSize.x; i++)
            {
                Vector3Int pos = new Vector3Int(tilePosition.x + i, tilePosition.y - 1, 0);
                
                // If there's a room tile here (from the bottom wall pass), replace it with one-way platform
                if (tilemap.GetTile(pos) == roomTile)
                {
                    tilemap.SetTile(pos, oneWayPlatformTile);
                }
            }
        }

        private IEnumerator MoveToPosition(Vector2Int targetPosition, float duration = 1f)
        {
            float time = 0;
            Vector2 startPosition = transform.position;
            Vector2 targetPos = targetPosition;
            
            while (time < duration)
            {
                time += Time.deltaTime;
                transform.position = Vector2.Lerp(startPosition, targetPos, time / duration);
                yield return null;
            }
            
            // Ensure final position is exact
            transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
        }
    }
}