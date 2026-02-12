using System;
using System.Collections;
using System.Collections.Generic;
using _Projects.GamePlay;
using UnityEngine;

public class RotationObject : DisposableObject
{
    public float hoverSpeed = 1f;
    public float hoverHeight = 0.5f;
    public float rotateSpeed = 30f;
    public Vector3 rotateAxis = Vector3.right;
    public bool reverseRotation = false;
    
    public bool stop = false;

    private Vector3 startPos;
    
    private void Start()
    {
        startPos = transform.position;
    }

    public override void ChangeState()
    {
        base.ChangeState();
        if (IsAttached)
        {
            stop = true;
            Rotation();
        }
    }

    public override void Recycle()
    {
        Rotation();
        base.Recycle();
        stop = false;

    }

    private void Update()
    {
        if (!stop) { Rotation(); }
        
    }

    private void Rotation()
    {
        // 上下悬浮
        float newY = startPos.y + Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);

        // 匀速自转，支持自定义旋转轴和方向
        float direction = reverseRotation ? -1f : 1f;
        transform.Rotate(rotateAxis, rotateSpeed * direction * Time.deltaTime);
        
    }

   
}
