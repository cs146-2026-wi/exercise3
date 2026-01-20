using UnityEngine;
using System;

public class CoinCollector : MonoBehaviour
{
    [Header("Coin Settings")]
    [SerializeField] private int coinValue = 1;

    [Header("Optional Effects")]
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private GameObject collectEffect;

    // Static event for coin collection notifications
    public static Action<int> CoinCollected;

    private AudioSource audioSource;
    private bool isCollected = false;
    private Collider2D coinCollider;

    void Start()
    {
        // Get or add a collider for overlap detection
        coinCollider = GetComponent<Collider2D>();
        if (coinCollider == null)
        {
            // Add a CircleCollider2D if none exists
            coinCollider = gameObject.AddComponent<CircleCollider2D>();
            Debug.Log($"CoinCollector on {gameObject.name}: Added CircleCollider2D automatically.");
        }
        
        // Try to get AudioSource component, or add one if needed
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && collectSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    // Public method that can be called from MoveScript or other scripts
    // This allows physics-free detection
    public bool TryCollect(Vector3 position)
    {
        if (isCollected) return false;
        
        // Check if the position is within the coin's collider bounds
        if (coinCollider != null && coinCollider.bounds.Contains(position))
        {
            CollectCoin(null);
            return true;
        }
        
        return false;
    }

    // Keep OnTriggerEnter2D for compatibility if you do add Rigidbody2D later
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isCollected)
        {
            CollectCoin(other.gameObject);
        }
    }

    private void CollectCoin(GameObject player)
    {
        // Prevent multiple collections
        isCollected = true;

        // Play sound effect if available
        if (collectSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(collectSound);
        }

        // Spawn collect effect if available
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

        // Disable the collider immediately so player can't re-collect
        if (coinCollider != null)
        {
            coinCollider.enabled = false;
        }

        // Play collect animation if CoinAnimator is present (check children since animator may be on child)
        CoinAnimator animator = GetComponentInChildren<CoinAnimator>();
        if (animator != null)
        {
            animator.PlayCollectAnimation(() =>
            {
                // Disable the coin after animation completes
                gameObject.SetActive(false);
                // Notify the coin manager
                OnCoinCollected(coinValue);
            });
        }
        else
        {
            // No animator, disable immediately (fallback behavior)
            gameObject.SetActive(false);
            OnCoinCollected(coinValue);
        }
    }

    // Override this method or subscribe to events to handle coin collection
    protected virtual void OnCoinCollected(int value)
    {
        Debug.Log($"CoinCollector: Coin collected! Value: {value}. Firing event...");
        
        // Notify the coin manager via static event
        if (CoinCollected != null)
        {
            CoinCollected.Invoke(value);
            Debug.Log("CoinCollector: Event fired successfully!");
        }
        else
        {
            Debug.LogWarning("CoinCollector: No subscribers to CoinCollected event! Make sure CoinManager is in the scene.");
        }
    }

    // Public method to reset the coin (called by CoinManager on restart)
    public void ResetCoin()
    {
        isCollected = false;
        gameObject.SetActive(true);
        
        // Re-enable the collider
        if (coinCollider != null)
        {
            coinCollider.enabled = true;
        }
        
        // Reset the animator state (check children since animator may be on child)
        CoinAnimator animator = GetComponentInChildren<CoinAnimator>();
        if (animator != null)
        {
            animator.ResetAnimator();
        }
    }
}
