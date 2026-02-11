using UnityEngine;

namespace _Projects.GamePlay
{
    /// <summary>
    /// 红心扑克敌人
    /// 特殊机制：
    /// - 贴点后反转：受击时触发→为玩家恢复1点血量
    /// - 状态切换：正常态↔治愈态
    /// </summary>
    public class HeartPokerEnemy : EnemyBase
    {
        [Header("红心扑克特殊设置")]
        [Tooltip("治愈模式下恢复的血量")]
        public float healAmount = 1f;
        
        [Tooltip("治愈攻击冷却时间（秒）")]
        public float healAttackCooldown = 1f;
        
        [Tooltip("是否处于治愈模式")]
        public bool IsHealingMode => IsAttached;

        [Header("视觉效果")]
        [Tooltip("正常态材质")]
        public Material normalMaterial;
        
        [Tooltip("治愈态材质")]
        public Material healingMaterial;

        private Renderer _renderer;
        private float _lastHealAttackTime; 

        protected override void Awake()
        {
            base.Awake();
            _renderer = GetComponentInChildren<Renderer>();
        }

        /// <summary>
        /// 被贴点 - 进入治愈模式
        /// </summary>
        public override void OnPointAttached()
        {
            base.OnPointAttached();
            
            if (_renderer != null && healingMaterial != null)
            {
                _renderer.material = healingMaterial;
            }
            
            Debug.Log($"[HeartPoker] {gameObject.name} 进入治愈模式");
        }

        /// <summary>
        /// 点被回收 - 退出治愈模式
        /// </summary>
        public override void OnPointDetached()
        {
            base.OnPointDetached();
            
            // 切换回正常态材质
            if (_renderer != null && normalMaterial != null)
            {
                _renderer.material = normalMaterial;
            }
            
            Debug.Log($"[HeartPoker] {gameObject.name} 退出治愈模式");
        }

        public override void AttackPlayer(PlayerStats player)
        {
            if (IsHealingMode)
            {
                // 检查治愈攻击冷却
                if (Time.time - _lastHealAttackTime >= healAttackCooldown)
                {
                    if (player != null)
                    {
                        player.Heal(healAmount);
                        _lastHealAttackTime = Time.time;
                        Debug.Log($"[HeartPoker] 治愈模式：为玩家恢复 {healAmount} 点血量");
                        
                        // 播放治愈音效
                        if (hitSound != null)
                        {
                            AudioSource.PlayClipAtPoint(hitSound, transform.position, soundVolume);
                        }
                        
                        // 播放治愈动画
                        if (_animator != null)
                        {
                            _animator.SetTrigger(_animAttack);
                        }
                    }
                }
            }
            else
            {
                // 正常攻击模式
                base.AttackPlayer(player);
            }
        }
    }
}
