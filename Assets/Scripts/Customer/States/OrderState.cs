using System.Collections;
using Managers;
using UnityEngine;

namespace Customer.States
{
    public class OrderState : ICustomerState
    {
        private readonly Costumer _customer;
        private Coroutine _orderCoroutine;

        public OrderState(Costumer customer)
        {
            _customer = customer;
        }

        public void EnterState()
        {
            UnityEngine.Debug.Log("Entering Order State");
            _customer.stateReference = CustomerState.Order;
            _orderCoroutine = _customer.StartCoroutine(OrderRoutine());
        }

        public void ExecuteState()
        {
            // Logic handled in coroutine
        }

        public void ExitState()
        {
            if (_orderCoroutine != null) _customer.StopCoroutine(_orderCoroutine);

            if (_customer != null && _customer.Think != null) _customer.Think.StopThink();
        }

        private IEnumerator OrderRoutine()
        {
            while (true)
            {   
                // Thinking about ordering
                _customer.Think.StartThink("UI/InGame/logo-fork-knife", false);
                yield return Wait(_customer.OrderSpeed);
                
                GameManager.Instance.GetManager<SFXManager>().PlayClip("Order");
                
                var orderManager = GameManager.Instance.GetManager<OrderManager>();
                if (orderManager != null)
                {
                    _customer.NowOrder = orderManager.GetRandomOrder();
                }
                else
                {
                    UnityEngine.Debug.LogError("OrderManager not found!");
                    _customer.NowOrder = "fresh-fish"; // Fallback
                }

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