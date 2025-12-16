using UnityEngine;

namespace GameObjects
{
    public class Furniture : Grabbable
    {
        public bool isOccupied;
        
        public void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void Start()
        {
            isOccupied = false;
        }

        /// <summary>
        /// Called when the food is grabbed by the player
        /// </summary>
        public override void OnGrab()
        {
            Debug.Log($"{gameObject.name} was grabbed!");
        }
        
        /// <summary>
        /// Called when the food is released by the player
        /// </summary>
        public override void OnRelease()
        {
            Debug.Log($"{gameObject.name} was released!");
        }
    }
}