using UnityEngine;

namespace Building
{
    public interface IInteract
    {
        Transform Transform { get; }
        void Interact();
    }
}