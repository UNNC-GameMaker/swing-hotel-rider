using System;
using UnityEngine;

namespace GameObjects.Generators
{
    public class FoodGenerator : Interactable
    {
        [SerializeField]
        private GameObject foodPrefab;

        public override void OnInteract()
        {
            Instantiate(foodPrefab, transform.position, Quaternion.identity);
        }
    }
}