using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace GamePlay.Controller
{
    /// <summary>
    /// 玩家输入处理器 - 负责接收和管理所有玩家输入
    /// </summary>
    public class PlayerInputHandler : MonoBehaviour
    {
        // ===== 输入值（只读） =====
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool JumpInput { get; private set; }
        public bool SprintInput { get; private set; }
        public bool InteractInput { get; private set; }
        public bool DisposeInput { get; private set; }
        public bool RecycleInput { get; private set; }
        public bool AttackInput { get; private set; }

        [Header("Input Settings")]
        [Tooltip("是否启用模拟移动（根据摇杆推动距离调整速度）")]
        public bool analogMovement;

        [Tooltip("是否允许鼠标控制视角")]
        public bool cursorInputForLook = true;

        [Header("Cursor Settings")]
        [Tooltip("是否锁定光标")]
        public bool cursorLocked = true;

#if ENABLE_INPUT_SYSTEM
        private PlayerInputSystem _inputSystem;

        private void Awake()
        {
            _inputSystem = new PlayerInputSystem();
        }

        private void OnEnable()
        {
            // 订阅输入事件
            _inputSystem.GamePlay.Move.performed += OnMovePerformed;
            _inputSystem.GamePlay.Move.canceled += OnMoveCanceled;

            _inputSystem.GamePlay.Look.performed += OnLookPerformed;
            _inputSystem.GamePlay.Look.canceled += OnLookCanceled;

            _inputSystem.GamePlay.Jump.performed += OnJumpPerformed;
            _inputSystem.GamePlay.Jump.canceled += OnJumpCanceled;

            _inputSystem.GamePlay.Sprint.performed += OnSprintPerformed;
            _inputSystem.GamePlay.Sprint.canceled += OnSprintCanceled;
            
            _inputSystem.GamePlay.Interact.performed += ctx => InteractInput = true;
            _inputSystem.GamePlay.Interact.canceled += ctx => InteractInput = false;
            
            _inputSystem.GamePlay.Dispose.performed += ctx => DisposeInput = true;
            _inputSystem.GamePlay.Dispose.canceled += ctx => DisposeInput = false;
            
            _inputSystem.GamePlay.Recycle.performed += ctx => RecycleInput = true;
            _inputSystem.GamePlay.Recycle.canceled += ctx => RecycleInput = false;
            
            _inputSystem.GamePlay.Attack.performed += ctx => AttackInput = true;
            _inputSystem.GamePlay.Attack.canceled += ctx => AttackInput = false;

            _inputSystem.GamePlay.Enable();
        }

        private void OnDisable()
        {
            // 取消订阅输入事件
            _inputSystem.GamePlay.Move.performed -= OnMovePerformed;
            _inputSystem.GamePlay.Move.canceled -= OnMoveCanceled;

            _inputSystem.GamePlay.Look.performed -= OnLookPerformed;
            _inputSystem.GamePlay.Look.canceled -= OnLookCanceled;

            _inputSystem.GamePlay.Jump.performed -= OnJumpPerformed;
            _inputSystem.GamePlay.Jump.canceled -= OnJumpCanceled;

            _inputSystem.GamePlay.Sprint.performed -= OnSprintPerformed;
            _inputSystem.GamePlay.Sprint.canceled -= OnSprintCanceled;

            _inputSystem.GamePlay.Disable();
        }

        // 输入回调
        private void OnMovePerformed(InputAction.CallbackContext ctx)
        {
            MoveInput = ctx.ReadValue<Vector2>();
        }

        private void OnMoveCanceled(InputAction.CallbackContext ctx)
        {
            MoveInput = Vector2.zero;
        }

        private void OnLookPerformed(InputAction.CallbackContext ctx)
        {
            if (cursorInputForLook)
            {
                LookInput = ctx.ReadValue<Vector2>();
            }
        }

        private void OnLookCanceled(InputAction.CallbackContext ctx)
        {
            LookInput = Vector2.zero;
        }

        private void OnJumpPerformed(InputAction.CallbackContext ctx)
        {
            JumpInput = true;
        }

        private void OnJumpCanceled(InputAction.CallbackContext ctx)
        {
            JumpInput = false;
        }

        private void OnSprintPerformed(InputAction.CallbackContext ctx)
        {
            SprintInput = true;
        }

        private void OnSprintCanceled(InputAction.CallbackContext ctx)
        {
            SprintInput = false;
        }
#endif

        // 提供给虚拟控制器（UI摇杆）调用的公共方法
        public void SetMoveInput(Vector2 input)
        {
            MoveInput = input;
        }

        public void SetLookInput(Vector2 input)
        {
            LookInput = input;
        }

        public void SetJumpInput(bool input)
        {
            JumpInput = input;
        }

        public void SetSprintInput(bool input)
        {
            SprintInput = input;
        }

        public void ResetJumpInput()
        {
            JumpInput = false;
        }

        public void ResetAttackInput()
        {
            AttackInput = false;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(cursorLocked);
        }

        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }
}

