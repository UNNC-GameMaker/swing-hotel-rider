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
        }

        public void OnDisable()
        {
            _furnitureManager?.RemoveFurniture(this);
        }

        public void Init()
        {
            _isOccupied = false;
            FurnitureManager.Instance.AddFurniture(this);
            GetComponent<Tiltable>().Activate();
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

        #region Private Fields

        private bool _isOccupied;
        private readonly FurnitureManager _furnitureManager = FurnitureManager.Instance;

        #endregion
    }
}