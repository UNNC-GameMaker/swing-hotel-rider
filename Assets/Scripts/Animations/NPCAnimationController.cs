using System.Collections;
using UnityEngine;

namespace Animations
{
    public class NPCAnimationController : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private readonly int _idleTrigger = Animator.StringToHash("IDLE_2");
        private readonly int _isWalking = Animator.StringToHash("walking");
        private readonly int _isSitting = Animator.StringToHash("sitting");
        private readonly int _isEating = Animator.StringToHash("eating");
        
        private float _lastY = float.NegativeInfinity;

        private void Start()
        {
            StartCoroutine(RandomIdle());
        }

        public void SetWalking(bool isWalking)
        {
            animator.SetBool(_isWalking, isWalking);
        }

        public void SetSitting(bool isSitting)
        {
            animator.SetBool(_isSitting, isSitting);
        }

        public void SetEating(bool isEating)
        {
            animator.SetBool(_isEating, isEating);
        }

        public void UpdateDirection(float velocityX)
        {
            if (Mathf.Abs(velocityX) > 0.1f)
            {
                spriteRenderer.flipX = velocityX < 0;
            }
        }

        private IEnumerator RandomIdle()
        {
            while (true)
            {
                var randomTime = Random.Range(3f, 10f);
                yield return new WaitForSeconds(randomTime);
                animator.SetTrigger(_idleTrigger);
            }
        }
    }
}