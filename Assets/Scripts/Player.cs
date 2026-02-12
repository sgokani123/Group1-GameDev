using UnityEngine;

using UnityEngine.InputSystem;



public class Player : MonoBehaviour

{

    private float leftBorder;
    private float rightBorder;
    public float buffer = 0.6f;

    void Start()

    {
        Vector3 screenLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));
        Vector3 screenRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0));
        leftBorder = screenLeft.x;
        rightBorder = screenRight.x;
    }


    void Update()
    {
       
        if (Keyboard.current == null) return;
        Vector3 acc = Vector3.zero;

        if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
        {
            acc.x = -0.1f;
            transform.localScale = new Vector3(-1, 1, 1); // Á³³¯×ó
        }
        else if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
        {
            acc.x = 0.1f;
            transform.localScale = new Vector3(1, 1, 1); // Á³³¯ÓÒ
        }

        
        if (acc != Vector3.zero)
        {
            Vector3 targetPos = transform.position + acc;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 5.0f * Time.deltaTime);
        }

        Vector3 currentPos = transform.position;
       
        if (currentPos.x < leftBorder - buffer)
        {
            currentPos.x = rightBorder + buffer;
        }
        
        else if (currentPos.x > rightBorder + buffer)
        {
            currentPos.x = leftBorder - buffer;
        }

        transform.position = currentPos;
    }



    private void OnTriggerEnter2D(Collider2D collision)

    {

        if (collision.tag == "Platform")
        {

            Jump(1);
        }
    }



    public void Jump(float x)

    {

        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        GetComponent<Rigidbody2D>().AddForce(new Vector2(0, 12 * x), ForceMode2D.Impulse);

    }

}