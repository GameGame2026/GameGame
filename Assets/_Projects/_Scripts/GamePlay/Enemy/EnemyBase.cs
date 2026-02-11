using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace _Projects.GamePlay
{
    /// <summary>
    /// 敌人基类 - 所有敌人继承此类
    /// 包含通用AI逻辑、状态机、移动、攻击等功能
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyBase : DisposableObject
    {
        [Header("基础属性")]
        [Tooltip("最大血量")]
        public float maxHealth = 100f;
        
        [Tooltip("当前血量")]
        public float currentHealth;
        
        [Tooltip("攻击力")]
        public float attackDamage = 10f;
        
        [Tooltip("攻击冷却时间")]
        public float AttackCooldown = 1.5f;

        [Header("移动设置")]
        [Tooltip("巡逻速度")]
        public float PatrolSpeed = 2f;
        
        [Tooltip("追击速度")]
        public float chaseSpeed = 4f;
        
        [Tooltip("转向速度")]
        public float rotationSpeed = 5f;

        [Header("检测范围")]
        [Tooltip("警戒范围（检测玩家）")]
        public float detectionRange = 8f;
        
        [Tooltip("追击范围")]
        public float chaseRange = 15f;
        
        [Tooltip("攻击范围")]
        public float attackRange = 2f;
        
        [Tooltip("巡逻范围半径")]
        public float patrolRadius = 10f;

        [Header("战斗设置")]
        [Tooltip("无敌帧时间")]
        public float invincibilityTime = 0.3f;

        [Header("音效")]
        public AudioClip hitSound;
        public AudioClip attackSound;
        public AudioClip deathSound;
        [Range(0, 1)] public float soundVolume = 0.7f;

        [Header("受击视觉效果")]
        [Tooltip("受击时的闪红颜色")]
        public Color hitFlashColor = new Color(1f, 0f, 0f, 0.7f);
        
        [Tooltip("受击闪红持续时间")]
        public float hitFlashDuration = 0.2f;

        [Header("贴点系统")]
        [Tooltip("是否可被攻击")]
        public bool CanBeAttacked = true;
        
        
        
        // 贴点状态
        // `IsAttached` from DisposableObject is used to track whether this object is attached.
        // 使用基类 DisposableObject 中的 `IsAttached`（已定义为 public bool IsAttached { get; protected set; }）
        // 这里不再重复定义 IsPointAttached。
        protected MaterialFlashEffect _materialFlash;
        public bool IsFriendly { get; protected set; }

        // 组件引用
        protected Rigidbody _rigidbody;
        protected Animator _animator;
        protected NavMeshAgent _navAgent;
        protected Collider _collider;

        // 状态机
        protected EnemyStateMachine _stateMachine;

        // 玩家引用
        protected Transform _player;
        protected PlayerStats _playerStats;

        // 内部状态
        protected bool _isInvincible;
        protected float _invincibilityTimer;
        protected Vector3 _spawnPosition;

        // 攻击相关状态
        protected bool _isAttacking; // 攻击播放期间保持原地不动
        
        public bool IsAttacking => _isAttacking;

        // 动画参数
        protected int _animSpeed;
        protected int _animAttack;
        protected int _animHit;
        protected int _animDead;
        protected int _animPointAttached;
        
        public GameObject attackTriggerRange;

        protected virtual void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _animator = GetComponentInChildren<Animator>();
            _navAgent = GetComponent<NavMeshAgent>();
            _collider = GetComponent<Collider>();
            
            // 获取或添加 MaterialFlashEffect 组件
            _materialFlash = GetComponent<MaterialFlashEffect>();
            if (_materialFlash == null)
            {
                _materialFlash = gameObject.AddComponent<MaterialFlashEffect>();
            }
            
            _spawnPosition = transform.position;
            currentHealth = maxHealth;

            // 初始化动画参数
            _animSpeed = Animator.StringToHash("Speed");
            _animAttack = Animator.StringToHash("Attack");
            _animHit = Animator.StringToHash("Hit");
            _animDead = Animator.StringToHash("Dead");
            _animPointAttached = Animator.StringToHash("PointAttached");
            
            // 确保 animator 的初始状态与 IsPointAttached 同步
            if (_animator != null)
            {
                // 使用基类的 IsAttached
                _animator.SetBool(_animPointAttached, IsAttached);
            }

            if (attackTriggerRange != null)
            {
                attackTriggerRange.SetActive(false);
            }
          
        }

        protected virtual void Start()
        {
            // 查找玩家
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                _player = playerObj.transform;
                _playerStats = playerObj.GetComponent<PlayerStats>();
            }

            // 初始化状态机
            InitializeStateMachine();
        }

        /// <summary>
        /// 初始化状态机 - 子类可以重写以添加特殊状态
        /// </summary>
        protected virtual void InitializeStateMachine()
        {
            _stateMachine = new EnemyStateMachine(this);

            // 注册基础状态
            _stateMachine.RegisterState(EnemyStateType.Idle, new EnemyIdleState(this, _stateMachine));
            _stateMachine.RegisterState(EnemyStateType.Patrol, new EnemyPatrolState(this, _stateMachine));
            _stateMachine.RegisterState(EnemyStateType.Alert, new EnemyAlertState(this, _stateMachine));
            _stateMachine.RegisterState(EnemyStateType.Chase, new EnemyChaseState(this, _stateMachine));
            _stateMachine.RegisterState(EnemyStateType.Attack, new EnemyAttackState(this, _stateMachine));
            _stateMachine.RegisterState(EnemyStateType.Stunned, new EnemyStunnedState(this, _stateMachine));
            _stateMachine.RegisterState(EnemyStateType.Dead, new EnemyDeadState(this, _stateMachine));
            _stateMachine.RegisterState(EnemyStateType.Friendly, new EnemyFriendlyState(this, _stateMachine));

            // 从待机状态开始
            _stateMachine.ChangeState(EnemyStateType.Idle);
        }

        protected virtual void Update()
        {
            // 更新状态机
            _stateMachine?.Update();

            // 更新无敌帧
            UpdateInvincibility();
        }

        protected virtual void FixedUpdate()
        {
            _stateMachine?.FixedUpdate();
        }

        #region 移动相关

        /// <summary>
        /// 移动到目标位置
        /// </summary>
        public virtual void MoveTo(Vector3 destination, float speed)
        {
            // 如果正在播放攻击动画，保持原地不动
            if (_isAttacking)
            {
                // 确保 agent/rigidbody 不会移动
                if (_navAgent != null && _navAgent.enabled)
                {
                    _navAgent.isStopped = true;
                    _navAgent.ResetPath();
                }
                if (_rigidbody != null)
                {
                    _rigidbody.velocity = Vector3.zero;
                }
                // 更新动画移动参数为0
                if (_animator != null)
                {
                    _animator.SetFloat(_animSpeed, 0f);
                }
                return;
            }
            
            if (_navAgent != null && _navAgent.enabled)
            {
                _navAgent.speed = speed;
                _navAgent.SetDestination(destination);
            }
            else
            {
                // 简单移动逻辑
                Vector3 direction = (destination - transform.position).normalized;
                direction.y = 0;
                transform.position += direction * speed * Time.deltaTime;
                
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
            }

            // 更新动画
            if (_animator != null)
            {
                _animator.SetFloat(_animSpeed, speed > 0 ? 1f : 0f);
            }
        }

        /// <summary>
        /// 追击玩家
        /// </summary>
        public virtual void ChasePlayer()
        {
            if (_player != null)
            {
                MoveTo(_player.position, chaseSpeed);
            }
        }

        /// <summary>
        /// 获取随机巡逻点
        /// </summary>
        public virtual Vector3 GetRandomPatrolPoint()
        {
            Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
            randomDirection += _spawnPosition;
            randomDirection.y = transform.position.y;

            // 如果有NavMesh，使用NavMesh采样
            if (_navAgent != null)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
                {
                    return hit.position;
                }
            }

            return randomDirection;
        }

        /// <summary>
        /// 检查是否到达目标位置
        /// </summary>
        public virtual bool HasReachedDestination(Vector3 destination)
        {
            float distance = Vector3.Distance(transform.position, destination);
            return distance < 0.5f;
        }

        /// <summary>
        /// 面向玩家
        /// </summary>
        public virtual void LookAtPlayer()
        {
            if (_player == null) return;

            Vector3 direction = (_player.position - transform.position).normalized;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        #endregion

        #region 检测相关

        /// <summary>
        /// 是否能看到玩家（在警戒范围内）
        /// </summary>
        public virtual bool CanSeePlayer()
        {
            if (_player == null) return false;
            return Vector3.Distance(transform.position, _player.position) <= detectionRange;
        }

        /// <summary>
        /// 玩家是否在追击范围内
        /// </summary>
        public virtual bool IsPlayerInChaseRange()
        {
            if (_player == null) return false;
            return Vector3.Distance(transform.position, _player.position) <= chaseRange;
        }

        /// <summary>
        /// 玩家是否在攻击范围内
        /// </summary>
        public virtual bool IsPlayerInAttackRange()
        {
            if (_player == null) return false;
            return Vector3.Distance(transform.position, _player.position) <= attackRange;
        }

        /// <summary>
        /// 获取到玩家的距离
        /// </summary>
        public float GetDistanceToPlayer()
        {
            if (_player == null) return float.MaxValue;
            return Vector3.Distance(transform.position, _player.position);
        }

        #endregion

        #region 战斗相关

        /// <summary>
        /// 执行攻击 - 子类可以重写实现特殊攻击
        /// </summary>
        public virtual void PerformAttack()
        {
            // 标记为正在攻击以保持原地不动（动画期间通过 OnAttackEnd 恢复）
            _isAttacking = true;
            
            Debug.Log($"[{gameObject.name}] 执行攻击");
            
            // 停止 NavMeshAgent 的移动（如果存在）并清零刚体速度
            if (_navAgent != null && _navAgent.enabled)
            {
                _navAgent.isStopped = true;
                _navAgent.ResetPath();
            }
            if (_rigidbody != null)
            {
                _rigidbody.velocity = Vector3.zero;
            }
            
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
        }

        /// <summary>
        /// 攻击玩家（造成伤害）
        /// </summary>
        public virtual void AttackPlayer(PlayerStats player)
        {
            if (player != null && !IsFriendly)
            {
                player.TakeDamage(attackDamage);
                Debug.Log($"[{gameObject.name}] 攻击玩家，造成 {attackDamage} 点伤害");
            }
        }

        /// <summary>
        /// 动画事件：攻击开始（可在动画开头触发）
        /// 如果你在动画里已经调用了 OnAttackBegin，可以依赖该回调来处理额外逻辑
        /// </summary>
        public void OnAttackBegin()
        {
            _isAttacking = true;
            if (_navAgent != null && _navAgent.enabled)
            {
                _navAgent.isStopped = true;
                _navAgent.ResetPath();
            }
            if (_rigidbody != null)
            {
                _rigidbody.velocity = Vector3.zero;
            }
        }

        /// <summary>
        /// 动画事件：攻击结束（在动画尾帧触发）
        /// 恢复移动并记录冷却起点
        /// </summary>
        public void OnAttackEnd()
        {
            _isAttacking = false;
            if (_navAgent != null && _navAgent.enabled)
            {
                _navAgent.isStopped = false;
            }
        }

        /// <summary>
        /// 受到伤害
        /// </summary>
        public virtual void TakeDamage(float damage)
        {
            if (!CanBeAttacked || _isInvincible || currentHealth <= 0) return;

            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);

            Debug.Log($"[{gameObject.name}] 受到 {damage} 点伤害，剩余血量: {currentHealth}/{maxHealth}");

            // 播放受击音效
            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, transform.position, soundVolume);
            }

            // 触发材质闪红效果（替代受击动画）
            if (_materialFlash != null)
            {
                _materialFlash.Flash(hitFlashColor, hitFlashDuration);
            }

            // 开启无敌帧
            _isInvincible = true;
            _invincibilityTimer = invincibilityTime;

            // 检查死亡
            if (currentHealth <= 0)
            {
                _stateMachine.ChangeState(EnemyStateType.Dead);
            }
        }

        /// <summary>
        /// 更新无敌帧
        /// </summary>
        protected void UpdateInvincibility()
        {
            if (_isInvincible)
            {
                _invincibilityTimer -= Time.deltaTime;
                if (_invincibilityTimer <= 0f)
                {
                    _isInvincible = false;
                }
            }
        }

        #endregion

        #region 贴点系统

        public override void ChangeState()
        {
            base.ChangeState();
            OnPointAttached();
        }

        public override void Recycle()
        {
            base.Recycle();
            OnPointDetached();
        }

        /// <summary>
        /// 被贴点时调用 - 子类重写实现特殊效果
        /// </summary>
        public virtual void OnPointAttached()
        {
            // 同步到 Animator
            if (_animator != null)
            {
                _animator.SetBool(_animPointAttached, true);
            }
        }

        /// <summary>
        /// 点被回收时调用 - 子类重写实现特殊效果
        /// </summary>
        public virtual void OnPointDetached()
        {
            
            // 同步到 Animator
            if (_animator != null)
            {
                _animator.SetBool(_animPointAttached, false);
            }

            // 回到正常状态继续攻击
            _stateMachine.ChangeState(EnemyStateType.Idle);
        }

        /// <summary>
        /// 贴点状态切换 - 外部调用
        /// </summary>
        public void SetPointAttached(bool attached)
        {
            if (attached && !IsAttached)
            {
                _stateMachine.ChangeState(EnemyStateType.Stunned);
            }
            else if (!attached && IsAttached)
            {
                OnPointDetached();
            }
        }

        #endregion

        #region 友方系统

        /// <summary>
        /// 变为友方时调用
        /// </summary>
        public virtual void OnBecomeFriendly()
        {
            IsFriendly = true;
            Debug.Log($"[{gameObject.name}] 变为友方");
        }

        /// <summary>
        /// 变回敌对时调用
        /// </summary>
        public virtual void OnBecomeHostile()
        {
            IsFriendly = false;
            Debug.Log($"[{gameObject.name}] 变回敌对");
        }

        /// <summary>
        /// 友方行为 - 子类重写实现
        /// </summary>
        public virtual void FriendlyBehavior()
        {
            // 默认：跟随玩家
            if (_player != null)
            {
                float distanceToPlayer = GetDistanceToPlayer();
                if (distanceToPlayer > 3f)
                {
                    MoveTo(_player.position, PatrolSpeed);
                }
            }
        }

        #endregion

        #region 动画和死亡

        /// <summary>
        /// 设置动画状态
        /// </summary>
        public virtual void SetAnimationState(string stateName)
        {
            // 子类可以重写实现具体动画逻辑
        }

        /// <summary>
        /// 死亡处理
        /// </summary>
        public virtual void OnDeath()
        {
            Debug.Log($"[{gameObject.name}] 死亡");
            
            if (IsAttached)
            {
                Recycle();
            }

            // 播放死亡音效
            if (deathSound != null)
            {
                AudioSource.PlayClipAtPoint(deathSound, transform.position, soundVolume);
            }

            // 禁用碰撞
            if (_collider != null)
            {
                _collider.enabled = false;
            }

            // 禁NavMeshAgent
            if (_navAgent != null)
            {
                _navAgent.enabled = false;
            }

            // 播放死亡动画
            if (_animator != null)
            {
                _animator.SetTrigger(_animDead);
            }

            // 延迟销毁
            Destroy(gameObject, 3f);
        }

        #endregion
        
        public void EnableAttack()
        {
            attackTriggerRange.SetActive(true);
        }
        
        public void DisableAttack()
        {
            attackTriggerRange.SetActive(false);
        }

        #region Gizmos

        protected virtual void OnDrawGizmosSelected()
        {
            // 警戒范围（黄色）
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // 追击范围（橙色）
            Gizmos.color = new Color(1f, 0.5f, 0f);
            Gizmos.DrawWireSphere(transform.position, chaseRange);

            // 攻击范围（红色）
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // 巡逻范围（绿色）
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(Application.isPlaying ? _spawnPosition : transform.position, patrolRadius);
        }

        #endregion
    }
}
