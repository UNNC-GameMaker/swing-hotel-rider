using System;
using UnityEngine;

namespace Managers
{
    public class LevelTimer : Manager
    {
        [SerializeField] private float defaultDuration = 120f;

        public float RemainingTime { get; private set; }
        public bool IsRunning { get; private set; }

        private void Update()
        {
            if (IsRunning)
            {
                RemainingTime -= Time.deltaTime;
                if (GameManager.Instance.TimerText != null)
                {
                    GameManager.Instance.TimerText.text = RemainingTime.ToString("F0");
                }
                if (RemainingTime <= 0)
                {
                    RemainingTime = 0;
                    IsRunning = false;
                    OnTimerEnd?.Invoke();
                }
            }
        }

        public event Action OnTimerEnd;

        public override void Init()
        {
            GameManager.Instance.RegisterManager(this);
        }

        public void StartTimer(float duration)
        {
            RemainingTime = duration;
            IsRunning = true;
        }

        public void StopTimer()
        {
            IsRunning = false;
        }
    }
}