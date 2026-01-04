using System.Collections;
using UnityEngine;

namespace Customer.States
{
    public class LeaveState : ICustomerState
    {
        private readonly Costumer _customer;
        private bool _isMoving;

        public LeaveState(Costumer customer)
        {
            _customer = customer;
        }

        public void EnterState()
        {
            UnityEngine.Debug.Log("Enter LeaveState");
            _customer.stateReference = CustomerState.Leaving;
            _customer.Think.StopThink();
            SetOffDesk();
            _customer.StartCoroutine(LeaveRoutine());
            
            var collider = _customer.GetComponent<Collider2D>();
            if (collider != null) collider.enabled = true;
        }

        public void ExecuteState()
        {
        }

        public void ExitState()
        {
        }

        private void SetOffDesk()
        {
            if (_customer.Desk != null)
            {
                UnityEngine.Debug.Log("Leave");
                _customer.Desk.SetFree();
                _customer.Desk = null;
                var rb = _customer.GetComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Dynamic;
                _customer.transform.SetParent(null);
                rb.velocity = Vector2.zero;
            }
        }

        private IEnumerator LeaveRoutine()
        {
            while (true)
            {
                if (_isMoving)
                {
                    yield return null;
                    continue;
                }

                if (_customer.Level == 0) break;

                yield return StartLevel(false);
                yield return null;
            }

            yield return StartMove(_customer.BuildingGridManager.GridSize.x *
                                   _customer.BuildingGridManager.BuildableSize.x * 0.5f);
            Object.Destroy(_customer.gameObject);
        }

        private IEnumerator StartMove(float target)
        {
            _isMoving = true;
            _customer.HorizontalMovement.MoveToX(target);
            yield return new WaitUntil(() => !_customer.HorizontalMovement.IsMoving());
            _isMoving = false;
        }

        private IEnumerator StartLevel(bool up)
        {
            _isMoving = true;
            var level = up ? _customer.Level : _customer.Level - 1;
            var position = _customer.FurnitureManager.FindLevelUp(level);

            if (position == -1)
            {
                // Should not happen if logic is correct, but just in case
                _isMoving = false;
                yield break;
            }

            var target = _customer.BuildingGridManager.GridSize.x * (position + 0.5f);
            _customer.HorizontalMovement.MoveToX(target);

            yield return new WaitUntil(() => !_customer.HorizontalMovement.IsMoving());
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
            _isMoving = false;
        }
    }
}