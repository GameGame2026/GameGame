using UnityEngine;
using UnityEngine.Events;

public class PlayerStats : MonoBehaviour
{
    [Header("生命值设置")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool canRegenHealth = true;
    [SerializeField] private float healthRegenRate = 5f; // 每秒恢复量
    [SerializeField] private float healthRegenDelay = 3f; // 受伤后多久开始恢复
    
    [Header("点数系统（影响角色外观动画）")]
    [SerializeField] private int currentPoints = 0; // 当前点数 (0-3)
    [SerializeField] private int maxPoints = 3; // 最大点数
    
    [Header("无敌帧设置")]
    [SerializeField] private float invincibilityDuration = 1f; // 受伤后无敌时间
    private float _invincibilityTimer;
    
    // 计时
    private float _lastDamageTime;
    
    // 事件
    [Header("事件")]
    public UnityEvent<float> OnHealthChanged;
    public UnityEvent<float> OnStaminaChanged;
    public UnityEvent<int> OnPointsChanged; // 点数变化事件
    public UnityEvent OnDeath;
    public UnityEvent<float> OnDamageTaken;
    public UnityEvent OnHealthRegen;
    
    // 属性
    public float Health => currentHealth;
    public float MaxHealth => maxHealth;
    public float HealthPercent => currentHealth / maxHealth;
    public int Points => currentPoints; // 当前点数
    public int MaxPoints => maxPoints; // 最大点数
    public bool IsAlive => currentHealth > 0;
    public bool IsInvincible => _invincibilityTimer > 0;
    
    private void Awake()
    {
        // 初始化数值
        currentHealth = maxHealth;
    }
    
    private void Update()
    {
        HandleHealthRegen();
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
        _invincibilityTimer = invincibilityDuration;
        OnHealthChanged?.Invoke(currentHealth);
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
    
    // ===== 点数系统方法 =====
    
    /// <summary>
    /// 增加点数
    /// </summary>
    public void AddPoints(int amount)
    {
        if (amount <= 0) return;
        
        int oldPoints = currentPoints;
        currentPoints = Mathf.Clamp(currentPoints + amount, 0, maxPoints);
        
        if (currentPoints != oldPoints)
        {
            OnPointsChanged?.Invoke(currentPoints);
            Debug.Log($"[PlayerStats] 点数增加: {oldPoints} -> {currentPoints}");
        }
    }
    
    /// <summary>
    /// 减少点数
    /// </summary>
    public void RemovePoints(int amount)
    {
        if (amount <= 0) return;
        
        int oldPoints = currentPoints;
        currentPoints = Mathf.Clamp(currentPoints - amount, 0, maxPoints);
        
        if (currentPoints != oldPoints)
        {
            OnPointsChanged?.Invoke(currentPoints);
            Debug.Log($"[PlayerStats] 点数减少: {oldPoints} -> {currentPoints}");
        }
    }
    
    /// <summary>
    /// 直接设置点数
    /// </summary>
    public void SetPoints(int points)
    {
        int oldPoints = currentPoints;
        currentPoints = Mathf.Clamp(points, 0, maxPoints);
        
        if (currentPoints != oldPoints)
        {
            OnPointsChanged?.Invoke(currentPoints);
            Debug.Log($"[PlayerStats] 点数设置: {oldPoints} -> {currentPoints}");
        }
    }
}

