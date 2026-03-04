using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;

    private float leftBorder;
    private float rightBorder;
    public float buffer = 0.6f;

    private Rigidbody2D rb;
    private Camera mainCam;
    private bool isDead = false;

    // Added field to carry horizontal input from Update() to FixedUpdate()
    private float moveX;

    [Header("Physics Polish")]
    public float lerpSpeed = 10f; // Adjust for "snappiness"


    // Hard floor — set by GameManager to platform_0's Y so the player can never fall through it.
    private float floorY = float.MinValue;

    public void SetFloorY(float y) { floorY = y; }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCam = Camera.main;
        RefreshBorders();
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
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (KeyBindings.IsRightPressed())
        {
            acc.x = 1f;
            transform.localScale = new Vector3(-1, 1, 1);
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