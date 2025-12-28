using UnityEngine;

namespace Buliding
{
    public interface IInteract
    {
        Transform Transform { get; }
        void Interact();
    }
}