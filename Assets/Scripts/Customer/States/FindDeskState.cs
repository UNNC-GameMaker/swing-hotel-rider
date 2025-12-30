using UnityEngine;

namespace Customer.States
{
    public class FindDeskState : ICustomerState
    {
        private Costumer _customer;

        public FindDeskState(Costumer customer)
        {
            _customer = customer;
        }

        public void EnterState()
        {
            GetDesk();
            if (_customer.Desk == null)
            {
                _customer.RandomMove.StartRandomMove();
            }
        }

        public void ExecuteState()
        {
            if (_customer.Desk == null)
            {
                GetDesk();
                if (_customer.Desk != null)
                {
                    _customer.RandomMove.StopRandomMove();
                    _customer.ChangeState(new MoveToDeskState(_customer));
                }
            }
            else
            {
                _customer.ChangeState(new MoveToDeskState(_customer));
            }
        }

        public void ExitState()
        {
            _customer.RandomMove.StopRandomMove();
        }

        private void GetDesk()
        {
            if (_customer.DeskManager.FreeDesks.Count <= 0)
            {
                return;
            }
            _customer.Desk = _customer.DeskManager.FreeDesks[Random.Range(0, _customer.DeskManager.FreeDesks.Count)];
            _customer.Desk.SetOccupied();
            Debug.Log("GetDesk: " + _customer.Desk.name);
        }
    }
}
