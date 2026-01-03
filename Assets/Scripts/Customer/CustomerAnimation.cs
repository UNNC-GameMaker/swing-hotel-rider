using Animations;
using UnityEngine;

namespace Customer
{
    [RequireComponent(typeof(NPCAnimationController))]
    public class CustomerAnimation : MonoBehaviour
    {
        [SerializeField] private NPCAnimationController npcAnimationController;
        [SerializeField] private Costumer customer;
        
        private Rigidbody2D _rb;

        private void Awake()
        {
            if (npcAnimationController == null)
                npcAnimationController = GetComponent<NPCAnimationController>();
            
            if (customer != null)
                _rb = customer.GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (customer == null || npcAnimationController == null) return;

            UpdateAnimationState();
        }

        private void UpdateAnimationState()
        {
            bool isWalking = false;
            bool isSitting = false;
            bool isEating = false;

            switch (customer.StateReference)
            {
                case CustomerState.FindDesk:
                case CustomerState.MoveToDesk:
                case CustomerState.Leaving:
                    isWalking = true;
                    break;
                
                case CustomerState.Order:
                case CustomerState.OrderTimeout:
                case CustomerState.Waiting:
                    isSitting = true;
                    break;
                
                case CustomerState.Eating:
                    isSitting = true;
                    isEating = true;
                    break;
            }

            npcAnimationController.SetWalking(isWalking);
            npcAnimationController.SetSitting(isSitting);
            npcAnimationController.SetEating(isEating);
        }

        private void FixedUpdate()
        {
            UpdateDirection();
        }

        private void UpdateDirection()
        {
            if (_rb != null)
            {
                npcAnimationController.UpdateDirection(_rb.velocity.x);
            }
        }
    }
}