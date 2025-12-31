using System.Collections;
using Managers;
using UnityEngine;

namespace Customer.States
{
    public class EatingState : ICustomerState
    {
        private readonly Costumer _customer;
        private Coroutine _eatCoroutine;

        public EatingState(Costumer customer)
        {
            _customer = customer;
        }

        public void EnterState()
        {
            UnityEngine.Debug.Log("Eating State in");
            _eatCoroutine = _customer.StartCoroutine(EatRoutine());
        }

        public void ExecuteState()
        {
        }

        public void ExitState()
        {
            if (_eatCoroutine != null) _customer.StopCoroutine(_eatCoroutine);
            _customer.Think.StopThink();
        }

        private IEnumerator EatRoutine()
        {
            _customer.Think.StartThink("Eating", false);
            yield return Wait(_customer.EatingSpeed);

            _customer.Think.StopThink();
            _customer.OrderCount++;

            if (_customer.OrderCount >= GameManager.Instance.nowCostumerOrderCount)
            {
                _customer.CostumerSuccess();
            }
            else
            {
                UnityEngine.Debug.Log("AGAIN!: " + _customer.OrderCount);
                _customer.ChangeState(new OrderState(_customer));
            }
        }

        private IEnumerator Wait(float time)
        {
            float nowTime = 0;
            while (nowTime < time)
            {
                nowTime += Time.deltaTime;
                yield return null;
            }
        }
    }
}