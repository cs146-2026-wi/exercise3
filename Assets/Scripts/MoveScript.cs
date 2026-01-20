using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MoveScript : MonoBehaviour
{
    [Header("Tilemap References")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap obstacleTilemap;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float guardSpeedMultiplier = 0.5f;

    [Header("Collision Settings")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private Vector3 positionOffset = new Vector3(0f, 0.5f, 0f); // Offset to adjust player position (raise feet)

    private Vector3 targetPosition;
    private Vector3 homePosition; // Starting position to teleport back to on reset
    private SpriteRenderer spriteRenderer;
    private float lastHorizontalDirection = 1f; // 1 = right, -1 = left
    
    // Public properties for state observation
    public bool IsMoving { get; private set; } = false;
    public bool IsAttacking { get; private set; } = false;
    public bool IsGuarding { get; private set; } = false;
    public Vector2 LastMoveDirection { get; private set; } = Vector2.zero;

    void Start()
    {
        // Get SpriteRenderer component
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Snap to nearest tile center on start
        Vector3Int cellPos = groundTilemap.WorldToCell(transform.position);
        targetPosition = groundTilemap.GetCellCenterWorld(cellPos) + positionOffset;
        transform.position = targetPosition;
        
        // Save home position for reset
        homePosition = targetPosition;
        
        // Check for coins at starting position
        CheckForCoins(targetPosition);
    }

    void Update()
    {
        // Handle attack input (trigger action) - can trigger even while moving
        if (Input.GetKeyDown(KeyCode.Space) && !IsAttacking)
        {
            TriggerAttack();
        }

        // Handle guard input (hold action) - can trigger even while moving
        bool guardInput = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if (guardInput != IsGuarding && !IsAttacking)
        {
            SetGuard(guardInput);
        }

        // Handle movement - attacking continues at normal speed, guarding slows movement
        if (IsMoving)
        {
            // Calculate current speed based on state
            float currentSpeed = moveSpeed;
            if (IsGuarding)
            {
                currentSpeed *= guardSpeedMultiplier;
            }

            // Move toward target position
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, currentSpeed * Time.deltaTime);

            // Check if we've arrived
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                IsMoving = false;
                
                // Check for coins at this position (physics-free detection)
                CheckForCoins(targetPosition);
            }
        }
        
        // Allow new movement input when not currently moving (but can be attacking or guarding)
        if (!IsMoving)
        {
            // Get movement input
            Vector2 input = Vector2.zero;

            if (Input.GetKey(KeyCode.UpArrow)) input.y += 1;
            if (Input.GetKey(KeyCode.DownArrow)) input.y -= 1;
            if (Input.GetKey(KeyCode.RightArrow)) input.x += 1;
            if (Input.GetKey(KeyCode.LeftArrow)) input.x -= 1;

            if (input != Vector2.zero)
            {
                TryMove(input);
            }
        }
    }

    private void TryMove(Vector2 direction)
    {
        // Calculate target position (one tile in the input direction)
        Vector3 moveVector = new Vector3(direction.x, direction.y, 0f);
        Vector3 newTargetPosition = targetPosition + moveVector;

        // Check if there's ground at the target position
        Vector3Int targetCell = groundTilemap.WorldToCell(newTargetPosition);
        if (!groundTilemap.HasTile(targetCell))
        {
            return; // No ground, can't move there
        }

        // Check if there's an obstacle at the target position
        if (obstacleTilemap != null && obstacleTilemap.HasTile(targetCell))
        {
            return; // Obstacle detected, can't move there
        }

        // Check if there's a wall collider at the target position
        Vector3 cellCenter = groundTilemap.GetCellCenterWorld(targetCell);
        if (Physics2D.OverlapPoint(cellCenter, wallLayer))
        {
            return; // Wall detected, can't move there
        }

        // All clear, move to the new position
        targetPosition = cellCenter + positionOffset;
        IsMoving = true;
        LastMoveDirection = direction;
        
        // Update sprite direction based on horizontal movement (only when not guarding)
        if (direction.x != 0 && !IsGuarding)
        {
            lastHorizontalDirection = direction.x;
            FlipSprite(direction.x < 0);
        }
    }

    private void FlipSprite(bool flipLeft)
    {
        if (spriteRenderer != null)
        {
            // Use SpriteRenderer.flipX if available
            spriteRenderer.flipX = flipLeft;
        }
        else
        {
            // Fallback to transform scale flipping
            Vector3 scale = transform.localScale;
            scale.x = flipLeft ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    private void TriggerAttack()
    {
        IsAttacking = true;
        
        // Reset attack state after a delay (animation controller handles animation timing)
        StartCoroutine(ResetAttackState());
    }

    private void SetGuard(bool guard)
    {
        IsGuarding = guard;
    }

    private IEnumerator ResetAttackState()
    {
        // Wait for attack animation to complete
        // This is a gameplay timing, animation controller handles visual timing
        yield return new WaitForSeconds(0.5f);
        
        IsAttacking = false;
    }

    private void CheckForCoins(Vector3 position)
    {
        // Find all CoinCollector scripts in the scene and check if player is at their position
        // This is a simple approach - for better performance with many coins, use a layer mask or tag-based search
        CoinCollector[] coins = FindObjectsOfType<CoinCollector>();
        foreach (CoinCollector coin in coins)
        {
            if (coin != null)
            {
                coin.TryCollect(position);
            }
        }
    }

    // Public method to reset player to home position (called by CoinManager on game reset)
    public void ResetToHome()
    {
        // Teleport to home position
        transform.position = homePosition;
        targetPosition = homePosition;
        
        // Reset movement state
        IsMoving = false;
        IsAttacking = false;
        IsGuarding = false;
        
        Debug.Log($"MoveScript: Player teleported to home position {homePosition}");
    }
}
