using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Camera targetCamera;
    public bool lockYAxis = false; // 可选：锁定Y轴旋转
    public bool lockXAxis = false; // 可选：锁定X轴旋转

    void Update()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        Vector3 direction = targetCamera.transform.position - transform.position;
        
        if (lockYAxis)
            direction.y = 0; // 保持Y轴与世界一致（如地面上的标志）
        if (lockXAxis)
        {
            direction.x = 0;
        }

        transform.rotation = Quaternion.LookRotation(-direction);
    }
}