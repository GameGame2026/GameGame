using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 玩家状态管理器 - 管理生命值、耐力等数值
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [Header("生命值设置")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool canRegenHealth = true;
    [SerializeField] private float healthRegenRate = 5f; // 每秒恢复量
    [SerializeField] private float healthRegenDelay = 3f; // 受伤后多久开始恢复
    
    [Header("耐力设置")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float currentStamina;
    [SerializeField] private float staminaRegenRate = 20f; // 每秒恢复量
    [SerializeField] private float staminaDrainRate = 10f; // 冲刺时每秒消耗
    
    [Header("无敌帧设置")]
    [SerializeField] private float invincibilityDuration = 1f; // 受伤后无敌时间
    private float _invincibilityTimer;
    
    // 计时��
    private float _lastDamageTime;
    
    // 事件
    [Header("事件")]
    public UnityEvent<float> OnHealthChanged;
    public UnityEvent<float> OnStaminaChanged;
    public UnityEvent OnDeath;
    public UnityEvent<float> OnDamageTaken;
    public UnityEvent OnHealthRegen;
    
    // 属性
    public float Health => currentHealth;
    public float MaxHealth => maxHealth;
    public float HealthPercent => currentHealth / maxHealth;
    public float Stamina => currentStamina;
    public float MaxStamina => maxStamina;
    public float StaminaPercent => currentStamina / maxStamina;
    public bool IsAlive => currentHealth > 0;
    public bool IsInvincible => _invincibilityTimer > 0;
    public bool HasStamina => currentStamina > 0;
    
    private void Awake()
    {
        // 初始化数值
        currentHealth = maxHealth;
        currentStamina = maxStamina;
    }
    
    private void Update()
    {
        HandleHealthRegen();
        HandleStaminaRegen();
        UpdateInvincibility();
    }
    
    /// <summary>
    /// 处理生命值自动恢复
    /// </summary>
    private void HandleHealthRegen()
    {
        if (!canRegenHealth || !IsAlive) return;
        if (currentHealth >= maxHealth) return;
        
        // 检查是否超过延迟时间
        if (Time.time - _lastDamageTime >= healthRegenDelay)
        {
            float regenAmount = healthRegenRate * Time.deltaTime;
            currentHealth = Mathf.Min(currentHealth + regenAmount, maxHealth);
            OnHealthChanged?.Invoke(currentHealth);
            OnHealthRegen?.Invoke();
        }
    }
    
    /// <summary>
    /// 处理耐力自动恢复
    /// </summary>
    private void HandleStaminaRegen()
    {
        if (currentStamina >= maxStamina) return;
        
        float regenAmount = staminaRegenRate * Time.deltaTime;
        currentStamina = Mathf.Min(currentStamina + regenAmount, maxStamina);
        OnStaminaChanged?.Invoke(currentStamina);
    }
    
    /// <summary>
    /// 更新无敌时间
    /// </summary>
    private void UpdateInvincibility()
    {
        if (_invincibilityTimer > 0)
        {
            _invincibilityTimer -= Time.deltaTime;
        }
    }
    
    /// <summary>
    /// 受到伤害
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (!IsAlive || IsInvincible || damage <= 0) return;
        
        currentHealth = Mathf.Max(currentHealth - damage, 0);
        _lastDamageTime = Time.time;
        _invincibilityTimer = invincibilityDuration;
        
        OnHealthChanged?.Invoke(currentHealth);
        OnDamageTaken?.Invoke(damage);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    /// <summary>
    /// 恢复生命值
    /// </summary>
    public void Heal(float amount)
    {
        if (!IsAlive || amount <= 0) return;
        
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
    }
    
    /// <summary>
    /// 消耗耐力
    /// </summary>
    public bool UseStamina(float amount)
    {
        if (currentStamina < amount) return false;
        
        currentStamina = Mathf.Max(currentStamina - amount, 0);
        OnStaminaChanged?.Invoke(currentStamina);
        return true;
    }
    
    /// <summary>
    /// 持续消耗耐力（用于冲刺等）
    /// </summary>
    public bool DrainStamina(float deltaTime)
    {
        float drainAmount = staminaDrainRate * deltaTime;
        return UseStamina(drainAmount);
    }
    
    /// <summary>
    /// 恢复耐力
    /// </summary>
    public void RestoreStamina(float amount)
    {
        if (amount <= 0) return;
        
        currentStamina = Mathf.Min(currentStamina + amount, maxStamina);
        OnStaminaChanged?.Invoke(currentStamina);
    }
    
    /// <summary>
    /// 设置最大生命值
    /// </summary>
    public void SetMaxHealth(float newMaxHealth)
    {
        float healthPercent = HealthPercent;
        maxHealth = Mathf.Max(newMaxHealth, 1);
        currentHealth = maxHealth * healthPercent;
        OnHealthChanged?.Invoke(currentHealth);
    }
    
    /// <summary>
    /// 设置最大耐力
    /// </summary>
    public void SetMaxStamina(float newMaxStamina)
    {
        float staminaPercent = StaminaPercent;
        maxStamina = Mathf.Max(newMaxStamina, 1);
        currentStamina = maxStamina * staminaPercent;
        OnStaminaChanged?.Invoke(currentStamina);
    }
    
    /// <summary>
    /// 完全恢复
    /// </summary>
    public void FullRestore()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        OnHealthChanged?.Invoke(currentHealth);
        OnStaminaChanged?.Invoke(currentStamina);
    }
    
    /// <summary>
    /// 死亡
    /// </summary>
    private void Die()
    {
        OnDeath?.Invoke();
        // 这里可以添加死亡逻辑，比如播放动画、禁用控制等
    }
    
    /// <summary>
    /// 复活
    /// </summary>
    public void Respawn()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        _invincibilityTimer = invincibilityDuration;
        OnHealthChanged?.Invoke(currentHealth);
        OnStaminaChanged?.Invoke(currentStamina);
    }
    
    /// <summary>
    /// 即死（用于掉落悬崖等）
    /// </summary>
    public void InstantKill()
    {
        currentHealth = 0;
        OnHealthChanged?.Invoke(currentHealth);
        Die();
    }
}

