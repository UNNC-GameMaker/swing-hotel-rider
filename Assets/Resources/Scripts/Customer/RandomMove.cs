using UnityEngine;
using System.Collections;
using Managers;

namespace Customer
{
    /// <summary>
    /// Random movement component for customers:
    ///   • Randomly picks a target X position within a specified range
    ///   • Uses HorizontalMovement2D for precise movement to the target
    ///   • After arrival, waits for a random duration before moving again
    /// </summary>
    [RequireComponent(typeof(HorizontalMovement2D))]
    public class RandomMove : MonoBehaviour
    {
        [Header("Random Target Range (World Coordinates)")]
        [SerializeField, Tooltip("Minimum X coordinate")]
        private float minX = -5f;
        
        [SerializeField, Tooltip("Maximum X coordinate")]
        private float maxX = 5f;

        [Header("Wait Time (Seconds)")]
        [SerializeField, Tooltip("Minimum wait time at destination")]
        private float minWaitTime = 1f;
        
        [SerializeField, Tooltip("Maximum wait time at destination")]
        private float maxWaitTime = 3f;
        

        private HorizontalMovement2D _mover;
        private bool _isRandomMoving;
        private BuildingGridManager _buildingGridManager;

        /// <summary>
        /// Get the current level (floor) based on Y position
        /// </summary>
        public int Level 
        { 
            get 
            { 
                if (_buildingGridManager == null) return 0;
                return Mathf.FloorToInt(transform.position.y / _buildingGridManager.GridSize.y); 
            } 
        }

        void Awake()
        {
            _mover = GetComponent<HorizontalMovement2D>();
        }

        void Start()
        {
            _buildingGridManager = GameManager.Instance.GetManager<BuildingGridManager>();
        }

        /// <summary>
        /// Stop random movement
        /// </summary>
        public void StopRandomMove()
        {
            if (!_isRandomMoving)
            {
                return;
            }
            
            _isRandomMoving = false;
            _mover.StopMove();
        }

        /// <summary>
        /// Start random movement loop
        /// </summary>
        public void StartRandomMove()
        {
            if (_isRandomMoving)
            {
                return;
            }
            
            _isRandomMoving = true;
            StartCoroutine(MoveLoop());
        }

        /// <summary>
        /// Infinite loop: pick random target → move → wait → repeat
        /// </summary>
        private IEnumerator MoveLoop()
        {
            while (_isRandomMoving)
            {
                // 1. Randomly select target X position
                float targetX = Random.Range(minX, maxX);
                _mover.MoveToX(targetX);

                // 2. Wait until movement is complete or stopped
                yield return new WaitUntil(() => !_mover.IsMoving() || !_isRandomMoving);

                if (!_isRandomMoving)
                {
                    yield break;
                }

                // 3. Wait for random duration
                float waitDuration = Random.Range(minWaitTime, maxWaitTime);
                yield return new WaitForSeconds(waitDuration);

                if (!_isRandomMoving)
                {
                    yield break;
                }
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Draw movement range visualization in Scene view for easier debugging
        /// </summary>
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Vector3 leftBound = new Vector3(minX, transform.position.y, 0);
            Vector3 rightBound = new Vector3(maxX, transform.position.y, 0);
            
            // Draw range line
            Gizmos.DrawLine(leftBound, rightBound);
            
            // Draw boundary markers
            Gizmos.DrawSphere(leftBound, 0.1f);
            Gizmos.DrawSphere(rightBound, 0.1f);
        }
#endif
    }
}