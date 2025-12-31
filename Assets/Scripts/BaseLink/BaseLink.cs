using Managers;
using UnityEngine;

namespace BaseLink
{
    public class BaseLink : MonoBehaviour
    {
        private float _tiltAngle;

        private TiltManager _tiltManager;

        // Start is called before the first frame update
        private void Start()
        {
            _tiltManager = GameManager.Instance.GetManager<TiltManager>();
            UnityEngine.Debug.Log("BaseLink Init");
        }

        // Update is called once per frame
        private void Update()
        {
            _tiltAngle = _tiltManager.TiltTarget;
            transform.localRotation = Quaternion.Euler(0f, 0f, _tiltAngle);
        }
    }
}