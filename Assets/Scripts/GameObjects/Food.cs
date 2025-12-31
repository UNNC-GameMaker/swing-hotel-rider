using System.Collections.Generic;
using UnityEngine;

namespace GameObjects
{
    public class Food : Grabbable
    {
        [Header("Food Properties")] public string FoodType = "Default";

        public void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void OnEnable()
        {
            if (!Foods.Contains(this)) Foods.Add(this);
        }

        public void OnDisable()
        {
            if (Foods.Contains(this)) Foods.Remove(this);
            Destroy(this);
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

        # region static

        public static readonly List<Food> Foods = new();

        public static void ResetFoods()
        {
            foreach (var food in Foods)
                if (food != null)
                    Destroy(food);

            Foods.Clear();
        }

        # endregion
    }
}