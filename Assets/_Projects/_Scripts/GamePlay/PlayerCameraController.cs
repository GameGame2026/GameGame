using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 简单的第三人称/第一人称摄像机控制器
/// </summary>
public class PlayerCameraController : MonoBehaviour
{
    [Header("目标设置")]
    [SerializeField] private Transform target; // 玩家目标
    [SerializeField] private Vector3 targetOffset = new Vector3(0, 1.5f, 0); // 相机注视点偏移
    
    [Header("摄像机设置")]
    [SerializeField] private float distance = 5f;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 10f;
    
    [Header("旋转设置")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float minVerticalAngle = -40f;
    [SerializeField] private float maxVerticalAngle = 80f;
    [SerializeField] private bool invertY = false;
    
    [Header("平滑设置")]
    [SerializeField] private float rotationSmooth = 5f;
    [SerializeField] private float positionSmooth = 10f;
    
    [Header("碰撞检测")]
    [SerializeField] private bool enableCollision = true;
    [SerializeField] private float collisionOffset = 0.3f;
    [SerializeField] private LayerMask collisionLayers = -1;
    
    // 组件引用
    private PlayerInputSystem _inputSystem;
    private Camera _camera;
    
    // 摄像机状态
    private float _currentYaw;
    private float _currentPitch;
    private float _currentDistance;
    private Vector3 _smoothPosition;
    
    private void Awake()
    {
        _camera = GetComponent<Camera>();
        if (_camera == null)
            _camera = Camera.main;
        
        if (target != null)
        {
            _inputSystem = target.GetComponent<PlayerInputSystem>();
        }
        
        _currentDistance = distance;
    }
    
    private void Start()
    {
        // 初始化摄像机旋转
        if (target != null)
        {
            Vector3 angles = transform.eulerAngles;
            _currentYaw = angles.y;
            _currentPitch = angles.x;
        }
        
        // 锁定鼠标
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }
    
    private void LateUpdate()
    {
        if (target == null) return;
        
        HandleCameraRotation();
        HandleCameraPosition();
    }
    
    /// <summary>
    /// 处理摄像机旋转
    /// </summary>
    private void HandleCameraRotation()
    {
        if (_inputSystem == null) return;
        
        Vector2 lookInput = _inputSystem.LookDirection;
        
        // 应用鼠标灵敏度
        float mouseX = lookInput.x * mouseSensitivity * 0.1f;
        float mouseY = lookInput.y * mouseSensitivity * 0.1f;
        
        // 更新旋转角度
        _currentYaw += mouseX;
        _currentPitch += (invertY ? mouseY : -mouseY);
        
        // 限制垂直角度
        _currentPitch = Mathf.Clamp(_currentPitch, minVerticalAngle, maxVerticalAngle);
    }
    
    /// <summary>
    /// 处理摄像机位置
    /// </summary>
    private void HandleCameraPosition()
    {
        Vector3 targetPosition = target.position + targetOffset;
        Quaternion targetRotation = Quaternion.Euler(_currentPitch, _currentYaw, 0);
        
        Vector3 desiredPosition;
        
        Vector3 offset = targetRotation * new Vector3(0, 0, -_currentDistance);
        desiredPosition = targetPosition + offset;
            
        // 碰撞检测
        if (enableCollision)
        {
            Vector3 direction = desiredPosition - targetPosition;
            if (Physics.Raycast(targetPosition, direction.normalized, out RaycastHit hit, 
                    direction.magnitude, collisionLayers, QueryTriggerInteraction.Ignore))
            {
                desiredPosition = hit.point - direction.normalized * collisionOffset;
            }
        }
  
        
        // 平滑移动和旋转
        _smoothPosition = Vector3.Lerp(_smoothPosition, desiredPosition, positionSmooth * Time.deltaTime);
        transform.position = _smoothPosition;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmooth * Time.deltaTime);
    }
    
    /// <summary>
    /// 设置摄像机目标
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (newTarget != null)
        {
            _inputSystem = newTarget.GetComponent<PlayerInputSystem>();
            _smoothPosition = transform.position;
        }
    }
    
    /// <summary>
    /// 设置摄像机距离（用于缩放）
    /// </summary>
    public void SetDistance(float newDistance)
    {
        _currentDistance = Mathf.Clamp(newDistance, minDistance, maxDistance);
    }
    
    
    // 公开属性
    public Transform Target => target;
    public float Yaw => _currentYaw;
    public float Pitch => _currentPitch;
}

