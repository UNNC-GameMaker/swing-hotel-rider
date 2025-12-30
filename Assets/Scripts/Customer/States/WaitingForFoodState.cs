using UnityEngine;

namespace Customer.States
{
    public class WaitingForFoodState : ICustomerState
    {
        private Costumer _customer;
        private float _waitTime;
        private float _waitOrderTime;

        public WaitingForFoodState(Costumer customer)
        {
            _customer = customer;
        }

        public void EnterState()
        {
            Debug.Log("Waiting for Food State");
            _waitTime = 0;
            _waitOrderTime = 0;
        }

        public void ExecuteState()
        {
            if (_customer.NowOrder == null) return;

            bool isPlayer = _customer.IsPickedUpByPlayer();
            bool inRestaurant = _customer.IsInRestaurant();

            if (_customer.transform.position.y > _customer.Level * _customer.BuildingGridManager.GridSize.y + 1f || isPlayer || !inRestaurant)
            {
                _waitTime += Time.deltaTime;

                if (_waitTime > _customer.MaxWaitTime)
                {
                    _customer.CostumerFail();
                    return;
                }

                UpdateWaitThinkBubble();
            }
            else
            {
                _waitTime = 0;
                
                _waitOrderTime += Time.deltaTime;
                
                if (_waitOrderTime > _customer.MaxWaitOrderTime)
                {
                    _customer.CostumerFail();
                    return;
                }

                UpdateOrderThinkBubble();

                if (_customer.CheckFood(_customer.NowOrder))
                {
                    // Food received, go back to eating in OrderState
                    // We need to transition back to a state that handles eating.
                    // Since OrderState has the logic, we can create a new one or use a dedicated EatingState.
                    // For simplicity, let's call a method on the previous state or create a new EatingState.
                    // But wait, OrderState was exited. Let's create a new EatingState or handle it here.
                    // Actually, the previous design had Order -> CheckFood -> Eating in one loop.
                    // Let's transition to a new EatingState.
                    _customer.ChangeState(new EatingState(_customer));
                }
            }
        }

        public void ExitState()
        {
            _customer.Think.StopThink();
        }

        private void UpdateWaitThinkBubble()
        {
            if (_waitTime > _customer.MaxWaitTime / 2)
            {
                float lerp = Mathf.InverseLerp(_customer.MaxWaitTime / 2, _customer.MaxWaitTime, _waitTime);
                _customer.Think.StartThink("Waiting", true, Mathf.RoundToInt(_waitTime * 10), Color.Lerp(_customer.DefaultColor, _customer.WaitColor, lerp));
            }
            else
            {
                _customer.Think.StartThink("Waiting", false);
            }
        }

        private void UpdateOrderThinkBubble()
        {
            float colorLerp = 0;
            if (_waitOrderTime > _customer.MaxWaitOrderTime / 3)
            {
                colorLerp = (_waitOrderTime - _customer.MaxWaitOrderTime / 3) / (_customer.MaxWaitOrderTime * 2 / 3);
            }
            _customer.Think.StartThink(_customer.NowOrder, true, Mathf.RoundToInt(_waitOrderTime * 10), Color.Lerp(_customer.DefaultColor, _customer.WaitColor, colorLerp));
        }
    }
}
