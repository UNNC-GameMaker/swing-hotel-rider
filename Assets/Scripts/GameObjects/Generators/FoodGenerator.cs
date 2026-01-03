using System;
using UnityEngine;

namespace GameObjects.Generators
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class FoodGenerator : Interactable
    {
        [SerializeField]
        private GameObject foodPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private BoxCollider2D interactionBox;

        protected override void Awake()
        {
            base.Awake();
            if (interactionBox == null) interactionBox = GetComponent<BoxCollider2D>();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (interactionBox == null) interactionBox = GetComponent<BoxCollider2D>();
        }

        public override void OnInteract()
        {   
            UnityEngine.Debug.LogError("called");
            var spawnTransform = spawnPoint != null ? spawnPoint : transform;
            Instantiate(foodPrefab, spawnTransform.position, Quaternion.identity);
        }
    }
}
