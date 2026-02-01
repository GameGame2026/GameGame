// ====================================================================================================
// INPUT SYSTEM 重构示例
// ====================================================================================================
// 这个文件展示了如何使用新的 C# 事件风格来处理输入，而不是使用 PlayerInput 组件的回调方式
// ====================================================================================================

using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace GamePlay.Controller.Examples
{
    /// <summary>
    /// 示例：展示如何实现带触发器的输入参数类
    /// 类似您提供的例子中的 _param.AttackTrigger.Set()
    /// </summary>
    public class InputTrigger
    {
        private bool _triggered;

        public void Set()
        {
            _triggered = true;
        }

        public bool Check()
        {
            if (_triggered)
            {
                _triggered = false;
                return true;
            }
            return false;
        }

        public void Reset()
        {
            _triggered = false;
        }
    }

    /// <summary>
    /// 示例：输入参数类，包含所有输入值
    /// </summary>
    public class PlayerInputParams
    {
        public Vector2 MoveInput;
        public Vector2 LookInput;
        public bool JumpInput;
        public bool SprintInput;
        public InputTrigger AttackTrigger = new InputTrigger();
    }

    /// <summary>
    /// 示例控制器 - 展示新的输入系统使用方式
    /// </summary>
    public class ExamplePlayerController : MonoBehaviour
    {
#if ENABLE_INPUT_SYSTEM
        private PlayerInputSystem _input;
        private PlayerInputParams _param;

        private void Awake()
        {
            _input = new PlayerInputSystem();
            _param = new PlayerInputParams();
        }

        private void OnEnable()
        {
            // 方式 1: 使用 lambda 表达式直接赋值（推荐用于简单的值传递）
            _input.GamePlay.Move.performed += ctx => _param.MoveInput = ctx.ReadValue<Vector2>();
            _input.GamePlay.Move.canceled += _ => _param.MoveInput = Vector2.zero;

            // 方式 2: 使用命名方法（推荐用于复杂逻辑或需要取消订阅的情况）
            _input.GamePlay.Look.performed += OnLookPerformed;
            _input.GamePlay.Look.canceled += OnLookCanceled;

            // 方式 3: 触发器模式（推荐用于一次性动作，如攻击）
            // _input.GamePlay.Attack.performed += OnAttackPerformed;

            _input.GamePlay.Enable();
        }

        private void OnDisable()
        {
            // 取消订阅 - 注意：lambda 表达式需要使用相同的实例才能取消订阅
            // 因此对于需要取消订阅的情况，建议使用命名方法
            _input.GamePlay.Move.performed -= ctx => _param.MoveInput = ctx.ReadValue<Vector2>();
            _input.GamePlay.Move.canceled -= _ => _param.MoveInput = Vector2.zero;

            _input.GamePlay.Look.performed -= OnLookPerformed;
            _input.GamePlay.Look.canceled -= OnLookCanceled;

            // _input.GamePlay.Attack.performed -= OnAttackPerformed;

            _input.GamePlay.Disable();
        }

        // 命名方法示例
        private void OnLookPerformed(InputAction.CallbackContext ctx)
        {
            _param.LookInput = ctx.ReadValue<Vector2>();
        }

        private void OnLookCanceled(InputAction.CallbackContext ctx)
        {
            _param.LookInput = Vector2.zero;
        }

        // 触发器模式示例
        private void OnAttackPerformed(InputAction.CallbackContext ctx)
        {
            _param.AttackTrigger.Set();
        }

        private void Update()
        {
            // 使用输入
            if (_param.MoveInput != Vector2.zero)
            {
                Debug.Log($"Moving: {_param.MoveInput}");
            }

            // 检查触发器
            if (_param.AttackTrigger.Check())
            {
                Debug.Log("Attack triggered!");
                // 执行攻击逻辑
            }
        }
#endif
    }

    // ====================================================================================================
    // 最佳实践总结：
    // ====================================================================================================
    // 1. 简单值赋值：使用 lambda 表达式
    //    _input.GamePlay.Move.performed += ctx => _param.MoveInput = ctx.ReadValue<Vector2>();
    //
    // 2. 复杂逻辑：使用命名方法
    //    _input.GamePlay.Look.performed += OnLookPerformed;
    //
    // 3. 一次性动作：使用触发器模式
    //    _input.GamePlay.Attack.performed += OnAttackPerformed;
    //    private void OnAttackPerformed(InputAction.CallbackContext ctx) { _param.AttackTrigger.Set(); }
    //
    // 4. 记得在 OnDisable 中取消订阅所有事件
    //
    // 5. 对于 lambda 表达式，如果需要正确取消订阅，必须使用命名方法
    // ====================================================================================================
}

