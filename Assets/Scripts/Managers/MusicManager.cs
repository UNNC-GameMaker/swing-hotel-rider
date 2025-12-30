using UnityEngine;

namespace Managers
{
    public class MusicManager : Manager
    {
        public static MusicManager Instance { get; private set; }
        
        private AudioSource _audioSource;
        
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
            _audioSource.loop = true;
        }

        public override void Init()
        {
            GameManager.Instance.RegisterManager(this);
        }

        public void PlayMusic(string musicName)
        {
            AudioClip clip = Resources.Load<AudioClip>("Audio/Background/" + musicName);
            if (clip == null)
            {
                clip = Resources.Load<AudioClip>(musicName);
            }
            
            if (clip != null)
            {
                if (_audioSource.isPlaying && _audioSource.clip == clip) return;
                
                _audioSource.clip = clip;
                _audioSource.Play();
            }
            else
            {
                Debug.LogWarning($"MusicManager: Could not find music '{musicName}'");
            }
        }
        
        public void StopMusic()
        {
            _audioSource.Stop();
        }
        
        public void SetVolume(float volume)
        {
            _audioSource.volume = Mathf.Clamp01(volume);
        }
    }
}