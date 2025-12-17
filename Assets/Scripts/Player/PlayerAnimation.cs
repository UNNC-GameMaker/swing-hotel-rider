using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    // TODO : make these private later I think
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    
    private PlayerMovement _playerMovement;
    
    private PlayerMovement.MovementState _movementState;
    private Transform _spriteNode;

    void Awake()
    {  
        _playerMovement = GetComponent<PlayerMovement>();
        _spriteNode = transform.Find("SpriteNode");
        animator = _spriteNode.GetComponent<Animator>();
        spriteRenderer = _spriteNode.GetComponent<SpriteRenderer>();
        Debug.Log("get Animator = ", animator);
        Debug.Log("get SpriteNode = ", spriteRenderer);
    }

    void Update()
    {
        // Default sprite facing is right
        _movementState = _playerMovement.currentState;
        if (_movementState == PlayerMovement.MovementState.Left || _movementState == PlayerMovement.MovementState.Right)
        {
            
        }
    }
    
}
