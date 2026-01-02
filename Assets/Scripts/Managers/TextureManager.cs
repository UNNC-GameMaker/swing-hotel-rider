using UnityEngine;
using System.Collections.Generic;

namespace Managers
{
    public class TextureManager : Manager
    {

        private Dictionary<string, Sprite> _spriteCache;

        void Awake()
        {
            _spriteCache = new Dictionary<string, Sprite>();
            UnityEngine.Debug.Log("TextureManager: Awake");
        }

        /// <summary>
        /// Loads a sprite from the Resources folder.
        /// </summary>
        /// <param name="path">The path relative to the Resources folder (e.g., "Order/Burger").</param>
        /// <returns>The loaded Sprite, or null if not found.</returns>
        public Sprite GetSprite(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                UnityEngine.Debug.LogWarning("TextureManager: Path is empty or null.");
                return null;
            }
            
            path = "Textures/" + path;
            
            if (_spriteCache.ContainsKey(path))
            {
                return _spriteCache[path];
            }

            Sprite loadedSprite = Resources.Load<Sprite>(path);

            if (loadedSprite != null)
            {
                _spriteCache[path] = loadedSprite;
            }
            else
            {
                UnityEngine.Debug.LogError($"TextureManager: Failed to load sprite at path 'Resources/{path}'");
            }

            return loadedSprite;
        }

        /// <summary>
        /// Clears the internal sprite cache to free memory.
        /// </summary>
        public void ClearCache()
        {
            _spriteCache.Clear();
            Resources.UnloadUnusedAssets();
        }

        public override void Init()
        {
            UnityEngine.Debug.Log("TextureManager: Init");
            GameManager.Instance.RegisterManager(this);
        }
    }
}