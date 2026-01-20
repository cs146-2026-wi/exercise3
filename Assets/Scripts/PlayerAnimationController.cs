using System.Collections;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;
    private MoveScript moveScript;
    private SpriteRenderer spriteRenderer;

    void Start() {
        animator = GetComponent<Animator>();
        moveScript = GetComponent<MoveScript>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Check if player is moving (and not attacking or guarding)
        bool isMoving = moveScript.IsMoving && !moveScript.IsAttacking && !moveScript.IsGuarding;
        
        // Set the IsMoving parameter
        animator.SetBool("IsMoving", isMoving);
        
        // Set the Attack parameter
        animator.SetBool("IsAttacking", moveScript.IsAttacking);
        
        // Set the Guard parameter
        animator.SetBool("IsGuarding", moveScript.IsGuarding);
    }
}
