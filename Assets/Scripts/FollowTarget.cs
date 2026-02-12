using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    public Transform target; // 拖入你的 Player
    public float smoothSpeed = 0.125f;

    void LateUpdate()
    {
        // 如果玩家向上跳过了摄像机当前高度，摄像机就跟上去
        if (target != null && target.position.y > transform.position.y)
        {
            Vector3 newPos = new Vector3(transform.position.x, target.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, newPos, smoothSpeed);
        }
    }
}