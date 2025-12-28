using System.Collections;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buliding
{
    public class BaseBuilding : MonoBehaviour
    {
        public Vector2Int position;

        [SerializeField] [Tooltip("The size of the building tile")]
        private Vector2Int roomSize;

        private List<Rigidbody2D> _activatedRigidbodies;

        private BuildingGridManager _gridManager;

        private Tilemap _map;
        private TileBase _oneWayPlatTile;

        private List<Rigidbody2D> _rigidbodies;
        private TileBase _roomTile;
        private TileBase _solidTile;

        // TODO: not done yet
        private void Awake()
        {
            _map = GameObject.Find("ColliderTile").GetComponent<Tilemap>();
            _gridManager = GameManager.Instance.GetManager<BuildingGridManager>();
        }

        public virtual void ResetFurniture()
        {
        }

        public virtual void Set(bool withoutCheck = false)
        {
            _translatePosition();
            CheckBuildable(withoutCheck);

            StartCoroutine(ToPosition(Vector2Int.Scale(position, _gridManager.GridSize)));
            ResetFurniture();

            _gridManager.UpdateBuildArea(position.x, position.y, roomSize);

            // TODO: apply room collision and add furniture/others
        }

        private void _translatePosition()
        {
            position = new Vector2Int(Mathf.RoundToInt(transform.position.x / _gridManager.BuildableSize.x),
                Mathf.RoundToInt(transform.position.y / _gridManager.BuildableSize.y));
        }

        public bool CheckBuildable(bool withoutCheck)
        {
            if (withoutCheck) return true;
            return _gridManager.CheckBuildable(position.x, position.y, roomSize);
        }

        private IEnumerator ToPosition(Vector2Int pos, float duration = 1)
        {
            float timer = 0;
            while (timer < duration)
            {
                transform.position = Vector2.Lerp(transform.position, pos, timer / duration);
                timer += Time.deltaTime;
                yield return null;
            }
        }
    }
}