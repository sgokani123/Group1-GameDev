using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Ajust : MonoBehaviour
{
    [Tooltip("If true, background stays centered on the camera.")]
    public bool followCamera = true;

    private SpriteRenderer sr;
    private Camera cam;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        cam = Camera.main;
    }

    void Start()
    {
        Apply();
    }

    void LateUpdate()
    {
        if (followCamera && cam != null)
        {
            transform.position = new Vector3(
                cam.transform.position.x,
                cam.transform.position.y,
                transform.position.z
            );
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            sr = GetComponent<SpriteRenderer>();
            cam = Camera.main;
            Apply();
        }
    }
#endif

    void Apply()
    {
        if (sr == null || cam == null || sr.sprite == null) return;

        // Reset scale so it doesn't compound
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity;

        // Camera world size
        float camHeight = cam.orthographicSize * 2f;
        float camWidth  = camHeight * cam.aspect;

        // Sprite world size at scale = 1
        float spriteWidth  = sr.bounds.size.x;
        float spriteHeight = sr.bounds.size.y;

        if (spriteWidth <= 0f || spriteHeight <= 0f) return;

        // Stretch independently on X and Y (NO aspect preservation)
        float scaleX = camWidth  / spriteWidth;
        float scaleY = camHeight / spriteHeight;

        transform.localScale = new Vector3(scaleX, scaleY, 1f);
    }
}