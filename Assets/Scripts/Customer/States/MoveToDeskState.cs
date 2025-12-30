using System.Collections;
using UnityEngine;

namespace Customer.States
{
    public class MoveToDeskState : ICustomerState
    {
        private Costumer _customer;
        private bool _isMoving;

        public MoveToDeskState(Costumer customer)
        {
            _customer = customer;
        }

        public void EnterState()
        {
            _isMoving = false;
        }

        public void ExecuteState()
        {
            if (_isMoving) return;

            if (IsOnDesk())
            {
                SetOnDesk();
                _customer.ChangeState(new OrderState(_customer));
                return;
            }

            if (_customer.Level == _customer.Desk.Level)
            {
                _customer.StartCoroutine(StartMove(_customer.Desk.transform.position.x));
            }
            else
            {
                if (_customer.Level > _customer.Desk.Level)
                {
                    _customer.StartCoroutine(StartLevel(false));
                }
                else
                {
                    _customer.StartCoroutine(StartLevel(true));
                }
            }
        }

        public void ExitState()
        {
        }

        private bool IsOnDesk()
        {
            if (_customer.Desk == null) return false;
            return Mathf.Abs(_customer.Desk.transform.position.x - _customer.transform.position.x) <= 0.5f && _customer.Level == _customer.Desk.Level;
        }

        private void SetOnDesk()
        {
            Rigidbody2D rb = _customer.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            _customer.transform.SetParent(_customer.Desk.transform);
            _customer.transform.localPosition = Vector3.zero;
        }

        private IEnumerator StartMove(float target)
        {
            Debug.Log("StartMove: " + target);
            _isMoving = true;
            _customer.HorizontalMovement.MoveToX(target);
            yield return new WaitUntil(() => !_customer.HorizontalMovement.IsMoving());
            _isMoving = false;
        }

        private IEnumerator StartLevel(bool up)
        {
            _isMoving = true;
            int level = up ? _customer.Level : _customer.Level - 1;
            int position = _customer.DeskManager.FindLevelUp(level);

            if (position == -1)
            {
                Debug.Log("找不到楼梯!:" + level);
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
            _isMoving = false;
        }
    }
}
