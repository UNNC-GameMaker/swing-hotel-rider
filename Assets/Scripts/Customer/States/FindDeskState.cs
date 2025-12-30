using System.Collections.Generic;
using GameObjects;
using Managers;
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
                    _customer.Desk.Book();
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
            // get desk in current floor first
            var availableDesks = _customer.FurnitureManager.GetLocalEmptyFurnitureList(_customer.Level);
            if (availableDesks.Count > 0)
            {
                _customer.Desk = availableDesks[Random.Range(0, availableDesks.Count)];
            }
            else
            {
                var allDesks = _customer.FurnitureManager.GetAllEmptyFurnitureList();
                if (allDesks.Count > 0)
                {
                    _customer.Desk = allDesks[Random.Range(0, allDesks.Count)];
                }
            }
        }
    }
}
