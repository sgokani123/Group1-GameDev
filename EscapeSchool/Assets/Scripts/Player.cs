using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    // Min/max world-units per second for the speed slider (slider 0-100 maps across this range)
    public const float SpeedMin = 2f;
    public const float SpeedMax = 14f;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;

    private float leftBorder;
    private float rightBorder;
    public float buffer = 0.6f;

    private Rigidbody2D rb;
    private Camera mainCam;
    private bool isDead = false;

    // Original sprite and scale — used to normalise skin swaps so size stays consistent
    private Sprite _originalSprite;
    private Vector3 _originalScale;

    // Added field to carry horizontal input from Update() to FixedUpdate()
    private float moveX;

    [Header("Physics Polish")]
    public float lerpSpeed = 10f; // Adjust for "snappiness"


    // Hard floor — set by GameManager to platform_0's Y so the player can never fall through it.
    private float floorY = float.MinValue;

    public void SetFloorY(float y) { floorY = y; }

    /// <summary>Swaps the player sprite while keeping the same world size as the original.</summary>
    public void ApplySkin(Sprite newSprite)
    {
        if (newSprite == null) return;
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        Sprite baseSprite = _originalSprite != null ? _originalSprite : sr.sprite;
        if (baseSprite == null) { sr.sprite = newSprite; return; }

        // World size of the baseline sprite at the recorded scale
        float origW = (baseSprite.rect.width  / baseSprite.pixelsPerUnit) * Mathf.Abs(_originalScale.x);
        float origH = (baseSprite.rect.height / baseSprite.pixelsPerUnit) * Mathf.Abs(_originalScale.y);

        // Scale needed so the new sprite occupies the same world size
        float newPPU = newSprite.pixelsPerUnit;
        float scaleX = origW / (newSprite.rect.width  / newPPU);
        float scaleY = origH / (newSprite.rect.height / newPPU);

        // Save collider world-space offsets BEFORE scale changes so we can restore them
        var circle = GetComponent<CircleCollider2D>();
        var box    = GetComponent<BoxCollider2D>();
        Vector2 circleWorldOffset = circle != null ? Vector2.Scale(circle.offset, transform.localScale) : Vector2.zero;
        Vector2 boxWorldOffset    = box    != null ? Vector2.Scale(box.offset,    transform.localScale) : Vector2.zero;

        sr.sprite = newSprite;
        Vector3 newScale = new Vector3(
            Mathf.Sign(_originalScale.x) * scaleX,
            Mathf.Sign(_originalScale.y) * scaleY,
            _originalScale.z);
        transform.localScale = newScale;

        // Restore collider offsets in new local space so world position is unchanged
        if (circle != null)
            circle.offset = new Vector2(circleWorldOffset.x / newScale.x, circleWorldOffset.y / newScale.y);
        if (box != null)
            box.offset = new Vector2(boxWorldOffset.x / newScale.x, boxWorldOffset.y / newScale.y);
    }

    /// <summary>Wraps the player across screen edges. Called by Rocket during flight
    /// because player.enabled = false disables Update().</summary>
    public void WrapPosition()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return;
        Vector3 pos = transform.position;
        if (pos.x < leftBorder - buffer)
            pos.x = rightBorder + buffer;
        else if (pos.x > rightBorder + buffer)
            pos.x = leftBorder - buffer;
        transform.position = pos;
    }

    /// <summary>Called by OptionsMenuController slider. sliderValue 0-100.</summary>
    public void SetSpeedFromSlider(float sliderValue)
    {
        moveSpeed = Mathf.Lerp(SpeedMin, SpeedMax, sliderValue / 100f);
        PlayerPrefs.SetFloat("PlayerSpeed", sliderValue);
        PlayerPrefs.Save();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCam = Camera.main;
        RefreshBorders();

        // Record the baseline sprite and scale so skin swaps stay the same world size
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) _originalSprite = sr.sprite;
        _originalScale = transform.localScale;

        // Restore saved speed
        float saved = PlayerPrefs.GetFloat("PlayerSpeed", 38f);
        SetSpeedFromSlider(saved);
    }

    // Ensure references are valid when the component is enabled (prevents NRE when toggled)
    void OnEnable()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (mainCam == null) mainCam = Camera.main;
        RefreshBorders();
    }

    public void RefreshBorders()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return;
        Vector3 screenLeft  = mainCam.ScreenToWorldPoint(new Vector3(0, 0, 0));
        Vector3 screenRight = mainCam.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0));
        leftBorder  = screenLeft.x;
        rightBorder = screenRight.x;
    }

    void Update()
    {
        if (isDead) return;
        if (Keyboard.current == null) return;

        // Ensure mainCam is available for the death check and wrapping calculations
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return;

        Vector3 acc = Vector3.zero;

        if (KeyBindings.IsLeftPressed())
        {
            acc.x = -1f;
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (KeyBindings.IsRightPressed())
        {
            acc.x = 1f;
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        // Store horizontal input for FixedUpdate physics write
        moveX = acc.x;

        // Screen-edge wrapping
        Vector3 pos = transform.position;
        if (pos.x < leftBorder - buffer)
            pos.x = rightBorder + buffer;
        else if (pos.x > rightBorder + buffer)
            pos.x = leftBorder - buffer;
        transform.position = pos;

        // Death check — fell below the bottom of the camera view
        float camBottom = mainCam.transform.position.y - mainCam.orthographicSize;
        if (transform.position.y < camBottom - 1f)
        {
            Die();
        }
    }

    // FixedUpdate: physics writes happen here for stability
    
    void FixedUpdate()
    {
        if (isDead || rb == null) return;

        // Hard floor: if the player reaches platform_0's Y, bounce them back up.
        // This is a guaranteed safety net regardless of trigger/collision state.
        if (rb.position.y <= floorY && rb.linearVelocity.y <= 0f)
        {
            rb.position = new Vector2(rb.position.x, floorY);
            Jump(1f);
        }

        float targetXVelocity = moveX * moveSpeed;
        // Smoothly transition to the target velocity instead of snapping
        float newX = Mathf.Lerp(rb.linearVelocity.x, targetXVelocity, Time.fixedDeltaTime * lerpSpeed);
        
        rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
    }

    public void Jump(float multiplier)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(new Vector2(0, jumpForce * multiplier), ForceMode2D.Impulse);

        // Play jump sound effect (index 3 in SoundManager's sfx array)
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(3);
    }

    private void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        GameManager.Instance.GameOver();
    }

   
    public void ResetPlayer(Vector3 startPosition)
    {
        isDead = false;
        this.enabled = true; 

        transform.position = startPosition;
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 1f;
        rb.linearVelocity = Vector2.zero;

        foreach (Transform child in transform)
        {
            
            if (child.name.ToLower().Contains("rocket"))
            {
                Destroy(child.gameObject);
            }
        }

        RefreshBorders();
        Jump(1f);
    }
}