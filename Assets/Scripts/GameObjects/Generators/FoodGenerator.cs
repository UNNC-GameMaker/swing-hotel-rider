using System;
using UnityEngine;

namespace GameObjects.Generators
{
    public class FoodGenerator : Interactable
    {
        [SerializeField]
        private GameObject foodPrefab;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }
        
        public override void OnInteract()
        {   
            UnityEngine.Debug.LogError("called");
            Instantiate(foodPrefab, transform.position, Quaternion.identity);
        }
    }
}