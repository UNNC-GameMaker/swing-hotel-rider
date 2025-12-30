using GravityTilt;
using Managers;
using UnityEngine;


namespace GameObjects
{
    public class Furniture : Grabbable
    {
        
        public void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            _furnitureManager = GameManager.Instance.GetManager<FurnitureManager>();
        }

        public void OnDisable()
        {
            _furnitureManager?.RemoveFurniture(this);
        }

        public void Init()
        {
            _isOccupied = false;
            if (_furnitureManager == null) _furnitureManager = FurnitureManager.Instance;
            _furnitureManager?.AddFurniture(this);
            GetComponent<Tiltable>().Activate();
        }

        public void Book()
        {
            _furnitureManager.BookFreeFurniture(this);
        }

        public void SetOccupied()
        {
            _isOccupied = true;
            _furnitureManager.RemoveFreeFurniture(this);
        }

        public void SetFree()
        {
            _isOccupied = false;
            _furnitureManager.AddFreeFurniture(this);
        }

        /// <summary>
        ///     Called when the food is grabbed by the player
        /// </summary>
        public override void OnGrab()
        {
            UnityEngine.Debug.Log($"{gameObject.name} was grabbed!");
        }

        /// <summary>
        ///     Called when the food is released by the player
        /// </summary>
        public override void OnRelease()
        {
            UnityEngine.Debug.Log($"{gameObject.name} was released!");
        }

        public int Level
        {   get{
            return Mathf.FloorToInt(transform.position.y/GameManager.Instance.GetManager<BuildingGridManager>().GridSize.y);
            }
        }

        #region Private Fields

        private bool _isOccupied;
        private FurnitureManager _furnitureManager;

        #endregion
    }
}