using UnityEngine;
using System.Collections.Generic;
using _Projects.GamePlay;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace GamePlay.Controller
{
    [RequireComponent(typeof(CharacterController))]
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("配置文件")]
        [Tooltip("玩家控制器配置数据")]
        public PlayerControllerConfig config;

        [Header("运行时状态")]
        [Tooltip("角色是否在地面上. 不是 CharacterController 内置的接地检测的一部分")]
        public bool Grounded = true;

        [Header("Cinemachine")]
        [Tooltip("Cinemachine 虚拟摄像机中设置的跟随目标，摄像机将跟随该目标")]
        public GameObject CinemachineCameraTarget;

        [Header("检测器")]
        [Tooltip("玩家检测器（手动指定或自动查找）")]
        public PlayerProximityDetector proximityDetector;

        // 玩家相关
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // 超时计时
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // 动画参数 ID（使用Bool和Float控制，避免Trigger导致的卡顿）
        private int _animIDSpeed;        // Float: 移动速度（0=Idle, 1=Walk, 2=Run）
        private int _animIDGrounded;     // Bool: 是否在地面
        private int _animIDJump;         // Trigger: 跳跃触发（仅在起跳时触发一次）
        private int _animIDFacingAway;   // Bool: 是否背对摄像机（按W时true）
        private int _animIDPoints;       // Int: 点数（0-3），用于切换不同动画集
        
        private PlayerInputHandler _inputHandler;
        private PlayerStats _playerStats;  // 玩家状态（包含点数）
        private Animator _animator;
        private CharacterController _controller;
        private GameObject _mainCamera;
        private FaceCamera _faceCamera;  // FaceCamera组件引用

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        // 交互系统
        private List<DisposableObject> _disposedObjects = new List<DisposableObject>();
        private InteractableNPC _currentNPC;
        private bool _disposeInputPressed;
        private bool _recycleInputPressed;



        private void Awake()
        {
            // 获取主相机的引用
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }

            // 获取或查找检测器
            if (proximityDetector == null)
            {
                proximityDetector = GetComponentInChildren<PlayerProximityDetector>();
                if (proximityDetector == null)
                {
                    Debug.LogWarning("[ThirdPersonController] 未找到 PlayerProximityDetector，请手动添加并配置。", this);
                }
            }
        }

        private void Start()
        {
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _inputHandler = GetComponent<PlayerInputHandler>();
            _playerStats = GetComponent<PlayerStats>();
            _faceCamera = GetComponentInChildren<FaceCamera>();
            
            // 订阅点数变化事件
            if (_playerStats != null)
            {
                _playerStats.OnPointsChanged.AddListener(OnPointsChanged);
                // 初始化时设置一次点数
                OnPointsChanged(_playerStats.Points);
            }

            AssignAnimationIDs();

            // 启动时重置超时计时器
            _jumpTimeoutDelta = config.jumpTimeout;
            _fallTimeoutDelta = config.fallTimeout;
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity();
            GroundedCheck();
            if (DialogueUI.Instance != null && DialogueUI.Instance.IsDialogueActive == false)
            {
                Move();
                Interact();
                Dispose();
                Recycle();
            }
            else
            {
                _speed = 0f;
                _animationBlend = Mathf.Lerp(_animationBlend, 0f, Time.deltaTime * config.speedChangeRate);
                
                if (_hasAnimator)
                {
                    _animator.SetFloat(_animIDSpeed, 0f);
                }
            }
            
        }
        
        private void AssignAnimationIDs()
        {
            // Speed: 0 = Idle, 0-1 = Walk, 1-2 = Run
            _animIDSpeed = Animator.StringToHash("Speed");
            // Grounded: true = 在地面, false = 在空中（Jump动画）
            _animIDGrounded = Animator.StringToHash("Grounded");
            // Jump: 仅在起跳瞬间触发
            _animIDJump = Animator.StringToHash("Jump");
            // FacingAway: true = 背对摄像机（按W），false = 面对摄像机（按S或左右）
            _animIDFacingAway = Animator.StringToHash("FacingAway");
            // Points: 0-3，控制使用哪套动画集
            _animIDPoints = Animator.StringToHash("Points");
        }

        private void GroundedCheck()
        {
            // 计算球形检测位置，包含偏移
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - config.groundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, config.groundedRadius, config.groundLayers,
                QueryTriggerInteraction.Ignore);
            
            // 更新动画器的Grounded参数
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }
        

        private void Move()
        {
            // 根据行走/冲刺输入设置目标速度
            float targetSpeed = _inputHandler.SprintInput ? config.sprintSpeed : config.moveSpeed;

            // 如果没有输入，则目标速度为 0
            if (_inputHandler.MoveInput == Vector2.zero) targetSpeed = 0.0f;

            // 玩家当前水平速度的引用
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _inputHandler.analogMovement ? _inputHandler.MoveInput.magnitude : 1f;

            // 加速或减速到目标速度
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * config.speedChangeRate);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * config.speedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // 规范化输入方向
            Vector3 inputDirection = new Vector3(_inputHandler.MoveInput.x, 0.0f, _inputHandler.MoveInput.y).normalized;

            // 计算相对于摄像机的移动方向
            if (_inputHandler.MoveInput != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // 移动玩家
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // 更新2D动画状态
            UpdateAnimation();
        }

        private void UpdateAnimation()
        {
            if (!_hasAnimator) return;

            // 计算动画速度值
            // Speed: 0 = Idle, 1 = Walk, 2 = Run
            float animSpeed = 0f;
            
            // 只有在地面上才根据移动状态设置速度
            if (Grounded)
            {
                if (_inputHandler.MoveInput != Vector2.zero)
                {
                    // 按住Shift并且移动 -> Run (Speed = 2)
                    if (_inputHandler.SprintInput)
                    {
                        animSpeed = 2f;
                    }
                    // 移动但没按Shift -> Walk (Speed = 1)
                    else
                    {
                        animSpeed = 1f;
                    }
                }
                // 没有移动输入 -> Idle (Speed = 0)
                else
                {
                    animSpeed = 0f;
                }
            }
            
            // 更新动画参数
            _animator.SetFloat(_animIDSpeed, animSpeed);
            
            // 从FaceCamera获取朝向状态
            if (_faceCamera != null)
            {
                _animator.SetBool(_animIDFacingAway, _faceCamera.IsFacingAway);
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // 重置下落超时计时器
                _fallTimeoutDelta = config.fallTimeout;

                // 当位于地面时，防止速度无限下落
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // 跳跃
                if (_inputHandler.JumpInput && _jumpTimeoutDelta <= 0.0f)
                {
                    // H * -2 * G 的平方根 = 达到期望高度所需的初速度
                    _verticalVelocity = Mathf.Sqrt(config.jumpHeight * -2f * config.gravity);
                    
                    // 触发跳跃动画（只触发一次）
                    if (_hasAnimator)
                    {
                        _animator.SetTrigger(_animIDJump);
                    }
                }

                // 跳跃冷却
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // 重置跳跃超时计时器
                _jumpTimeoutDelta = config.jumpTimeout;

                // 下落冷却
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }

                // 如果不在地面上，则禁止跳跃
                _inputHandler.ResetJumpInput();
            }

            // 如果未达到终端速度则应用重力（乘以 deltaTime 两次以实现线性加速）
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += config.gravity * Time.deltaTime;
            }
        }
        
        private void Interact()
        {
            // 查找范围内可互动的NPC
            InteractableNPC closestNPC = FindClosestInteractable();
            
            // 更新当前NPC的UI显示
            if (closestNPC != _currentNPC)
            {
                // 隐藏之前NPC的UI
                if (_currentNPC != null)
                {
                    _currentNPC.HideInteractUI();
                }
                
                _currentNPC = closestNPC;
                
                // 显示新NPC的UI
                if (_currentNPC != null)
                {
                    _currentNPC.ShowInteractUI();
                }
            }
            
            // 按下交互键时触发对话
            if (_inputHandler.InteractInput && _currentNPC != null)
            {
                _currentNPC.TriggerAction();
            }
        }

        private InteractableNPC FindClosestInteractable()
        {
            if (proximityDetector != null)
            {
                return proximityDetector.GetInteractable();
            }
            
            Debug.LogWarning("[ThirdPersonController] proximityDetector 未设置", this);
            return null;
        }

        private void Dispose()
        {
            // 检测按键按下（避免连续触发）
            if (_inputHandler.DisposeInput && !_disposeInputPressed)
            {
                _disposeInputPressed = true;
                
                // 查找最近的可贴附物体
                DisposableObject closestDisposable = FindClosestDisposable();
                
                if (closestDisposable != null && !closestDisposable.IsAttached)
                {
                    Debug.Log($"[Dispose] 贴附物体: {closestDisposable.gameObject.name}");
                    
                    // 贴上prefab，改变物体状态
                    closestDisposable.ChangeState();
                    
                    if (closestDisposable.IsAttached && !_disposedObjects.Contains(closestDisposable))
                    {
                        _disposedObjects.Add(closestDisposable);
                    }
                   
                }
            }
            else if (!_inputHandler.DisposeInput)
            {
                _disposeInputPressed = false;
            }
        }

        private DisposableObject FindClosestDisposable()
        {
            if (proximityDetector != null)
            {
                var result = proximityDetector.GetDisposable();
                Debug.Log($"[ThirdPersonController] FindClosestDisposable 返回: {(result != null ? result.name : "null")}", this);
                return result;
            }
            
            Debug.LogWarning("[ThirdPersonController] proximityDetector 未设置", this);
            return null;
        }

        private void Recycle()
        {
            // 检测按键按下（避免连续触发）
            if (_inputHandler.RecycleInput && !_recycleInputPressed)
            {
                _recycleInputPressed = true;
                
                // 按照放置次序回收（从列表末尾开始，即最后贴附的）
                if (_disposedObjects.Count > 0)
                {
                    DisposableObject lastDisposed = _disposedObjects[_disposedObjects.Count - 1];
                    
                    Debug.Log($"[Recycle] 回收物体: {lastDisposed.gameObject.name}");
                    
                    // 回收prefab，恢复物体状态
                    lastDisposed.Recycle();
                    
                    // 从列表中移除
                    _disposedObjects.RemoveAt(_disposedObjects.Count - 1);
                }
                else
                {
                    Debug.Log("[Recycle] 没有可回收的物体");
                }
            }
            else if (!_inputHandler.RecycleInput)
            {
                _recycleInputPressed = false;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            if (config == null) return;

            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // 选中时，在与着陆检测半径相同的位置绘制 Gizmo
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - config.groundedOffset, transform.position.z),
                config.groundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (config.footstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, config.footstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(config.footstepAudioClips[index], transform.TransformPoint(_controller.center), config.footstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(config.landingAudioClip, transform.TransformPoint(_controller.center), config.footstepAudioVolume);
            }
        }
        
        private void OnDestroy()
        {
            // 取消订阅点数变化事件，防止内存泄漏
            if (_playerStats != null)
            {
                _playerStats.OnPointsChanged.RemoveListener(OnPointsChanged);
            }
        }
        
        /// <summary>
        /// 点数变化时的回调，更新Animator参数
        /// </summary>
        private void OnPointsChanged(int newPoints)
        {
            if (_hasAnimator)
            {
                _animator.SetInteger(_animIDPoints, newPoints);
                Debug.Log($"[ThirdPersonController] 动画点数更新: {newPoints}");
            }
        }
    }
}
