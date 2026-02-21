using UnityEngine;

public class Tile : MonoBehaviour
{
    [Header("Base setting")]
    public int tileType;        // 0: Normal, 1: Broken, 2: Disposable, 3: Spring, 4: Horizontal movement, 5: Vertical movement
    public Sprite[] sprites;
    private SpriteRenderer sp;

    [Header("move platform setting")]
    public float moveSpeed = 2f;
    public float moveRange = 3f;
    private Vector3 startPos;
    private int direction = 1;    // 1 is up, -1 is down

    // Add these fields near the top
    private Rigidbody2D rb;
    private Collider2D col;

    // Cache components for pool-safe reuse
    private void Awake()
    {
        sp  = GetComponent<SpriteRenderer>();
        rb  = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    // First-time init (pool reuse won't call Start again)
    private void Start()
    {
        // First-time init (pool reuse won't call Start again)
        ApplyType(tileType);
    }

    // Called every time the spawner reuses this platform
    public void ApplyType(int newType)
    {
        tileType = newType;

        // Reset baseline for movement
        startPos = transform.position;
        direction = 1;

        // Reset physics (broken/disposable may have made it Dynamic)
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 0f;

            // pick what your prefab normally is:
            // rb.bodyType = RigidbodyType2D.Static;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Ensure collider enabled
        if (col != null) col.enabled = true;

        // Update sprite for this type
        if (sp != null && sprites != null && tileType >= 0 && tileType < sprites.Length)
            sp.sprite = sprites[tileType];

        // Ensure object active when reused
        gameObject.SetActive(true);
    }

    private void Update()
    {
        // every hz
        HandleMovement();
    }

    private void HandleMovement()
    {
        //horizontal logic
        if (tileType == 4)
        {
            // move direct
            transform.position += new Vector3(moveSpeed * direction * Time.deltaTime, 0, 0);

            // if too far away than turn round
            if (transform.position.x > startPos.x + moveRange)
            {
                direction = -1;
            }
            // if too left turn round
            else if (transform.position.x < startPos.x - moveRange)
            {
                direction = 1;
            }
        }
        // vertical logic
        else if (tileType == 5)
        {
            transform.position += new Vector3(0, moveSpeed * direction * Time.deltaTime, 0);

            if (transform.position.y > startPos.y + moveRange)
            {
                direction = -1;
            }
            else if (transform.position.y < startPos.y - moveRange)
            {
                direction = 1;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();

        // only drop down 
        if (collision.CompareTag("Player") && playerRb != null && playerRb.linearVelocity.y <= 0.1f)
        {
            switch (tileType)
            {
                case 0: // normal
                case 4: // horizontal
                case 5: // vertical
                    collision.GetComponent<Player>().Jump(1);
                    break;

                case 1: // broken platform
                    Rigidbody2D rb1 = GetComponent<Rigidbody2D>();
                    if (rb1 != null)
                    {
                        rb1.bodyType = RigidbodyType2D.Dynamic;
                        rb1.gravityScale = 1;
                    }
                    // Pool-safe: disable instead of destroying
                    gameObject.SetActive(false);
                    break;

                case 2: // one time platfrom
                    collision.GetComponent<Player>().Jump(1);
                    Rigidbody2D rb2 = GetComponent<Rigidbody2D>();
                    if (rb2 != null)
                    {
                        rb2.bodyType = RigidbodyType2D.Dynamic;
                        rb2.gravityScale = 1;
                    }
                    break;

                case 3: // jumpdouble platform
                    collision.GetComponent<Player>().Jump(1.5f);
                    break;

                default:
                    collision.GetComponent<Player>().Jump(1);
                    break;
            }
        }
    }
}

