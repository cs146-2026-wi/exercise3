using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class CoinManager : MonoBehaviour
{
    [Header("Tilemap Overlay")]
    [SerializeField] private Tilemap winScreenTilemap; // The tilemap overlay for win screen
    [SerializeField] private GameObject winScreenTilemapObject; // The GameObject containing the tilemap (for enable/disable)

    [Header("Text Display (Optional)")]
    [SerializeField] private TextMeshProUGUI winTextUI; // UI text (if using Canvas)
    [SerializeField] private TextMeshPro winTextWorld; // World space text (if using world space)
    [SerializeField] private TextMeshProUGUI restartTextUI; // "Press any key to restart" text

    [Header("Settings")]
    [SerializeField] private string winMessage = "You Win!";
    [SerializeField] private bool pauseGameOnWin = true;

    private int totalCoins = 0; // Total coins in scene (all coins, enabled or disabled)
    private bool gameWon = false;

    void Start()
    {
        // Count all coins in the scene at start (this counts all coins regardless of enabled state)
        CountAllCoins();

        // Ensure win screen tilemap is visible
        if (winScreenTilemapObject != null)
        {
            winScreenTilemapObject.SetActive(true);
        }
        else if (winScreenTilemap != null)
        {
            winScreenTilemap.gameObject.SetActive(true);
        }

        // Subscribe to coin collection events
        CoinCollector.CoinCollected += HandleCoinCollected;
        Debug.Log("CoinManager: Subscribed to CoinCollector.CoinCollected event");
        
        // Hide restart text initially
        if (restartTextUI != null)
        {
            restartTextUI.gameObject.SetActive(false);
        }
        
        // Update text to show initial coin count
        UpdateCoinText();
    }

    void Update()
    {
        // Check for any key press to restart when game is won
        if (gameWon && Input.anyKeyDown)
        {
            Debug.Log("CoinManager: Key pressed while game won - calling ResetGame()");
            ResetGame();
        }
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        CoinCollector.CoinCollected -= HandleCoinCollected;
    }

    private void CountAllCoins()
    {
        // Count all coins in the scene (including disabled ones)
        CoinCollector[] allCoins = FindObjectsOfType<CoinCollector>(true); // true = include inactive
        totalCoins = allCoins.Length;

        Debug.Log($"CoinManager: Found {totalCoins} coins in the scene. Coins remaining: {GetRemainingCoins()}");
    }

    private void HandleCoinCollected(int value)
    {
        if (gameWon) return; // Already won, ignore

        int remaining = GetRemainingCoins();
        Debug.Log($"CoinManager: Coin collected! Coins remaining: {remaining}");

        // Update text to show remaining coins
        UpdateCoinText();

        // Check if all coins are collected (all disabled)
        if (remaining == 0)
        {
            WinGame();
        }
    }

    private void WinGame()
    {
        gameWon = true;
        Debug.Log("CoinManager: All coins collected! You win!");

        // Show win screen tilemap
        if (winScreenTilemapObject != null)
        {
            winScreenTilemapObject.SetActive(true);
        }
        else if (winScreenTilemap != null)
        {
            winScreenTilemap.gameObject.SetActive(true);
        }

        // Update text to show win message
        UpdateCoinText();

        // Show restart text
        if (restartTextUI != null)
        {
            restartTextUI.gameObject.SetActive(true);
        }

        // Pause the game if requested
        if (pauseGameOnWin)
        {
            Time.timeScale = 0f;
        }
    }

    // Public method to reset (called when any key is pressed after winning)
    public void ResetGame()
    {
        Debug.Log("CoinManager: ResetGame() called");
        
        Time.timeScale = 1f;
        gameWon = false;

        // Keep tilemap visible (don't hide it on reset)

        // Re-enable all coins
        ReenableAllCoins();

        // Reset player to home position
        ResetPlayer();

        // Snap camera to player
        SnapCameraToPlayer();

        // Hide restart text
        if (restartTextUI != null)
        {
            restartTextUI.gameObject.SetActive(false);
        }

        // Update text to show coin count again
        UpdateCoinText();
        
        Debug.Log("CoinManager: ResetGame() complete");
    }

    private void ResetPlayer()
    {
        Debug.Log("CoinManager: ResetPlayer() called");
        
        // Find the player and reset to home position
        MoveScript player = FindObjectOfType<MoveScript>();
        if (player != null)
        {
            Debug.Log($"CoinManager: Found player at {player.transform.position}, calling ResetToHome()");
            player.ResetToHome();
            Debug.Log($"CoinManager: Player now at {player.transform.position}");
        }
        else
        {
            Debug.LogWarning("CoinManager: Could not find player (MoveScript) to reset!");
        }
    }

    private void SnapCameraToPlayer()
    {
        // Find the camera and snap it to the player position
        CameraFollow cameraFollow = FindObjectOfType<CameraFollow>();
        if (cameraFollow != null)
        {
            MoveScript player = FindObjectOfType<MoveScript>();
            if (player != null)
            {
                // Snap camera directly to player position (with offset)
                Vector3 targetPos = player.transform.position + new Vector3(0, 0, -10);
                cameraFollow.transform.position = targetPos;
                Debug.Log($"CoinManager: Camera snapped to {targetPos}");
            }
        }
    }

    private void ReenableAllCoins()
    {
        // Find all coins (including disabled ones)
        CoinCollector[] allCoins = FindObjectsOfType<CoinCollector>(true); // true = include inactive
        
        foreach (CoinCollector coin in allCoins)
        {
            if (coin != null)
            {
                coin.ResetCoin();
            }
        }
        
        Debug.Log($"CoinManager: Re-enabled {allCoins.Length} coins. Coins remaining: {GetRemainingCoins()}");
    }

    private void UpdateCoinText()
    {
        string textToShow;
        int remaining = GetRemainingCoins();
        
        if (gameWon || remaining == 0)
        {
            textToShow = winMessage;
        }
        else
        {
            textToShow = $"Coins Left: {remaining}";
        }

        Debug.Log($"CoinManager: Updating text to '{textToShow}'");

        // Update UI text if assigned
        if (winTextUI != null)
        {
            winTextUI.text = textToShow;
            Debug.Log("CoinManager: Updated winTextUI");
        }
        else
        {
            Debug.LogWarning("CoinManager: winTextUI is not assigned!");
        }

        // Update world space text if assigned
        if (winTextWorld != null)
        {
            winTextWorld.text = textToShow;
            Debug.Log("CoinManager: Updated winTextWorld");
        }
    }

    // Public getters for UI display
    public int GetTotalCoins() => totalCoins;
    
    // Get remaining coins by counting currently enabled coins
    public int GetRemainingCoins()
    {
        CoinCollector[] enabledCoins = FindObjectsOfType<CoinCollector>(); // Only finds active/enabled coins
        return enabledCoins.Length;
    }
    
    public int GetCollectedCoins() => totalCoins - GetRemainingCoins();
    public float GetCollectionProgress() => totalCoins > 0 ? (float)GetCollectedCoins() / totalCoins : 0f;
}
