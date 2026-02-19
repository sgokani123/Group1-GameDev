using UnityEngine;

public class Ajust : MonoBehaviour
{
    // 
    private void Start()
    {
        Resize();     
    }

    // 
    void  Resize()
    {
        float width = GetComponent<SpriteRenderer>().bounds.size.x;
        float targetWidth = Camera.main.orthographicSize * 2 / Screen.width * Screen.height;
        Vector3 scale = transform.localScale;
        scale.x = targetWidth / width;
        transform.localScale = scale;
    }
}
