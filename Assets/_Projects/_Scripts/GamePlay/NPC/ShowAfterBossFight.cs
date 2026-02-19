using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowAfterBossFight : MonoBehaviour
{
    public void Start()
    {
        gameObject.SetActive(false);
    }
    
    // 延迟时间，可以在 Inspector 中调整
    public float delay = 1f;

    // 公共方法，供外部调用（例如猫的死亡事件）
    public void AppearAfterDelay()
    {
        // 延迟 delay 秒后调用 ActivateSelf 方法
        Invoke(nameof(ActivateSelf), delay);
    }

    // 实际激活对象的方法
    private void ActivateSelf()
    {
        gameObject.SetActive(true);
    }
}
