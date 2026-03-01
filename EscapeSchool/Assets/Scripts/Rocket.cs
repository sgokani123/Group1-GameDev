using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class Rocket : MonoBehaviour
{
    public float flyDistance = 10f;
    public float flySpeed = 18f;
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

        // record original gravity
        originalGravity = rb.gravityScale;
        player.enabled = false;
        rb.gravityScale = 0;

        float targetY = player.transform.position.y + flyDistance;


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
            yield return null;
        }

        // logic endding
        if (player != null)
        {
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.gravityScale = originalGravity; 
                playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 0);
            }
            player.enabled = true;
        }

        if (this != null) Destroy(gameObject);
    }
}