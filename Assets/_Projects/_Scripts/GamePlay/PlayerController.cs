using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputSystem))]
[RequireComponent(typeof(PlayerPhysicsDetector))]
public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 10f;
    
    [Header("摄像机设置")]
    [SerializeField] private Transform cameraTransform; // 摄像机Transform，用于基于摄像机方向移动
    
    [Header("跳跃设置")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -15f;
    [SerializeField] private float coyoteTime = 0.2f; // 土狼时间：离开地面后还能跳跃的时间
    
    [Header("旋转设置")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private bool rotateTowardsMovement = true; // 是否朝向移动方向旋转
    
    [Header("耐力集成（可选）")]
    [SerializeField] private bool useStaminaForSprint; // 是否使用耐力系统
    
    [Header("调试信息")]
    [SerializeField] private bool showDebugInfo = true;
    
    // 组件引用
    private CharacterController _controller;
    private PlayerInputSystem _inputSystem;
    private PlayerStats _stats;
    private Camera _mainCamera; // 主摄像机引用
    private PlayerPhysicsDetector _physicsDetector; // 物理检测器
    
    // 运动状态
    private Vector3 _velocity;
    private Vector3 _currentSpeed;
    
    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _inputSystem = GetComponent<PlayerInputSystem>();
        _stats = GetComponent<PlayerStats>();
        _physicsDetector = GetComponent<PlayerPhysicsDetector>(); // 物理检测器
        
        // 如果没��指定摄像机Transform，尝试获取主摄像机
        if (cameraTransform == null)
        {
            _mainCamera = Camera.main;
            if (_mainCamera != null)
            {
                cameraTransform = _mainCamera.transform;
            }
        }
    }

    private void Update()
    {
        // 使用物理检测器检测地面
        bool isGrounded = _physicsDetector.CheckGround();
        
        HandleMovement();
        HandleJump(isGrounded);
        HandleGravity(isGrounded);
        ApplyMovement(isGrounded);

        if (isGrounded)
        {
            Debug.Log("Ground");   
        }
        
        if (showDebugInfo)
            DrawDebugInfo();
    }
    
    /// <summary>
    /// 处理角色移动
    /// </summary>
    private void HandleMovement()
    {
        Vector2 input = _inputSystem.MoveDirection;
        
        // 基于摄像机方向计算移动方向
        Vector3 moveDirection;
        
        if (cameraTransform != null)
        {
            // 获取摄像机的前方和右方向（忽略Y轴，只在水平面移动）
            Vector3 cameraForward = cameraTransform.forward;
            cameraForward.y = 0;
            cameraForward.Normalize();
            
            Vector3 cameraRight = cameraTransform.right;
            cameraRight.y = 0;
            cameraRight.Normalize();
            
            // W/S 控制沿着摄像机前后方向，A/D 控制左右方向
            moveDirection = cameraForward * input.y + cameraRight * input.x;
        }
        else
        {
            // 如果没有摄像机，使用世界坐标系
            moveDirection = new Vector3(input.x, 0, input.y);
        }
        
        // 获取目标速度 - 集成耐力系统
        bool canSprint = !useStaminaForSprint || (_stats != null && _stats.HasStamina);
        bool isSprinting = _inputSystem.SprintPressed && canSprint && moveDirection.magnitude > 0.1f;
        
        // 如果正在冲刺且使用耐力系统，消耗耐力
        if (isSprinting && useStaminaForSprint && _stats != null)
        {
            if (!_stats.DrainStamina(Time.deltaTime))
            {
                isSprinting = false; // 耐力不足，停止冲刺
            }
        }
        
        float targetSpeed = isSprinting ? sprintSpeed : walkSpeed;
        Vector3 targetVelocity = moveDirection.normalized * targetSpeed;
        
        // 平滑加速/减速
        float accelerationRate = moveDirection.magnitude > 0.1f ? acceleration : deceleration;
        _currentSpeed = Vector3.Lerp(_currentSpeed, targetVelocity, accelerationRate * Time.deltaTime);
        
        // 处理角色旋转
        if (rotateTowardsMovement && moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    /// <summary>
    /// 处理跳跃逻辑
    /// </summary>
    private void HandleJump(bool isGrounded)
    {
        // 土狼时间：刚离开地面后的短时间内仍可跳跃
        bool canJump = Time.time - _physicsDetector.LastGroundedTime < coyoteTime;
        
        if (_inputSystem.JumpPressed && canJump)
        {
            // 计算跳跃初速度: v = sqrt(2 * jumpHeight * gravity)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            _inputSystem.ConsumeJumpInput();
        }
        
        // 当角色在地面时，立即停止向下的速度，防止积累
        if (isGrounded && _velocity.y < 0)
        {
            _velocity.y = 0f;
        }
    }
    
    /// <summary>
    /// 处理重力
    /// </summary>
    private void HandleGravity(bool isGrounded)
    {
        if (!isGrounded)
        {
            _velocity.y += gravity * Time.deltaTime;
        }
    }
    
    /// <summary>
    /// 应用移动到角色控制器
    /// </summary>
    private void ApplyMovement(bool isGrounded)
    {
        Vector3 motion = _currentSpeed;
        
        // 只在非地面状态或跳跃时应用垂直速度
        if (!isGrounded || _velocity.y > 0)
        {
            motion += _velocity;
        }
        else
        {
            // 在地面时应用微小的向下力，确保贴地
            motion.y = -0.5f;
        }
        
        // 尝试台阶攀爬
        if (isGrounded && _velocity.y <= 0 && _currentSpeed.magnitude > 0.1f)
        {
            Vector3 horizontalMotion = new Vector3(_currentSpeed.x, 0, _currentSpeed.z);
            if (_physicsDetector.TryDetectStep(horizontalMotion, out float stepUpDistance, _velocity.y))
            {
                // stepUpDistance 是距离，转换为速度（距离/时间）
                float stepUpVelocity = stepUpDistance / Time.deltaTime;
                motion.y = Mathf.Max(motion.y, stepUpVelocity);
            }
        }
        
        _controller.Move(motion * Time.deltaTime);
    }
    
    /// <summary>
    /// 绘制调试信息
    /// </summary>
    private void DrawDebugInfo()
    {
        // 移动方向可视化
        if (_currentSpeed.magnitude > 0.1f)
        {
            Debug.DrawRay(transform.position + Vector3.up, _currentSpeed.normalized * 2f, Color.blue);
        }
        
        // 速度可视化
        if (_velocity.magnitude > 0.1f)
        {
            Debug.DrawRay(transform.position + Vector3.up * 1.5f, _velocity.normalized, Color.magenta);
        }
    }
    
    // 公开属性供其他脚本访问
    public bool IsGrounded => _physicsDetector != null && _physicsDetector.IsGrounded;
    public Vector3 Velocity => _velocity;
    public Vector3 CurrentSpeed => _currentSpeed;
    public PlayerPhysicsDetector PhysicsDetector => _physicsDetector;
}

