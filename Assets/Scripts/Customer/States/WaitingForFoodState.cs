using Managers;
using UnityEngine;

namespace Customer.States
{
    public class WaitingForFoodState : ICustomerState
    {
        private readonly Costumer _customer;
        private float _waitTime;
        private float _waitOrderTime;

        public WaitingForFoodState(Costumer customer)
        {
            _customer = customer;
        }

        public void EnterState()
        {
            UnityEngine.Debug.Log("Waiting for Food State");
            _customer.stateReference = CustomerState.Waiting;
            _waitTime = 0;
            _waitOrderTime = 0;
        }

        public void ExecuteState()
        {
            if (_customer.NowOrder == null)
            {
                UnityEngine.Debug.LogError("now order is null!");
                return;
            }
            
            _waitTime = 0;

            _waitOrderTime += Time.deltaTime;
            if (_waitOrderTime > _customer.MaxWaitOrderTime)
            {
                _customer.CostumerFail();
                return;
            }

            UpdateOrderThinkBubble();

            UnityEngine.Debug.Log($"[WaitingForFoodState] Checking food: {_customer.NowOrder}");
            if (_customer.CheckFood(_customer.NowOrder))
            {
                UnityEngine.Debug.Log("[WaitingForFoodState] Food check passed. Transitioning to EatingState.");
                _customer.ChangeState(new EatingState(_customer));
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
                var lerp = Mathf.InverseLerp(_customer.MaxWaitTime / 2, _customer.MaxWaitTime, _waitTime);
                _customer.Think.StartThink(GameManager.Instance.GetCurrentChapter() + "/Food/" + _customer.NowOrder, true, Mathf.RoundToInt(_waitTime * 10),
                    Color.Lerp(_customer.DefaultColor, _customer.WaitColor, lerp));
            }
            else
            {
                _customer.Think.StartThink(GameManager.Instance.GetCurrentChapter() + "/Food/" + _customer.NowOrder, false);
            }
        }

        private void UpdateOrderThinkBubble()
        {
            float colorLerp = 0;
            if (_waitOrderTime > _customer.MaxWaitOrderTime / 3)
                colorLerp = (_waitOrderTime - _customer.MaxWaitOrderTime / 3) / (_customer.MaxWaitOrderTime * 2 / 3);
            
            string spritePath = GameManager.Instance.GetCurrentChapter() + "/Food/" + _customer.NowOrder;
            _customer.Think.StartThink(spritePath, true, Mathf.RoundToInt(_waitOrderTime * 10),
                Color.Lerp(_customer.DefaultColor, _customer.WaitColor, colorLerp));
        }
    }
}