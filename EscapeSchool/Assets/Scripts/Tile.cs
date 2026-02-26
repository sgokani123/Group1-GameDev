using UnityEngine;

public class Tile : MonoBehaviour
{
    [Header("Base setting")]
    public int tileType; // 0 normal, 1 broken, 2 disposable, 3 spring, 4 horiz, 5 vert
    public Sprite[] sprites;

    [Header("Move settings")]
    public float moveSpeed = 2f;
    public float moveRange = 3f;

    private SpriteRenderer sp;
    private Rigidbody2D rb;
    private Collider2D col;

    private Vector3 startPos;
    private int direction = 1;

    void Awake()
    {
        sp = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    void Start()
    {
        ApplyType(tileType, 0f);
    }

    public void ApplyType(int newType, float difficulty01, float gap = 0f)
    {
        tileType = newType;

        startPos = transform.position;
        direction = 1;

        // Reset physics for pooling
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic; // or Static if your prefab works with it
        }

        if (col != null) col.enabled = true;

        // Difficulty scaling (optional but nice):
        // make moving platforms faster + a bit wider movement later
        if (tileType == 4 || tileType == 5)
        {
            moveSpeed = Mathf.Lerp(2f, 3.8f, difficulty01);
            moveRange = Mathf.Lerp(3f, 4.2f, difficulty01);
        }
        else
        {
            moveSpeed = 2f;
            moveRange = 3f;
        }

        // Horizontal mover: always center it and sweep edge-to-edge
        if (tileType == 4)
        {
            Camera cam = Camera.main;
            if (cam != null)
                moveRange = cam.orthographicSize * cam.aspect - 0.3f;
            startPos = new Vector3(0f, startPos.y, startPos.z);
            transform.position = new Vector3(0f, transform.position.y, transform.position.z);
        }
        // Vertical mover: cap range so it cannot overlap the platform directly below.
        // The spawner will ensure the platform above is also far enough.
        else if (tileType == 5 && gap > 0f)
        {
            float maxRange = Mathf.Max(gap - 0.4f, 0.3f);
            moveRange = Mathf.Min(moveRange, maxRange);
        }

        if (sp != null && sprites != null && tileType >= 0 && tileType < sprites.Length)
            sp.sprite = sprites[tileType];
    }

    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        if (tileType == 4)
        {
            transform.position += new Vector3(moveSpeed * direction * Time.deltaTime, 0, 0);
            if (transform.position.x > startPos.x + moveRange) direction = -1;
            else if (transform.position.x < startPos.x - moveRange) direction = 1;
        }
        else if (tileType == 5)
        {
            transform.position += new Vector3(0, moveSpeed * direction * Time.deltaTime, 0);
            if (transform.position.y > startPos.y + moveRange) direction = -1;
            else if (transform.position.y < startPos.y - moveRange) direction = 1;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        var player = collision.GetComponent<Player>();
        var playerRb = collision.GetComponent<Rigidbody2D>();
        if (player == null || playerRb == null) return;
        if (!collision.CompareTag("Player")) return;
        if (playerRb.linearVelocity.y > 0.1f) return; // only trigger when falling

        switch (tileType)
        {
            case 0:
            case 4:
            case 5:
                player.Jump(1f);
                break;

            case 1: // broken
                ConsumeAndFall();
                break;

            case 2: // disposable (one-time)
                player.Jump(1f);
                ConsumeAndFall();
                break;

            case 3: // spring
                player.Jump(1.5f);
                break;

            default:
                player.Jump(1f);
                break;
        }
    }

    void ConsumeAndFall()
    {
        if (col != null) col.enabled = false;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1f;
        }
    }
}
