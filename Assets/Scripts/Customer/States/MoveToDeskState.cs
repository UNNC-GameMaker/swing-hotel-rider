using System.Collections;
using Managers;
using UnityEngine;

namespace Customer.States
{
    public class MoveToDeskState : ICustomerState
    {
        private readonly Costumer _customer;
        private Vector3 _cachedDeskPosition;
        private int _cachedDeskLevel;
        private Coroutine _moveCoroutine;
        private Coroutine _subCoroutine;

        public MoveToDeskState(Costumer customer)
        {
            _customer = customer;
        }

        public void EnterState()
        {
            Debug.Log("Enter MoveToDeskState");
            
            // Validate desk exists
            if (_customer.Desk == null)
            {
                Debug.LogWarning("Customer entered MoveToDeskState without a desk assigned!");
                _customer.ChangeState(new FindDeskState(_customer));
                return;
            }
            
            // Cache the desk position once when entering state
            _cachedDeskPosition = _customer.Desk.transform.position;
            _cachedDeskLevel = _customer.Desk.Level;
            
            // Start the movement coroutine
            _moveCoroutine = _customer.StartCoroutine(MoveToDesk());
        }

        public void ExecuteState()
        {
            // Check if desk moved significantly
            if (_customer.Desk != null)
            {
                float distanceMoved = Vector3.Distance(_cachedDeskPosition, _customer.Desk.transform.position);
                if (distanceMoved > 0.5f) // Tolerance threshold
                {
                    // Desk moved significantly, recalculate path
                    _cachedDeskPosition = _customer.Desk.transform.position;
                    _cachedDeskLevel = _customer.Desk.Level;
                    
                    StopCoroutines();
                    ResetRigidbody();
                    _customer.HorizontalMovement.StopMove();
                    
                    _moveCoroutine = _customer.StartCoroutine(MoveToDesk());
                }
            }
        }

        public void ExitState()
        {
            StopCoroutines();
            ResetRigidbody();
        }

        private void StopCoroutines()
        {
            if (_subCoroutine != null)
            {
                _customer.StopCoroutine(_subCoroutine);
                _subCoroutine = null;
            }
            if (_moveCoroutine != null)
            {
                _customer.StopCoroutine(_moveCoroutine);
                _moveCoroutine = null;
            }
        }

        private void ResetRigidbody()
        {
            var rb = _customer.GetComponent<Rigidbody2D>();
            if (rb != null && rb.bodyType == RigidbodyType2D.Kinematic)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.velocity = Vector2.zero;
            }
        }

        private IEnumerator MoveToDesk()
        {
            // Check if we need to change levels
            int currentLevel = _customer.Level;
            int targetLevel = _cachedDeskLevel;
            int maxLevelChanges = 20;
            
            // Move to the correct level first
            while (currentLevel != targetLevel && maxLevelChanges > 0)
            {
                maxLevelChanges--;
                bool goingUp = targetLevel > currentLevel;
                _subCoroutine = _customer.StartCoroutine(StartLevel(goingUp));
                yield return _subCoroutine;
                currentLevel = _customer.Level;
                
                // Safety check
                if (_customer.Desk == null)
                {
                    _customer.ChangeState(new FindDeskState(_customer));
                    yield break;
                }
            }
            
            // Now move horizontally to the desk on the same level
            _subCoroutine = _customer.StartCoroutine(StartMove(_cachedDeskPosition.x));
            yield return _subCoroutine;
            
            // Check if we arrived at the desk
            if (IsOnDesk())
            {
                SetOnDesk();
                _customer.ChangeState(new OrderState(_customer));
            }
            else
            {
                // Failed to reach desk, find a new one
                Debug.LogWarning("Customer failed to reach desk, finding new desk");
                _customer.Desk.SetFree();
                _customer.Desk = null;
                _customer.ChangeState(new FindDeskState(_customer));
            }
        }

        private bool IsOnDesk()
        {
            if (_customer.Desk == null) return false;
            return Mathf.Abs(_customer.Desk.transform.position.x - _customer.transform.position.x) <= 0.5f && 
                   _customer.Level == _cachedDeskLevel;
        }

        private void SetOnDesk()
        {
            _customer.Desk.SetOccupied();
            Rigidbody2D rb = _customer.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            _customer.transform.SetParent(_customer.Desk.transform);
            _customer.transform.localPosition = Vector3.zero;
        }

        private IEnumerator StartMove(float target)
        {
            Debug.Log("StartMove: " + target);
            _customer.HorizontalMovement.MoveToX(target);
            yield return new WaitUntil(() => !_customer.HorizontalMovement.IsMoving());
        }

        private IEnumerator StartLevel(bool up)
        {
            int level = up ? _customer.Level : _customer.Level - 1;
            int position = _customer.FurnitureManager.FindLevelUp(level);

            // can't get up
            if (position == -1)
            {
                _customer.Desk.SetFree();
                _customer.Desk = null;
                _customer.ChangeState(new FindDeskState(_customer));
                yield break;
            }

            float target = _customer.BuildingGridManager.GridSize.x * (position + 0.5f);
            _customer.HorizontalMovement.MoveToX(target);

            yield return new WaitUntil(() => !_customer.HorizontalMovement.IsMoving());
            yield return new WaitForSeconds(0.3f);

            Rigidbody2D rb = _customer.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            float targetY = (up ? (_customer.Level + 1) : (_customer.Level - 1)) * _customer.BuildingGridManager.GridSize.y;

            while (true)
            {
                if (up)
                {
                    _customer.transform.position += new Vector3(0, _customer.UpSpeed * Time.deltaTime, 0);
                    if (_customer.transform.position.y > targetY + _customer.UpOffset) break;
                }
                else
                {
                    _customer.transform.position -= new Vector3(0, _customer.UpSpeed * Time.deltaTime, 0);
                    if (_customer.transform.position.y < targetY + _customer.UpOffset) break;
                }
                yield return null;
            }

            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.velocity = Vector2.up * _customer.UpSpeed;

            yield return new WaitForSeconds(0.3f);
        }
    }
}
