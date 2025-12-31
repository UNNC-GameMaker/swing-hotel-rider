using System.Collections;
using UnityEngine;

namespace Customer.States
{
    public class MoveToDeskState : ICustomerState
    {
        private readonly Costumer _customer;
        private int _cachedDeskLevel;
        private Vector3 _cachedDeskPosition;
        private Coroutine _moveCoroutine;

        public MoveToDeskState(Costumer customer)
        {
            _customer = customer;
        }

        public void EnterState()
        {
            UnityEngine.Debug.Log("Enter MoveToDeskState");
            _customer.StateReference = CustomerState.MoveToDesk;

            // Validate desk exists
            if (_customer.Desk == null)
            {
                UnityEngine.Debug.LogWarning("Customer entered MoveToDeskState without a desk assigned!");
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
            // Optional: Check if desk moved significantly
            if (_customer.Desk != null)
            {
                var distanceMoved = Vector3.Distance(_cachedDeskPosition, _customer.Desk.transform.position);
                if (distanceMoved > 0.5f) // Tolerance threshold
                {
                    // Desk moved significantly, recalculate path
                    _cachedDeskPosition = _customer.Desk.transform.position;
                    _cachedDeskLevel = _customer.Desk.Level;

                    if (_moveCoroutine != null) _customer.StopCoroutine(_moveCoroutine);
                    _moveCoroutine = _customer.StartCoroutine(MoveToDesk());
                }
            }
        }

        public void ExitState()
        {
            if (_moveCoroutine != null)
            {
                _customer.StopCoroutine(_moveCoroutine);
                _moveCoroutine = null;
            }
        }

        private IEnumerator MoveToDesk()
        {
            // Check if we need to change levels
            var currentLevel = _customer.Level;
            var targetLevel = _cachedDeskLevel;

            // Move to the correct level first
            while (currentLevel != targetLevel)
            {
                var goingUp = targetLevel > currentLevel;
                yield return _customer.StartCoroutine(StartLevel(goingUp));
                currentLevel = _customer.Level;

                // Safety check
                if (_customer.Desk == null)
                {
                    _customer.ChangeState(new FindDeskState(_customer));
                    yield break;
                }
            }

            // Now move horizontally to the desk on the same level
            yield return _customer.StartCoroutine(StartMove(_cachedDeskPosition.x));

            if (IsOnDesk())
            {
                SetOnDesk();
                _customer.ChangeState(new OrderState(_customer));
            }
            else
            {
                // Failed to reach desk, find a new one
                UnityEngine.Debug.LogWarning("Customer failed to reach desk, finding new desk");
                _customer.Desk.SetFree();
                _customer.Desk = null;
                _customer.ChangeState(new FindDeskState(_customer));
            }
        }

        private bool IsOnDesk()
        {
            if (_customer.Desk)
            {
                UnityEngine.Debug.Log("Desk Distance: " + (_customer.Desk.transform.position.x - _customer.transform.position.x));
                return Mathf.Abs(_customer.Desk.transform.position.x - _customer.transform.position.x) <= 0.5f &&
                       _customer.Level == _cachedDeskLevel;
            }

            return false;
        }

        private void SetOnDesk()
        {
            _customer.Desk.SetOccupied();
            var rb = _customer.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            _customer.transform.SetParent(_customer.Desk.transform);
            _customer.transform.localPosition = Vector3.zero;
        }

        private IEnumerator StartMove(float target)
        {
            UnityEngine.Debug.Log("StartMove: " + target);
            _customer.HorizontalMovement.MoveToX(target);

            var stuckTimer = 0f;
            var lastX = _customer.transform.position.x;

            while (_customer.HorizontalMovement.IsMoving())
            {
                yield return null;

                if (Mathf.Abs(_customer.transform.position.x - lastX) < 0.001f)
                {
                    stuckTimer += Time.deltaTime;
                    if (stuckTimer > 0.5f)
                    {
                        UnityEngine.Debug.LogWarning("Customer stuck moving to " + target + ". Force stopping.");
                        _customer.HorizontalMovement.StopMove();
                        break;
                    }
                }
                else
                {
                    stuckTimer = 0f;
                    lastX = _customer.transform.position.x;
                }
            }
        }

        private IEnumerator StartLevel(bool up)
        {
            var level = up ? _customer.Level : _customer.Level - 1;
            var position = _customer.FurnitureManager.FindLevelUp(level);

            // can't get up
            if (position == -1)
            {
                _customer.Desk.SetFree();
                _customer.Desk = null;
                _customer.ChangeState(new FindDeskState(_customer));
                yield break;
            }

            var target = _customer.BuildingGridManager.GridSize.x * (position + 0.5f);
            yield return _customer.StartCoroutine(StartMove(target));
            yield return new WaitForSeconds(0.3f);

            var rb = _customer.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            float targetY = (up ? _customer.Level + 1 : _customer.Level - 1) * _customer.BuildingGridManager.GridSize.y;

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