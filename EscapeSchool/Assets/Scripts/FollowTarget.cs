using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    public Transform target;      // Assign the Player in the Inspector
    public float yOffset = 3f;    // How far above the player the camera sits
    public float floorY  = -8f;   // Set this to platform_0's Y position in the Inspector

    private float highestY;
    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        highestY = GetClampedY(transform.position.y);
    }

    void Start()
    {
        highestY = GetClampedY(transform.position.y);
    }

    void LateUpdate()
    {
        if (target == null) return;

        float targetY = target.position.y + yOffset;

        if (targetY > highestY)
            highestY = targetY;

        // Never let the bottom of the camera dip below floorY
        highestY = GetClampedY(highestY);

        transform.position = new Vector3(transform.position.x, highestY, transform.position.z);
    }

    // Ensures the camera's bottom edge never goes below floorY
    float GetClampedY(float y)
    {
        float orthoSize = cam != null ? cam.orthographicSize : 9.3f;
        float minCamY   = floorY + orthoSize;
        return Mathf.Max(y, minCamY);
    }

    public void ResetCamera(float startY)
    {
        if (cam == null) cam = GetComponent<Camera>();
        highestY = GetClampedY(startY + yOffset);
        transform.position = new Vector3(transform.position.x, highestY, transform.position.z);
    }
}