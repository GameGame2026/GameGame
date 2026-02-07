using UnityEngine;
using GamePlay.Controller;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class FaceCamera : MonoBehaviour
{
    private Transform _characterRoot;
    private PlayerInputHandler _inputHandler;

    // 2D角色朝向控制
    private bool _isFacingRight = true;
    private bool _isFacingAway = false; 
    private enum LastVerticalInput { None, Forward, Backward }
    private LastVerticalInput _lastVerticalInput = LastVerticalInput.None;  // 记录最后的前后输入

    private void Start()
    {
        _characterRoot = transform.parent != null ? transform.parent : transform;
        _inputHandler = GetComponentInParent<PlayerInputHandler>();

        if (_inputHandler == null)
        {
            Debug.LogWarning("[FaceCamera] 未找到 PlayerInputHandler 组件", this);
        }
    }

    void LateUpdate()
    {
        // 获取主摄像机
        Camera cam = Camera.main;
        if (cam != null)
        {
            // 使用摄像机的 forward，但在水平面上（Y=0）进行朝向，这样只改变 Y 轴旋转
            Vector3 flatForward = cam.transform.forward;
            flatForward.y = 0f;

            if (flatForward.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(flatForward);
            }
            else
            {
                // 兜底：当 forward 在水平面接近零时，直接只用摄像机的 Y 角度
                transform.rotation = Quaternion.Euler(0f, cam.transform.eulerAngles.y, 0f);
            }
            
            // 2D角色朝向控制
            if (_inputHandler != null && _inputHandler.MoveInput != Vector2.zero)
            {
                // 获取输入的前后和左右分量
                float verticalInput = _inputHandler.MoveInput.y;
                float horizontalInput = _inputHandler.MoveInput.x;

                // 判断前后方向（相对于摄像机）
                if (Mathf.Abs(verticalInput) > 0.1f)
                {
                    if (verticalInput > 0) 
                    {
                        _isFacingAway = true;
                        _lastVerticalInput = LastVerticalInput.Forward;
                    }
                    else
                    {
                        _isFacingAway = false;
                        _lastVerticalInput = LastVerticalInput.Backward;
                    }
                }
                else if (Mathf.Abs(horizontalInput) > 0.1f)
                {
                    _isFacingAway = false;
                }
                
                if (Mathf.Abs(horizontalInput) > 0.1f)
                {
                    if (horizontalInput > 0)
                    {
                        _isFacingRight = true;
                    }
                    else
                    {
                        _isFacingRight = false;
                    }
                }
            }
            else if (_inputHandler != null && _inputHandler.MoveInput == Vector2.zero)
            {
                // Idle状态下，根据最后的前后输入决定朝向
                if (_lastVerticalInput == LastVerticalInput.Forward)
                {
                    _isFacingAway = true;
                }
                else if (_lastVerticalInput == LastVerticalInput.Backward)
                {
                    _isFacingAway = false;
                }
            }
            
            Vector3 localScale = _characterRoot.localScale;
            localScale.x = _isFacingRight ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
            _characterRoot.localScale = localScale;
        }
    }

    /// <summary>
    /// 获取当前是否背对摄像机的状态
    /// </summary>
    public bool IsFacingAway => _isFacingAway;
}
