using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    /// <summary>
    /// Z轴跟随模式
    /// </summary>
    public enum ZAxisMode
    {
        FixedFollow,    // 固定跟随玩家Z轴
        DepthScale      // 根据玩家Z轴进行深度缩放
    }
    
    [Header("目标设置")]
    [SerializeField] private Transform target; // 玩家目标
    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 5f, -10f); // 相机相对玩家的偏移
    
    [Header("移动边界")]
    [SerializeField] private bool useBounds = true;
    [SerializeField] private Vector3 boundsMin = new Vector3(-50f, 0f, -50f); // 边界最小值
    [SerializeField] private Vector3 boundsMax = new Vector3(50f, 20f, 50f);  // 边界最大值
    
    [Header("Z轴模式")]
    [SerializeField] private ZAxisMode zAxisMode = ZAxisMode.FixedFollow;
    [SerializeField] private float depthScaleFactor = 0.5f; // 深度缩放系数（仅在DepthScale模式下使用）
    [SerializeField] private float baseDepth = 0f; // 基准深度位置
    
    [Header("平滑设置")]
    [SerializeField] private float positionSmooth = 10f;
    
    [Header("碰撞检测")]
    [SerializeField] private bool enableCollision = true;
    [SerializeField] private float collisionOffset = 0.3f;
    [SerializeField] private LayerMask collisionLayers = -1;
    
    // 组件引用
    private PlayerInputSystem _inputSystem;
    private Camera _camera;
    
    // 摄像机状态
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
    }
    
    private void Start()
    {
        // 初始化摄像机位置
        if (target != null)
        {
            _smoothPosition = transform.position;
        }
    }
    
    private void LateUpdate()
    {
        if (target == null) return;
        
        HandleCameraPosition();
    }
    
    /// <summary>
    /// 处理摄像机位置
    /// </summary>
    private void HandleCameraPosition()
    {
        Vector3 desiredPosition = CalculateDesiredPosition();
        
        // 应用边界限制
        if (useBounds)
        {
            desiredPosition = ApplyBounds(desiredPosition);
        }
        
        // 碰撞检测
        if (enableCollision)
        {
            Vector3 targetPosition = target.position;
            Vector3 direction = desiredPosition - targetPosition;
            if (Physics.Raycast(targetPosition, direction.normalized, out RaycastHit hit, 
                    direction.magnitude, collisionLayers, QueryTriggerInteraction.Ignore))
            {
                desiredPosition = hit.point - direction.normalized * collisionOffset;
            }
        }
        
        // 平滑移动
        _smoothPosition = Vector3.Lerp(_smoothPosition, desiredPosition, positionSmooth * Time.deltaTime);
        transform.position = _smoothPosition;
    }
    
    /// <summary>
    /// 计算期望的摄像机位置（基于Z轴模式）
    /// </summary>
    private Vector3 CalculateDesiredPosition()
    {
        Vector3 desiredPosition = target.position + cameraOffset;
        
        if (zAxisMode == ZAxisMode.DepthScale)
        {
            // 深度缩放模式：根据玩家Z轴位置调整摄像机Z轴偏移
            float playerZOffset = target.position.z - baseDepth;
            desiredPosition.z = target.position.z + cameraOffset.z + (playerZOffset * depthScaleFactor);
        }
        // FixedFollow模式下直接使用 target.position + cameraOffset
        
        return desiredPosition;
    }
    
    /// <summary>
    /// 应用边界限制
    /// </summary>
    private Vector3 ApplyBounds(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, boundsMin.x, boundsMax.x);
        position.y = Mathf.Clamp(position.y, boundsMin.y, boundsMax.y);
        position.z = Mathf.Clamp(position.z, boundsMin.z, boundsMax.z);
        return position;
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
    /// 设置移动边界
    /// </summary>
    public void SetBounds(Vector3 min, Vector3 max)
    {
        boundsMin = min;
        boundsMax = max;
    }
    
    /// <summary>
    /// 启用/禁用边界限制
    /// </summary>
    public void SetUseBounds(bool use)
    {
        useBounds = use;
    }
    
    /// <summary>
    /// 设置Z轴跟随模式为固定跟随
    /// </summary>
    public void SetFixedFollowMode()
    {
        zAxisMode = ZAxisMode.FixedFollow;
    }
    
    /// <summary>
    /// 设置Z轴跟随模式为深度缩放
    /// </summary>
    /// <param name="scaleFactor">深度缩放系数</param>
    /// <param name="baseDepthPosition">基准深度位置</param>
    public void SetDepthScaleMode(float scaleFactor = 0.5f, float baseDepthPosition = 0f)
    {
        zAxisMode = ZAxisMode.DepthScale;
        depthScaleFactor = scaleFactor;
        baseDepth = baseDepthPosition;
    }
    
    /// <summary>
    /// 设置摄像机偏移
    /// </summary>
    public void SetCameraOffset(Vector3 offset)
    {
        cameraOffset = offset;
    }
    
    // 公开属性
    public Transform Target => target;
    public ZAxisMode CurrentZAxisMode => zAxisMode;
    public Vector3 BoundsMin => boundsMin;
    public Vector3 BoundsMax => boundsMax;
}

