using System;
using System.Collections.Generic;
using GameObjects.Generators;
using UnityEngine;

namespace Managers
{
    public class CustomerManager : Manager
    {
        [Serializable]
        public class SpawnRule
        {
            public int time;
            public int randomSec;
            public int count;
        }

        [Serializable]
        public class LevelSpawnData
        {
            public int timerStart;
            public List<SpawnRule> npc1;
        }

        private LevelSpawnData _levelData;
        private readonly List<float> _spawnTimes = new List<float>();
        private CostumerEntrance _costumerEntrance;
        private LevelTimer _levelTimer;

        public override void Init()
        {
            GameManager.Instance.RegisterManager(this);
        }

        private void Start()
        {
            _costumerEntrance = FindObjectOfType<CostumerEntrance>();
            _levelTimer = GameManager.Instance.GetManager<LevelTimer>();
            LoadLevelData();
        }

        private void LoadLevelData()
        {
            string chapter = GameManager.Instance.GetCurrentChapter();
            if (string.IsNullOrEmpty(chapter))
            {
                UnityEngine.Debug.LogWarning("[CustomerManager] Current chapter is empty/null.");
                return;
            }

            string path = $"ChapterData/CustomerList/{chapter}";
            TextAsset jsonText = Resources.Load<TextAsset>(path);
            
            if (jsonText == null)
            {
                UnityEngine.Debug.LogError($"[CustomerManager] Could not load spawn data at {path}");
                return;
            }

            try 
            {
                _levelData = JsonUtility.FromJson<LevelSpawnData>(jsonText.text);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[CustomerManager] Failed to parse JSON: {e.Message}");
                return;
            }
            
            if (_levelData != null)
            {
                UnityEngine.Debug.Log($"[CustomerManager] Loaded spawn data for {chapter}. TimerStart: {_levelData.timerStart}");
                
                if (_levelTimer != null)
                {
                    _levelTimer.StartTimer(_levelData.timerStart);
                }

                GenerateSpawnTimes();
            }
        }

        private void GenerateSpawnTimes()
        {
            _spawnTimes.Clear();
            if (_levelData.npc1 != null)
            {
                foreach (var rule in _levelData.npc1)
                {
                    for (int i = 0; i < rule.count; i++)
                    {
                        // [time-randomSec, time+randomSec]
                        float randomOffset = UnityEngine.Random.Range((float)-rule.randomSec, (float)rule.randomSec);
                        float spawnTime = rule.time + randomOffset;
                        _spawnTimes.Add(spawnTime);
                    }
                }
            }
            
            // Sort descending because timer counts down (100 -> 0)
            _spawnTimes.Sort((a, b) => b.CompareTo(a));
            
            UnityEngine.Debug.Log($"[CustomerManager] Generated {_spawnTimes.Count} spawn events.");
        }

        private void Update()
        {
            if (_levelTimer == null || !_levelTimer.IsRunning) return;

            // Check if we need to spawn
            while (_spawnTimes.Count > 0 && _levelTimer.RemainingTime <= _spawnTimes[0])
            {
                SpawnCustomer();
                _spawnTimes.RemoveAt(0);
            }
        }

        private void SpawnCustomer()
        {
            if (_costumerEntrance != null)
            {
                _costumerEntrance.SpawnCustomer();
            }
        }
    }
}