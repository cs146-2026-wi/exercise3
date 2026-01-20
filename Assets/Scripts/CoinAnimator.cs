using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CoinAnimator : MonoBehaviour
{
    [Header("Hover Animation")]
    [SerializeField] private float hoverAmplitude = 0.5f; // How high/low the coin moves
    [SerializeField] private float hoverSpeed = 2f; // Speed of the hover animation
    [SerializeField] private float shadowScaleMin = 0.8f; // Minimum shadow scale when coin is at lowest point
    [SerializeField] private float shadowScaleMax = 1.2f; // Maximum shadow scale when coin is at highest point
    [SerializeField] private Transform shadowTransform; // Drag the shadow child GameObject here
    
    [Header("Collect Animation")]
    [SerializeField] private float popHeight = 1.5f; // How high the coin pops when collected
    [SerializeField] private float popDuration = 0.15f; // Duration of the upward pop
    [SerializeField] private float fadeDuration = 0.2f; // Duration of the fade out
    
    private Vector3 initialPosition;
    private Vector3 initialShadowScale;
    private bool isCollecting = false;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer shadowSpriteRenderer;
    private Color initialSpriteColor;
    private Color initialShadowColor;
    private bool initialPositionSet = false;
    
    // Awake is called before Start and OnEnable, so we capture position here
    void Awake()
    {
        if (!initialPositionSet)
        {
            initialPosition = transform.position;
            initialPositionSet = true;
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        // Capture initial position if not already set (fallback)
        if (!initialPositionSet)
        {
            initialPosition = transform.position;
            initialPositionSet = true;
        }
        
        // Get the sprite renderer for fading
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        
        // Store initial sprite color
        if (spriteRenderer != null)
        {
            initialSpriteColor = spriteRenderer.color;
        }
        
        // Store the initial shadow scale and color if shadow is assigned
        if (shadowTransform != null)
        {
            initialShadowScale = shadowTransform.localScale;
            shadowSpriteRenderer = shadowTransform.GetComponent<SpriteRenderer>();
            if (shadowSpriteRenderer != null)
            {
                initialShadowColor = shadowSpriteRenderer.color;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Don't run hover animation if we're playing collect animation
        if (isCollecting) return;
        
        // Calculate sine wave offset for vertical movement
        float sineValue = Mathf.Sin(Time.time * hoverSpeed);
        float verticalOffset = sineValue * hoverAmplitude;
        
        // Update coin position
        transform.position = initialPosition + Vector3.up * verticalOffset;
        
        // Update shadow scale based on coin height
        if (shadowTransform != null)
        {
            // Normalize sine value from [-1, 1] to [0, 1]
            float normalizedHeight = (sineValue + 1f) / 2f;
            
            // Interpolate shadow scale factor between min and max based on coin height
            float shadowScaleFactor = Mathf.Lerp(shadowScaleMin, shadowScaleMax, normalizedHeight);
            
            // Multiply the initial scale by the scale factor
            shadowTransform.localScale = initialShadowScale * shadowScaleFactor;
        }
    }
    
    /// <summary>
    /// Play the collect animation (pop up + fade out), then invoke the callback
    /// </summary>
    public void PlayCollectAnimation(Action onComplete)
    {
        if (isCollecting) return;
        isCollecting = true;
        StartCoroutine(CollectAnimationCoroutine(onComplete));
    }
    
    private IEnumerator CollectAnimationCoroutine(Action onComplete)
    {
        Vector3 startPos = transform.position;
        Vector3 peakPos = startPos + Vector3.up * popHeight;
        
        // Phase 1: Pop upward with easing
        float elapsed = 0f;
        while (elapsed < popDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / popDuration;
            // Use ease-out curve for snappy pop
            float easeOut = 1f - Mathf.Pow(1f - t, 3f);
            transform.position = Vector3.Lerp(startPos, peakPos, easeOut);
            
            // Shrink shadow as coin goes up
            if (shadowTransform != null)
            {
                float shadowScale = Mathf.Lerp(1f, 0.3f, easeOut);
                shadowTransform.localScale = initialShadowScale * shadowScale;
            }
            
            yield return null;
        }
        
        // Phase 2: Fade out quickly while continuing slight upward drift
        elapsed = 0f;
        Vector3 fadeStartPos = transform.position;
        Color startColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
        Color shadowStartColor = shadowSpriteRenderer != null ? shadowSpriteRenderer.color : Color.white;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            
            // Continue drifting up slightly
            transform.position = fadeStartPos + Vector3.up * (t * 0.3f);
            
            // Fade out the coin sprite
            if (spriteRenderer != null)
            {
                Color c = startColor;
                c.a = Mathf.Lerp(startColor.a, 0f, t);
                spriteRenderer.color = c;
            }
            
            // Fade out the shadow too
            if (shadowSpriteRenderer != null)
            {
                Color c = shadowStartColor;
                c.a = Mathf.Lerp(shadowStartColor.a, 0f, t);
                shadowSpriteRenderer.color = c;
            }
            
            yield return null;
        }
        
        // Animation complete, invoke callback
        onComplete?.Invoke();
    }
    
    /// <summary>
    /// Reset the animator state (called when coin is re-enabled)
    /// </summary>
    public void ResetAnimator()
    {
        isCollecting = false;
        
        // Restore position to original position (don't update initialPosition to current position!)
        // If initialPosition hasn't been captured yet, capture it now
        if (!initialPositionSet)
        {
            initialPosition = transform.position;
            initialPositionSet = true;
        }
        else
        {
            // Always restore to the original position
            transform.position = initialPosition;
        }
        
        // Reset sprite to original color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = initialSpriteColor;
        }
        
        // Reset shadow scale and color
        if (shadowTransform != null)
        {
            shadowTransform.localScale = initialShadowScale;
            if (shadowSpriteRenderer != null)
            {
                shadowSpriteRenderer.color = initialShadowColor;
            }
        }
    }
}
