using System;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class SFXManager : Manager
    {
        public static SFXManager Instance { get; private set; }
        
        private AudioSource _audioSource;
        private Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        public override void Init()
        {
            GameManager.Instance.RegisterManager(this);
        }

        public void PlayClip(String clipName)
        {
            // play audio by name
            if (Instance._clips.TryGetValue(clipName, out var clip))
            {
                Instance.PlayClip(clip);
                return;
            }

            // attempt to load from Resources if not already cached
            var loaded = Resources.Load<AudioClip>("Audio/SoundEffects/"+clipName);
            if (loaded != null)
            {
                Instance._clips[clipName] = loaded;
                Instance.PlayClip(loaded);
            }
            else
            {
                Debug.LogWarning($"SFXManager: clip '{clipName}' not found");
            }
        }
        
        //play audio by AudioClip
        public void PlayClip(AudioClip clip)
        {
            if (!clip)
            {
                throw new NullReferenceException($"SFXManager: clip is null");
            }
            _audioSource.PlayOneShot(clip);
        }
    }
}