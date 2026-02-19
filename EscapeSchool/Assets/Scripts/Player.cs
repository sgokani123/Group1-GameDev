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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCam = Camera.main;
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

        Vector3 acc = Vector3.zero;

        if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
        {
            acc.x = -1f;
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
        {
            acc.x = 1f;
            transform.localScale = new Vector3(1, 1, 1);
        }

        // Apply horizontal movement directly
        rb.linearVelocity = new Vector2(acc.x * moveSpeed, rb.linearVelocity.y);

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;
        if (collision.CompareTag("Platform"))
        {
            Jump(1f);
        }
    }

    public void Jump(float multiplier)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(new Vector2(0, jumpForce * multiplier), ForceMode2D.Impulse);
    }

    private void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        GameManager.Instance.GameOver();
    }

    // Called by GameManager when restarting
    public void ResetPlayer(Vector3 startPosition)
    {
        isDead = false;
        transform.position = startPosition;
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;
        RefreshBorders();
        // Give initial jump so player immediately lands on a platform
        Jump(1f);
    }
}