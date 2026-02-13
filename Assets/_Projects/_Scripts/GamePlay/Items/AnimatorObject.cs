using _Projects.GamePlay;
using UnityEngine;

public class AnimatorObject : DisposableObject
{
    [Header("Animator 设置")]
    [Tooltip("要控制的 Animator 组件")]
    public Animator targetAnimator;

    [Header("伤害设置")]
    [Tooltip("攻击力")]
    public float attackDamage = 10f;
    
    [Tooltip("攻击冷却时间")]
    public float attackCooldown = 1.5f;
    
    private bool isPaused = false;
    private float _lastAttackTime = -999f;

    private void Awake()
    {
        // 如果没有手动指定，尝试自动获取
        if (targetAnimator == null)
        {
            targetAnimator = GetComponent<Animator>();
            if (targetAnimator == null)
            {
                targetAnimator = GetComponentInChildren<Animator>();
            }
        }
    }

    public override void ChangeState()
    {
        if (!isPaused && targetAnimator != null)
        {
            // 暂停动画
            targetAnimator.speed = 0f;
            isPaused = true;
            Debug.Log($"[AnimatorObject] 暂停动画: {gameObject.name}");
        }
        
        base.ChangeState();
    }

    public override void Recycle()
    {
        if (isPaused && targetAnimator != null)
        {
            // 恢复动画播放
            targetAnimator.speed = 1f;
            isPaused = false;
            Debug.Log($"[AnimatorObject] 恢复动画: {gameObject.name}");
        }
        
        base.Recycle();
    }

    /// <summary>
    /// 攻击玩家（造成伤害）- 在 Trigger 中调用
    /// </summary>
    public void AttackPlayer(PlayerStats player)
    {
        // 检查冷却时间
        if (Time.time - _lastAttackTime < attackCooldown)
        {
            return;
        }

        // 只有在动画未暂停时才能攻击
        if (!isPaused && player != null)
        {
            player.TakeDamage(attackDamage);
            _lastAttackTime = Time.time;
            Debug.Log($"[AnimatorObject] {gameObject.name} 攻击玩家，造成 {attackDamage} 点伤害");
        }
    }
}

