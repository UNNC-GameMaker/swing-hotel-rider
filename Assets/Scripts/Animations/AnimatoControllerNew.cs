using System.Collections;
using UnityEngine;

namespace Animations
{
    public class AnimatoControllerNew : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
    
        private readonly int _idleTrigger = Animator.StringToHash("IDLE_2");
        private readonly int _descending = Animator.StringToHash("IsDown");
        private float _lastY = float.NegativeInfinity;
    
        void Start()
        {
            StartCoroutine(RandomIdle());
        }

        IEnumerator RandomIdle()
        {
            while (true)
            {
                float randomTime = Random.Range(3f, 10f);
                yield return new WaitForSeconds(randomTime);
                animator.SetTrigger(_idleTrigger);
            }
        }

        void FixedUpdate()
        {
            if(transform.position.y+0.001<_lastY)
            {
                animator.SetBool(_descending, true);
            }
            else
            {
                animator.SetBool(_descending, false);
            }

            _lastY = transform.position.y;
        }
    }
}
