using UnityEngine;

namespace GameObjects
{
    [RequireComponent(typeof(Collider2D))]
    public abstract class Interactable : MonoBehaviour
    {
        [SerializeField] private Collider2D interactionCollider;
        [SerializeField] private Transform interactionPoint;
        public Collider2D InteractionCollider => interactionCollider;
        public Transform Transform => transform;
        public Transform InteractionPoint => interactionPoint != null ? interactionPoint : transform;

        protected virtual void Awake()
        {
            if (interactionCollider == null) interactionCollider = GetComponent<Collider2D>();
            if (interactionPoint == null) interactionPoint = transform;
        }

        protected virtual void OnValidate()
        {
            if (interactionCollider == null) interactionCollider = GetComponent<Collider2D>();
            if (interactionPoint == null) interactionPoint = transform;
        }

        public virtual void OnInteract()
        {
            UnityEngine.Debug.Log("Interact");
        }
    }
}
