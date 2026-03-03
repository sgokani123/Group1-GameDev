using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("move setting")]
    [SerializeField] float speed = 2f;      // speed
    [SerializeField] float distance = 3f;   // scope

    Vector3 startPos;
    int direction = 0; // 0 left, 1 right

    SpriteRenderer sr;
    Collider2D col;

    void Awake()
    {
        sr  = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    void Start()
    {
        startPos = transform.position;
    }

    /// <summary>
    /// Called by PlatformSpawner to limit the patrol range so the enemy stays
    /// within the platform boundaries and does not clip into adjacent platforms.
    /// </summary>
    public void SetPatrolDistance(float maxDist)
    {
        distance = Mathf.Max(0.05f, maxDist);
    }

    /// <summary>
    /// Called by Rocket to grey out and disable all enemies during flight,
    /// then restore them when the rocket ends.
    /// </summary>
    public void SetRocketImmune(bool immune)
    {
        if (col != null) col.enabled = !immune;
        if (sr  != null) sr.color = immune ? new Color(0.55f, 0.55f, 0.55f, 0.6f) : Color.white;
    }

    void Update()
    {
        if (direction == 0) //left
        {
            transform.Translate(Vector2.left * speed * Time.deltaTime);

            //change direct
            if (transform.position.x < startPos.x - distance)
            {
                direction = 1;
                Flip();
            }
        }
        else //move
        {
            transform.Translate(Vector2.right * speed * Time.deltaTime);

            
            if (transform.position.x > startPos.x + distance)
            {
                direction = 0;
                Flip(); 
            }
        }
    }

   
    void Flip()
    {
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
        }
    }
}

