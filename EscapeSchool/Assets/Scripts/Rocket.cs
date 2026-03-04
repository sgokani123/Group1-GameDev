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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isUsed || !collision.CompareTag("Player")) return;

        Player player = collision.GetComponent<Player>();
        if (player == null || !player.enabled) return;

        isUsed = true;
        StartCoroutine(FlyRoutine(player));
    }


    private IEnumerator FlyRoutine(Player player)
    {
        if (player == null) yield break;

        GetComponent<Collider2D>().enabled = false;
        transform.SetParent(player.transform);
        transform.localPosition = offset;

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