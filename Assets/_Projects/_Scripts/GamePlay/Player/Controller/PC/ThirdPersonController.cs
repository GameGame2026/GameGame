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

        // 动画参数 ID
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;
        
        private PlayerInputHandler _inputHandler;
        private Animator _animator;
        private CharacterController _controller;
        private GameObject _mainCamera;

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
                    _animator.SetFloat(_animIDSpeed, _animationBlend);
                    _animator.SetFloat(_animIDMotionSpeed, 0f);
                }
            }
            
        }
        
        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            // 计算球形检测位置，包含偏移
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - config.groundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, config.groundedRadius, config.groundLayers,
                QueryTriggerInteraction.Ignore);

            // 如果有动画器则更新参数
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }
        

        private void Move()
        {
            // 根据行走/冲刺输入设置目标速度
            float targetSpeed = _inputHandler.SprintInput ? config.sprintSpeed : config.moveSpeed;

            // 一个简单的加速与减速实现，便于替换或调整

            // 注意：Vector2 的 == 运算符使用近似比较，避免浮点误差问题，且比 magnitude 更省性能
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
                // 使用 Lerp 产生更平滑的速度变化（非线性）
                // 注意 Lerp 的 t 值会被限制到 [0,1]
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * config.speedChangeRate);

                // 将速度四舍五入到小数点后三位
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

            // 注意：Vector2 的 != 运算符使用近似比较，避免浮点误差问题，且比 magnitude 更省性能
            // 如果有移动输入，则在移动时旋转玩家
            if (_inputHandler.MoveInput != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    config.rotationSmoothTime);

                // 相对于摄像机方向旋转，使玩家面向移动方向
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }


            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // 移动玩家
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // 如果有动画器则更新动画参数
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // 重置下落超时计时器
                _fallTimeoutDelta = config.fallTimeout;

                // 如果有动画器则更新参数
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

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

                    // 如果有动画器则设置跳跃状态
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
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
                else
                {
                    // 如果有动画器则设置自由下落状态
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
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
                Debug.Log("Dispose pressed");
                _disposeInputPressed = true;
                
                // 查找最近的可贴附物体
                DisposableObject closestDisposable = FindClosestDisposable();
                
                if (closestDisposable != null)
                {
                    Debug.Log("Found disposable: " + closestDisposable.gameObject.name);
                    // 贴上prefab，改变物体状态
                    closestDisposable.ChangeState();
                    
                    // 添加到已贴附列表（按放置顺序）
                    if (!_disposedObjects.Contains(closestDisposable))
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
                    
                    // 回收prefab，恢复物体状态
                    lastDisposed.Recycle();
                    _disposedObjects.RemoveAt(_disposedObjects.Count - 1);
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
    }
}
