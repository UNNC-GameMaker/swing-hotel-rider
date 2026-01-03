using UnityEngine;

namespace GameObjects
{
    public abstract class Interactable : MonoBehaviour
    {
        public Rigidbody2D rb;
        public Transform Transform => rb.transform;
        public virtual void OnInteract()
        {
            UnityEngine.Debug.Log("Interact");
        }
    }
}