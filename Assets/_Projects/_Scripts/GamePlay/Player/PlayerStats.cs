using UnityEngine;
using UnityEngine.Events;
using _Projects.GamePlay;
using _Projects.GamePlay.UI;
using _Projects._Scripts.GamePlay.UI;
using GamePlay.Controller;

public class PlayerStats : MonoBehaviour
{
    [Header("生命值设置")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool canRegenHealth = true;
    [SerializeField] private float healthRegenRate = 5f; // 每秒恢复量
    [SerializeField] private float healthRegenDelay = 3f; // 受伤后多久开始恢复
    
    [Header("点数系统（影响角色外观动画）")]
    [SerializeField] private int currentPoints = 3; // 当前点数 (0-3)
    [SerializeField] private int maxPoints = 3; // 最大点数
    
    [Header("战斗属性")]
    [SerializeField] private float attackDamage = 10f; // 攻击力
    
    [Header("无敌帧设置")]
    [SerializeField] private float invincibilityDuration = 0.7f; // 受伤后无敌时间
    private float _invincibilityTimer;
    
    [Header("音效")]
    [SerializeField] private AudioClip hitSound; // 受击音效
    [SerializeField] private AudioClip attackSound; // 攻击音效
    [SerializeField] [Range(0, 1)] private float soundVolume = 0.7f; // 音效音量
    
    [Header("受击视觉效果")]
    [Tooltip("受击时的闪红颜色")]
    [SerializeField] private Color hitFlashColor = new Color(1f, 0f, 0f, 0.7f);
    
    [Tooltip("受击闪红持续时间")]
    [SerializeField] private float hitFlashDuration = 0.2f;
    
    // 计时
    private float _lastDamageTime;
    
    // 材质闪红效果组件
    private MaterialFlashEffect _materialFlash;
    
    // 控制器引用
    private ThirdPersonController _controller;
    
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
    public float AttackDamage => attackDamage; // 攻击力
    public bool IsAlive => currentHealth > 0;
    public bool IsInvincible => _invincibilityTimer > 0;

    public float count = 1;
    
    private void Awake()
    {
        // 初始化数值
        currentHealth = maxHealth;
        
        // 获取或添加材质闪红效果组件
        _materialFlash = GetComponent<MaterialFlashEffect>();
        if (_materialFlash == null)
        {
            _materialFlash = gameObject.AddComponent<MaterialFlashEffect>();
        }
        
        // 获取控制器引用
        _controller = GetComponent<ThirdPersonController>();
    }
    
    private void Start()
    {
        OnPointsChanged?.Invoke(currentPoints);
    }
    
    private void Update()
    {
        HandleHealthRegen();
        UpdateInvincibility();
        if (count > 0)
        {
            count -= Time.deltaTime;
            OnPointsChanged?.Invoke(currentPoints);
        }
       
    }
    
    /// <summary>
    /// 处理生命值自动恢复
    /// </summary>
    private void HandleHealthRegen()
    {
        if (!canRegenHealth || !IsAlive) return;
        if (currentHealth >= maxHealth) return;
        
        // 如果有贴点对象，禁用自动回血
        if (_controller != null && _controller.HasAttachedObjects)
        {
            // Debug.Log("[PlayerStats] 有贴点对象，禁用自动回血");
            return;
        }
        
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
        
        // 播放受击音效
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position, soundVolume);
        }
        
        // 触发材质闪红效果
        if (_materialFlash != null)
        {
            _materialFlash.Flash(hitFlashColor, hitFlashDuration);
        }
        
        // 显示伤害数字
        if (DamageNumberSpawner.Instance != null)
        {
            DamageNumberSpawner.Instance.SpawnDamage(damage, transform.position, true);
        }
        
        OnHealthChanged?.Invoke(currentHealth);
        OnDamageTaken?.Invoke(damage);
        
        Debug.Log($"[PlayerStats] 玩家受到 {damage} 点伤害，剩余血量: {currentHealth}/{maxHealth}");
        
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
        
        // 显示死亡UI
        if (DeathUI.Instance != null)
        {
            DeathUI.Instance.ShowDeathPanel();
        }
        
        // 禁用玩家控制
        if (_controller != null)
        {
            _controller.enabled = false;
        }
        
        Debug.Log("[PlayerStats] 玩家已死亡");
    }
    
    /// <summary>
    /// 复活
    /// </summary>
    public void Respawn()
    {
        currentHealth = maxHealth;
        _invincibilityTimer = invincibilityDuration;
        OnHealthChanged?.Invoke(currentHealth);
        
        // 重新启用玩家控制
        if (_controller != null)
        {
            _controller.enabled = true;
        }
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
    
    
    
    
    // ===== 战斗系统方法 =====
    
    /// <summary>
    /// 触发攻击 - 在攻击动画中调用
    /// 此函数由动画事件调用，播放攻击音效
    /// </summary>
    public void OnAttackHit()
    {
        Debug.Log($"[PlayerStats] 玩家触发攻击");
        
        // 播放攻击音效
        if (attackSound != null)
        {
            AudioSource.PlayClipAtPoint(attackSound, transform.position, soundVolume);
        }
        
        // 注意：实际的伤害判定需要在碰撞检测中处理
        // 这里只是播放音效和触发攻击事件
    }
    
    /// <summary>
    /// 攻击敌人 - 当碰撞到敌人时调用（需要在攻击动画中触发）
    /// </summary>
    /// <param name="enemy">敌人的Enemy组件</param>
    public void AttackEnemy(EnemyBase enemy)
    {
        if (enemy != null)
        {
            enemy.TakeDamage(attackDamage);
            Debug.Log($"[PlayerStats] 玩家攻击敌人 {enemy.gameObject.name}，造成 {attackDamage} 点伤害");
        }
    }
}

