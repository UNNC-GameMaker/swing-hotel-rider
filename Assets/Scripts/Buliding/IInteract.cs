using UnityEngine;

public interface IInteract
{
    void Interact();
    
    Transform Transform { get; }
}