using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class TiltIndicator : MonoBehaviour
    {
        [SerializeField] private Image image;
        private TiltManager _manager;

        private void Awake()
        {
            _manager = GameManager.Instance.GetManager<TiltManager>();
        }

        private void Update()
        {
            var forceX = Mathf.Sin(_manager.TiltTarget * Mathf.Deg2Rad);
            var forceY = -Mathf.Cos(_manager.TiltTarget * Mathf.Deg2Rad) * 0.5f;
            var angle = Mathf.Atan2(forceY, forceX) * Mathf.Rad2Deg;
            image.transform.rotation = Quaternion.Euler(0, 0, -angle - 90);
        }
    }
}