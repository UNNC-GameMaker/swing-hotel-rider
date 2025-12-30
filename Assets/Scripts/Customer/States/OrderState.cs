using System.Collections;
using UnityEngine;
using Managers;

namespace Customer.States
{
    public class OrderState : ICustomerState
    {
        private Costumer _customer;
        private Coroutine _orderCoroutine;

        public OrderState(Costumer customer)
        {
            _customer = customer;
        }

        public void EnterState()
        {
            _orderCoroutine = _customer.StartCoroutine(OrderRoutine());
        }

        public void ExecuteState()
        {
            // Logic handled in coroutine
        }

        public void ExitState()
        {
            if (_orderCoroutine != null)
            {
                _customer.StopCoroutine(_orderCoroutine);
            }
            _customer.Think.StopThink();
        }

        private IEnumerator OrderRoutine()
        {
            while (true)
            {
                // Thinking about ordering
                _customer.Think.StartThink("Order", false);
                yield return Wait(_customer.OrderSpeed);
                
                SoundManager.Play("Order");
                _customer.NowOrder = _customer.DeskManager.OrderList[Random.Range(0, _customer.DeskManager.OrderList.Count)];
                
                // Waiting for food
                _customer.ChangeState(new WaitingForFoodState(_customer));
                yield break; // Exit this coroutine, state change handles the rest
            }
        }


        private IEnumerator Wait(float time)
        {
            float nowTime = 0;
            while (nowTime < time)
            {
                // Check if picked up or out of restaurant logic could be added here if needed during wait
                // For now just simple wait
                nowTime += Time.deltaTime;
                yield return null;
            }
        }
    }
}
