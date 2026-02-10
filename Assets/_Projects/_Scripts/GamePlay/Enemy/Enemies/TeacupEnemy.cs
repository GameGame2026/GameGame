using UnityEngine;

namespace _Projects.GamePlay
{
    /// <summary>
    /// 茶杯敌人（辅助型）
    /// 特殊机制：
    /// - 贴点前：大范围泼水攻击(2点伤害)
    /// - 贴点后：旋转变身，变为友方跟随单位
    /// - 友方AI：自动攻击进入范围的敌人（1点伤害）
    /// </summary>
    public class TeacupEnemy : EnemyBase
    {
        [Header("茶杯特殊设置")]
        [Tooltip("泼水攻击范围")]
        public float splashRange = 4f;
        
        [Tooltip("泼水攻击角度")]
        public float splashAngle = 90f;
        
        [Tooltip("泼水伤害")]
        public float splashDamage = 2f;
        
        [Tooltip("友方攻击伤害")]
        public float friendlyAttackDamage = 1f;
        
        [Tooltip("友方跟随距离")]
        public float followDistance = 3f;
        
        [Tooltip("友方攻击范围")]
        public float friendlyAttackRange = 2f;

        [Header("变身效果")]
        [Tooltip("变身旋转速度")]
        public float transformSpinSpeed = 720f;
        
        [Tooltip("变身持续时间")]
        public float transformDuration = 1f;
        
        [Tooltip("敌对态材质")]
        public Material hostileMaterial;
        
        [Tooltip("友方态材质")]
        public Material friendlyMaterial;

        private Renderer _renderer;
        private bool _isTransforming;
        private float _transformTimer;
        private EnemyBase _currentTarget;
        private float _friendlyAttackCooldown;

        protected override void Awake()
        {
            base.Awake();
            _renderer = GetComponentInChildren<Renderer>();
            
            // 设置攻击力为泼水伤害
            attackDamage = splashDamage;
        }

        /// <summary>
        /// 执行攻击 - 泼水攻击或友方攻击
        /// </summary>
        public override void PerformAttack()
        {
            if (IsFriendly)
            {
                // 友方模式：攻击敌人
                FriendlyAttack();
            }
            else
            {
                // 敌对模式：泼水攻击
                SplashAttack();
            }
        }

        /// <summary>
        /// 泼水攻击 - 大范围攻击
        /// </summary>
        private void SplashAttack()
        {
            Debug.Log($"[Teacup] {gameObject.name} 泼水攻击");

            // 播放攻击动画
            if (_animator != null)
            {
                _animator.SetTrigger(_animAttack);
            }

            // 播放攻击音效
            if (attackSound != null)
            {
                AudioSource.PlayClipAtPoint(attackSound, transform.position, soundVolume);
            }

            // 检测范围内的玩家
            if (_player != null)
            {
                float distance = Vector3.Distance(transform.position, _player.position);
                if (distance <= splashRange)
                {
                    // 检查是否在攻击角度内
                    Vector3 directionToPlayer = (_player.position - transform.position).normalized;
                    float angle = Vector3.Angle(transform.forward, directionToPlayer);
                    
                    if (angle <= splashAngle / 2f)
                    {
                        _playerStats?.TakeDamage(splashDamage);
                        Debug.Log($"[Teacup] 泼水命中玩家，造成 {splashDamage} 点伤害");
                    }
                }
            }
        }

        /// <summary>
        /// 友方攻击 - 攻击敌人
        /// </summary>
        private void FriendlyAttack()
        {
            if (_currentTarget == null) return;

            Debug.Log($"[Teacup] {gameObject.name} 友方攻击敌人 {_currentTarget.name}");

            // 播放攻击动画
            if (_animator != null)
            {
                _animator.SetTrigger(_animAttack);
            }

            // 播放攻击音效
            if (attackSound != null)
            {
                AudioSource.PlayClipAtPoint(attackSound, transform.position, soundVolume);
            }

            // 对目标造成伤害
            _currentTarget.TakeDamage(friendlyAttackDamage);
        }

        /// <summary>
        /// 被贴点 - 开始变身
        /// </summary>
        public override void OnPointAttached()
        {
            base.OnPointAttached();
            
            // 开始变身动画
            _isTransforming = true;
            _transformTimer = transformDuration;
            
            Debug.Log($"[Teacup] {gameObject.name} 开始变身");
        }

        /// <summary>
        /// 点被回收 - 恢复敌对状态
        /// </summary>
        public override void OnPointDetached()
        {
            base.OnPointDetached();
            
            // 立即恢复敌对
            IsFriendly = false;
            _isTransforming = false;
            
            // 切换材质
            if (_renderer != null && hostileMaterial != null)
            {
                _renderer.material = hostileMaterial;
            }
            
            // 重置攻击力
            attackDamage = splashDamage;
            
            Debug.Log($"[Teacup] {gameObject.name} 恢复敌对状态");
        }

        /// <summary>
        /// 变为友方
        /// </summary>
        public override void OnBecomeFriendly()
        {
            base.OnBecomeFriendly();
            
            // 切换材质
            if (_renderer != null && friendlyMaterial != null)
            {
                _renderer.material = friendlyMaterial;
            }
            
            // 设置友方攻击力
            attackDamage = friendlyAttackDamage;
        }

        /// <summary>
        /// 友方行为 - 跟随玩家并攻击敌人
        /// </summary>
        public override void FriendlyBehavior()
        {
            if (_player == null) return;

            // 跟随玩家
            float distanceToPlayer = GetDistanceToPlayer();
            if (distanceToPlayer > followDistance)
            {
                MoveTo(_player.position, PatrolSpeed);
            }
            else
            {
                // 在跟随范围内，寻找并攻击敌人
                FindAndAttackNearbyEnemy();
            }
        }

        /// <summary>
        /// 寻找并攻击附近的敌人
        /// </summary>
        private void FindAndAttackNearbyEnemy()
        {
            // 攻击冷却
            _friendlyAttackCooldown -= Time.deltaTime;
            
            // 查找范围内的敌人
            Collider[] colliders = Physics.OverlapSphere(transform.position, friendlyAttackRange);
            
            EnemyBase closestEnemy = null;
            float closestDistance = float.MaxValue;
            
            foreach (var col in colliders)
            {
                EnemyBase enemy = col.GetComponent<EnemyBase>();
                
                // 跳过自己和友方
                if (enemy == null || enemy == this || enemy.IsFriendly)
                    continue;
                
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }
            
            if (closestEnemy != null)
            {
                _currentTarget = closestEnemy;
                
                // 面向敌人
                Vector3 direction = (closestEnemy.transform.position - transform.position).normalized;
                direction.y = 0;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
                
                // 如果在攻击范围内且冷却完成，执行攻击
                if (closestDistance <= friendlyAttackRange && _friendlyAttackCooldown <= 0f)
                {
                    FriendlyAttack();
                    _friendlyAttackCooldown = AttackCooldown;
                }
                else if (closestDistance > friendlyAttackRange)
                {
                    // 移动到敌人位置
                    MoveTo(closestEnemy.transform.position, chaseSpeed);
                }
            }
            else
            {
                _currentTarget = null;
            }
        }

        protected override void Update()
        {
            // 变身动画
            if (_isTransforming)
            {
                // 旋转效果
                transform.Rotate(Vector3.up, transformSpinSpeed * Time.deltaTime);
                
                _transformTimer -= Time.deltaTime;
                if (_transformTimer <= 0f)
                {
                    _isTransforming = false;
                    
                    // 变身完成，切换到友方状态
                    _stateMachine.ChangeState(EnemyStateType.Friendly);
                }
                
                return; // 变身期间不执行其他更新
            }
            
            base.Update();
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            
            // 泼水攻击范围（蓝色扇形）
            Gizmos.color = Color.blue;
            
            // 绘制扇形范围
            Vector3 forward = transform.forward * splashRange;
            Vector3 left = Quaternion.Euler(0, -splashAngle / 2f, 0) * forward;
            Vector3 right = Quaternion.Euler(0, splashAngle / 2f, 0) * forward;
            
            Gizmos.DrawLine(transform.position, transform.position + left);
            Gizmos.DrawLine(transform.position, transform.position + right);
            Gizmos.DrawLine(transform.position + left, transform.position + right);
            
            // 友方攻击范围（青色）
            if (IsFriendly)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, friendlyAttackRange);
            }
        }
    }
}

