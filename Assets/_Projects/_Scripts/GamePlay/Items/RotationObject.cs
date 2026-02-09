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

        // 匀速自转
        transform.Rotate(Vector3.right, rotateSpeed * Time.deltaTime);
        
    }

   
}
