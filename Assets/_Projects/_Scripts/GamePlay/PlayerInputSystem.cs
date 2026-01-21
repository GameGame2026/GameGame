using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputSystem : MonoBehaviour
{
    private PlayerInput _input;
    
    // 公开访问的输入值
    public Vector2 MoveDirection { get; private set; }
    public Vector2 LookDirection { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool FirePressed { get; private set; }
    public bool SprintPressed { get; private set; } // 预留冲刺功能

    private void Awake()
    {
        _input = new PlayerInput();
        
        // 移动输入
        _input.GamePlay.Move.performed += ctx => MoveDirection = ctx.ReadValue<Vector2>();
        _input.GamePlay.Move.canceled += _ => MoveDirection = Vector2.zero;
        
        // 视角输入
        _input.GamePlay.Look.performed += ctx => LookDirection = ctx.ReadValue<Vector2>();
        _input.GamePlay.Look.canceled += _ => LookDirection = Vector2.zero;
        
        // 开火输入
        _input.GamePlay.Fire.performed += _ => FirePressed = true;
        _input.GamePlay.Fire.canceled += _ => FirePressed = false;
        
        // 跳跃输入
        _input.GamePlay.Jump.performed += _ => JumpPressed = true;
        _input.GamePlay.Jump.canceled += _ => JumpPressed = false;
        
        // 预留：冲刺输入（需要在InputActions中添加Sprint动作）
        // _input.GamePlay.Sprint.performed += _ => SprintPressed = true;
        // _input.GamePlay.Sprint.canceled += _ => SprintPressed = false;
    }

    private void OnEnable()
    {
        _input.Enable();
    }

    private void OnDisable()
    {
        _input.Disable();
    }
    
    // 用于消费跳跃输入（防止连续跳）
    public void ConsumeJumpInput()
    {
        JumpPressed = false;
    }
}
