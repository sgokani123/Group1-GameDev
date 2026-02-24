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

    void Start()
    {
        
        startPos = transform.position;
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
        // check player
        if (collision.CompareTag("Player"))
        {
            Debug.Log("enemy find player£¡");

            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
        }
    }
}

