using UnityEngine;

/// <summary>
/// 玩家物理检测器 - 专门负责玩家的物理检测逻辑
/// 职责：
/// 1. 地面检测
/// 2. 台阶检测
/// 3. 斜坡检测
/// 4. 提供检测结果给PlayerController使用
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerPhysicsDetector : MonoBehaviour
{
    [Header("地面检测设置")]
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer = -1;
    [SerializeField] private Vector3 groundCheckOffset = new Vector3(0, 0.1f, 0);
    
    [Header("台阶检测设置")]
    [SerializeField] private bool enableStepDetection = true;
    [SerializeField] private float stepHeight = 0.3f;
    [SerializeField] private float stepSmooth = 0.1f;
    
    [Header("调试设置")]
    [SerializeField] private bool showDebugGizmos = true;
    
    // 组件引用
    private CharacterController _controller;
    
    // 检测结果
    private bool _isGrounded;
    private RaycastHit _groundHit;
    private float _lastGroundedTime;
    
    // 公开属性
    public bool IsGrounded => _isGrounded;
    public RaycastHit GroundHit => _groundHit;
    public float LastGroundedTime => _lastGroundedTime;
    public Vector3 GroundNormal => _isGrounded ? _groundHit.normal : Vector3.up;
    
    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }
    
    /// <summary>
    /// 检测地面
    /// </summary>
    public bool CheckGround()
    {
        Vector3 checkPosition = transform.position + groundCheckOffset;
        _isGrounded = PhysicsHelper.CheckGroundSphere(
            checkPosition,
            _controller.radius * 0.9f,
            groundCheckDistance,
            groundLayer,
            out _groundHit);
        
        if (_isGrounded)
        {
            _lastGroundedTime = Time.time;
        }
        
        return _isGrounded;
    }
    
    /// <summary>
    /// 检测台阶并计算需要的向上运动
    /// </summary>
    /// <param name="horizontalMotion">水平移动方向</param>
    /// <param name="additionalUpMotion">输出：需要添加的向上运动量</param>
    /// <returns>是否检测到可攀爬的台阶</returns>
    public bool TryDetectStep(Vector3 horizontalMotion, out float additionalUpMotion)
    {
        additionalUpMotion = 0f;
        
        if (!enableStepDetection || horizontalMotion.magnitude < 0.01f)
        {
            return false;
        }
        
        // 检测前方是否有障碍物
        Vector3 checkStart = transform.position + Vector3.up * 0.1f;
        Vector3 direction = horizontalMotion.normalized;
        
        if (!Physics.Raycast(checkStart, direction, 
            _controller.radius + 0.1f, groundLayer, QueryTriggerInteraction.Ignore))
        {
            return false; // 前方没有障碍
        }
        
        // 检测台阶顶部
        Vector3 stepCheckPos = checkStart + direction * (_controller.radius + 0.1f) + Vector3.up * stepHeight;
        
        if (!Physics.Raycast(stepCheckPos, Vector3.down, out RaycastHit stepHit, 
            stepHeight, groundLayer, QueryTriggerInteraction.Ignore))
        {
            return false; // 没有检测到台阶顶部
        }
        
        float stepUpHeight = stepHit.point.y - transform.position.y;
        
        // 如果台阶高度在有效范围内
        if (stepUpHeight > 0.01f && stepUpHeight <= stepHeight)
        {
            additionalUpMotion = stepUpHeight / Time.deltaTime * stepSmooth;
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 检测斜坡
    /// </summary>
    public bool CheckSlope(out float slopeAngle)
    {
        Vector3 checkPosition = transform.position + groundCheckOffset;
        return PhysicsHelper.CheckSlope(checkPosition, groundCheckDistance + 0.5f, groundLayer, out slopeAngle);
    }
    
    /// <summary>
    /// 获取地面检��位置（用于调试）
    /// </summary>
    public Vector3 GetGroundCheckPosition()
    {
        return transform.position + groundCheckOffset;
    }
    
    /// <summary>
    /// 绘制调试信息
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        
        if (_controller == null)
        {
            _controller = GetComponent<CharacterController>();
            if (_controller == null) return;
        }
        
        // 绘制地面检测
        Vector3 checkPosition = transform.position + groundCheckOffset;
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(checkPosition, _controller.radius * 0.9f);
        Gizmos.DrawWireSphere(checkPosition + Vector3.down * groundCheckDistance, _controller.radius * 0.9f);
        
        // 绘制地面法线
        if (_isGrounded && Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(_groundHit.point, _groundHit.normal * 0.5f);
        }
        
        // 绘制台阶检测
        if (enableStepDetection)
        {
            Gizmos.color = Color.yellow;
            Vector3 forward = transform.forward;
            Vector3 stepCheck = transform.position + Vector3.up * stepHeight + forward * (_controller.radius + 0.1f);
            Gizmos.DrawLine(stepCheck, stepCheck + Vector3.down * stepHeight);
        }
    }
}

