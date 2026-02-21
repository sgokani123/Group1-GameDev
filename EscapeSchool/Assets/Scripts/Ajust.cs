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
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null || Camera.main == null) return;
        float width = sr.bounds.size.x;
        float targetWidth = Camera.main.orthographicSize * 2f * Camera.main.aspect;
        Vector3 scale = transform.localScale;
        scale.x *= targetWidth / width;
        transform.localScale = scale;
    }
}
