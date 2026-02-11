using UnityEngine;
using _Projects.GamePlay;

/// <summary>
/// 示例脚本：展示如何在自定义对象上使用材质闪红效果
/// 可以将此脚本作为模板，用于实现自己的受击反馈
/// </summary>
public class MaterialFlashExample : MonoBehaviour
{
    [Header("生命值")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    
    [Header("受击视觉效果")]
    [Tooltip("受击时的闪红颜色")]
    [SerializeField] private Color hitFlashColor = new Color(1f, 0f, 0f, 0.7f);
    
    [Tooltip("受击闪红持续时间")]
    [SerializeField] private float hitFlashDuration = 0.2f;
    
    [Header("不同伤害类型的颜色")]
    [SerializeField] private Color normalDamageColor = new Color(1f, 0f, 0f, 0.7f);     // 红色
    [SerializeField] private Color fireDamageColor = new Color(1f, 0.3f, 0f, 0.8f);    // 橙色
    [SerializeField] private Color iceDamageColor = new Color(0f, 0.5f, 1f, 0.7f);     // 蓝色
    [SerializeField] private Color poisonDamageColor = new Color(0.5f, 1f, 0f, 0.7f);  // 绿色
    
    // 材质闪红效���组件
    private MaterialFlashEffect _flashEffect;

    void Awake()
    {
        // 初始化生命值
        currentHealth = maxHealth;
        
        // 获取或添加材质闪红效果组件
        _flashEffect = GetComponent<MaterialFlashEffect>();
        if (_flashEffect == null)
        {
            _flashEffect = gameObject.AddComponent<MaterialFlashEffect>();
        }
        
        // 可以在这里自定义闪红效果的参数
        _flashEffect.SetFlashColor(hitFlashColor);
        _flashEffect.SetFlashDuration(hitFlashDuration);
    }

    /// <summary>
    /// 示例 1: 基础受伤
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        Debug.Log($"[{gameObject.name}] 受到 {damage} 点伤害，剩余生命: {currentHealth}/{maxHealth}");
        
        // 触发闪红效果
        _flashEffect.Flash(hitFlashColor, hitFlashDuration);
        
        if (currentHealth <= 0)
        {
            OnDeath();
        }
    }

    /// <summary>
    /// 示例 2: 根据伤害大小使用不同强度的闪红效果
    /// </summary>
    public void TakeDamageWithIntensity(float damage)
    {
        if (currentHealth <= 0) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        // 根据伤害大小调整闪红效果
        float intensity = Mathf.Clamp01(damage / 50f); // 50点伤害为最大强度
        Color flashColor = new Color(1f, 0f, 0f, 0.5f + intensity * 0.5f);
        float duration = 0.15f + intensity * 0.35f; // 0.15-0.5秒
        
        _flashEffect.Flash(flashColor, duration);
        
        Debug.Log($"[{gameObject.name}] 受到 {damage} 点伤害（强度: {intensity:F2}），剩余生命: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            OnDeath();
        }
    }

    /// <summary>
    /// 示例 3: 不同类型的伤害使用不同颜色
    /// </summary>
    public void TakeDamageByType(float damage, DamageType damageType)
    {
        if (currentHealth <= 0) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        // 根据伤害类型选择颜色
        Color flashColor;
        switch (damageType)
        {
            case DamageType.Fire:
                flashColor = fireDamageColor;
                break;
            case DamageType.Ice:
                flashColor = iceDamageColor;
                break;
            case DamageType.Poison:
                flashColor = poisonDamageColor;
                break;
            default:
                flashColor = normalDamageColor;
                break;
        }
        
        _flashEffect.Flash(flashColor, hitFlashDuration);
        
        Debug.Log($"[{gameObject.name}] 受到 {damage} 点{damageType}伤害，剩余生命: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            OnDeath();
        }
    }

    /// <summary>
    /// 示例 4: 持续伤害效果（如中毒、燃烧）
    /// </summary>
    public void ApplyDamageOverTime(float damagePerSecond, DamageType damageType, float duration)
    {
        StartCoroutine(DamageOverTimeCoroutine(damagePerSecond, damageType, duration));
    }

    private System.Collections.IEnumerator DamageOverTimeCoroutine(float damagePerSecond, DamageType damageType, float duration)
    {
        float elapsed = 0f;
        float tickInterval = 0.5f; // 每0.5秒造成一次伤害
        
        while (elapsed < duration && currentHealth > 0)
        {
            // 造成伤害
            float damage = damagePerSecond * tickInterval;
            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);
            
            // 选择颜色
            Color flashColor = damageType == DamageType.Fire ? fireDamageColor : poisonDamageColor;
            
            // 持续伤害使用较短的闪红时间
            _flashEffect.Flash(flashColor, 0.15f);
            
            Debug.Log($"[{gameObject.name}] 持续{damageType}伤害 {damage:F1}，剩余生命: {currentHealth}/{maxHealth}");
            
            if (currentHealth <= 0)
            {
                OnDeath();
                yield break;
            }
            
            elapsed += tickInterval;
            yield return new WaitForSeconds(tickInterval);
        }
    }

    /// <summary>
    /// 死亡处理
    /// </summary>
    private void OnDeath()
    {
        Debug.Log($"[{gameObject.name}] 死亡");
        // 这里可以添加死亡逻辑，比如播放动画、销毁对象等
        
        // 示例：延迟销毁
        Destroy(gameObject, 2f);
    }

    // ===== 测试用键盘输入 =====
    
    void Update()
    {
        // 按1键：普通伤害
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TakeDamage(10f);
        }
        
        // 按2键：高强度伤害
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TakeDamageWithIntensity(30f);
        }
        
        // 按3键：火焰伤害
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TakeDamageByType(15f, DamageType.Fire);
        }
        
        // 按4键：冰霜伤害
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            TakeDamageByType(15f, DamageType.Ice);
        }
        
        // 按5键：持续火焰伤害
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            ApplyDamageOverTime(5f, DamageType.Fire, 3f);
        }
    }
}

/// <summary>
/// 伤害类型枚举
/// </summary>
public enum DamageType
{
    Normal,   // 普通伤害
    Fire,     // 火焰伤害
    Ice,      // 冰霜伤害
    Poison    // 毒性伤害
}

