using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePhaseShifter : MonoBehaviour
{
    [Header("透明度设置")]
    [Range(0, 1)] public float intangibleAlpha = 0.3f;
    [Range(0, 1)] public float tangibleAlpha = 1.0f;
    
    [Header("当前状态")]
    public bool isIntangible = false;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            TogglePhase();
        }
    }

    private void ChangeState(bool makeIntangible)
    {
        Debug.Log("Changing phase state to " + (makeIntangible ? "Intangible" : "Tangible"));
        isIntangible = makeIntangible;
        
        // 获取渲染器
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            // 如果还没有创建新材质，就创建一个
            if (!renderer.material.name.Contains("Instance"))
            {
                renderer.material = new Material(renderer.material);
            }
            
            Color color = renderer.material.color;
            color.a = isIntangible ? intangibleAlpha : tangibleAlpha;
            renderer.material.color = color;
        }
        
        BoxCollider collider = GetComponent<BoxCollider>();
        if (collider != null)
        {
            collider.isTrigger = isIntangible;
        }
    }
    
    // 切换状态
    public void TogglePhase()
    {
        ChangeState(!isIntangible);
    }
    
    // 设置为虚化
    public void SetIntangible()
    {
        ChangeState(true);
    }
    
    // 设置为实体
    public void SetTangible()
    {
        ChangeState(false);
    }
    
    // 从Inspector调用
    [ContextMenu("切换状态")]
    private void ToggleInEditor()
    {
        TogglePhase();
    }
}