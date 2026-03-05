using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class Rocket : MonoBehaviour
{
    public float flyDistance = 14f;       // how far the rocket carries the player
    public float flySpeed = 18f;
    public float gravityRampTime = 1.2f;  // seconds over which gravity eases back in after the boost
    public Vector3 offset = new Vector3(0, 0, 0.1f);

    private bool isUsed = false;
    private float originalGravity = 1f;
    private Vector3 _attachOffset;  // set in Awake from StoreManager, falls back to offset

    void Awake()
    {
        // Default attach offset is the Inspector value
        _attachOffset = offset;

        if (GameManager.Instance?.storeMenuController == null) return;
        var store  = GameManager.Instance.storeMenuController;
        var sprite = store.GetSelectedRocketSprite();
        if (sprite == null) return;
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;

        // Normalise scale so the rocket stays the same world size as the default
        Sprite orig = sr.sprite;
        float origW = orig.rect.width  / orig.pixelsPerUnit;
        float origH = orig.rect.height / orig.pixelsPerUnit;
        float scaleX = origW / (sprite.rect.width  / sprite.pixelsPerUnit);
        float scaleY = origH / (sprite.rect.height / sprite.pixelsPerUnit);
        sr.sprite = sprite;
        Vector3 s = transform.localScale;
        transform.localScale = new Vector3(s.x * scaleX, s.y * scaleY, s.z);

        // Fit pickup collider to the new sprite's actual pixel bounds
        float worldW = (sprite.rect.width  / sprite.pixelsPerUnit) * Mathf.Abs(transform.localScale.x);
        float worldH = (sprite.rect.height / sprite.pixelsPerUnit) * Mathf.Abs(transform.localScale.y);
        var box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            box.size   = new Vector2(worldW / Mathf.Abs(transform.localScale.x),
                                     worldH / Mathf.Abs(transform.localScale.y));
            box.offset = Vector2.zero;
        }
        var circle = GetComponent<CircleCollider2D>();
        if (circle != null)
        {
            circle.radius = Mathf.Min(worldW, worldH) * 0.5f / Mathf.Abs(transform.localScale.x);
            circle.offset = Vector2.zero;
        }

        // Per-skin attachment offset
        _attachOffset = store.GetSelectedRocketOffset(offset);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isUsed || !collision.CompareTag("Player")) return;

        Player player = collision.GetComponent<Player>();
        if (player == null || !player.enabled) return;

        isUsed = true;

       if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(2);

        StartCoroutine(FlyRoutine(player));
    }


    private IEnumerator FlyRoutine(Player player)
    {
        if (player == null) yield break;

        GetComponent<Collider2D>().enabled = false;
        transform.SetParent(player.transform);
        transform.localPosition = _attachOffset;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb == null) yield break;

        // Grey out and disable all enemies for the duration of the flight.
        SetAllEnemiesImmune(true);

        // record original gravity
        originalGravity = rb.gravityScale;
        player.enabled = false;
        rb.gravityScale = 0;

        float targetY = player.transform.position.y + flyDistance;


        // ── Boost phase: carry the player upward ─────────────────────────
        while (player != null && this != null && player.transform.position.y < targetY)
        {
            if (GameManager.Instance.State != GameManager.GameState.Playing) break;

            float moveX = 0f;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX = -1f;
                else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX = 1f;
            }
            rb.linearVelocity = new Vector2(moveX * player.moveSpeed, flySpeed);
            player.WrapPosition();
            yield return null;
        }

        // ── Gravity-ramp phase: ease gravity back in so the player arcs down ─
        if (player != null && GameManager.Instance.State == GameManager.GameState.Playing)
        {
            float elapsed = 0f;
            while (elapsed < gravityRampTime && player != null)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / gravityRampTime;
                // Smoothly restore gravity scale
                rb.gravityScale = Mathf.Lerp(0f, originalGravity, t);
                // Let horizontal movement keep working while falling
                float moveX = 0f;
                if (Keyboard.current != null)
                {
                    if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX = -1f;
                    else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX = 1f;
                }
                rb.linearVelocity = new Vector2(moveX * player.moveSpeed, rb.linearVelocity.y);
                player.WrapPosition();
                yield return null;
            }
        }

        // Fully restore gravity and hand control back to the player
        if (player != null)
        {
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.gravityScale = originalGravity;
                // Preserve whatever horizontal velocity the player had; zero the Y so they fall naturally
                playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 0);
            }
            player.enabled = true;
        }

        // Restore all enemies to normal.
        SetAllEnemiesImmune(false);

        if (this != null) Destroy(gameObject);
    }

    static void SetAllEnemiesImmune(bool immune)
    {
        foreach (Enemy e in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
            e.SetRocketImmune(immune);
    }
}