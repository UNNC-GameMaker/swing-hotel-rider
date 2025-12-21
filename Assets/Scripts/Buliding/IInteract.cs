using UnityEngine;

public interface IInteract
{
    Transform Transform { get; }
    void Interact();
}