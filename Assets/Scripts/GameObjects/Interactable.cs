using UnityEngine;

namespace GameObjects
{
    public abstract class Interactable : MonoBehaviour
    {
        public virtual void OnInteract()
        {
            UnityEngine.Debug.Log("Interact");
        }
    }
}