using UnityEngine;
using _Projects.GamePlay;
using UnityEngine.Events;

namespace _Projects._Scripts.GamePlay.Enemy.Enemies
{
    public class CatEnemy : EnemyBase
    {
        [Header("Cat Settings")]
        [Tooltip("触发扑跳的最大距离（当玩家在此范围内时猫会尝试扑跳）")]
        public float pounceDistance = 6f;

        [Tooltip("触发扑跳的最小距离（太近则不扑跳，改为直接攻击）")]
        public float minPounceDistance = 1.2f;

        [Tooltip("扑跳持续时间（秒）")]
        public float pounceDuration = 0.8f;

        [Tooltip("扑跳到顶点时的额外高度（相对于起点和终点线性高度）")]
        public float pounceApexHeight = 1.2f;

        [Tooltip("落地后短时间内可判断并触发攻击（秒）")]
        public float landingAttackDelay = 0.05f;

        [Tooltip("扑跳冷却时间（秒）")]
        public float pounceCooldown = 1.5f;

        // 内部状态
        private bool _isPouncing;
        private Vector3 _pounceStart;
        private Vector3 _pounceTarget;
        private float _pounceTimer;

        // animator parameter hashes
        private int _animIsPouncing; // 使用Bool代替多个Trigger
        private int _animPounceProgress; // 跳跃进度 0-1

        // 记录上次攻击时间，用于冷却
        private float _lastAttackTime = -100f;
        
        // 记录上次扑跳时间，用于扑跳冷却
        private float _lastPounceTime = -100f;

        // 2.19 静影：补充猫死亡后事件
        public UnityEvent OnCatDeath;

        protected override void Awake()
        {
            base.Awake();

            // 确保有碰撞体（防止穿地）
            if (GetComponent<Collider>() == null)
            {
                var capsule = gameObject.AddComponent<CapsuleCollider>();
                capsule.height = 1f;
                capsule.radius = 0.3f;
                capsule.center = new Vector3(0, 0.5f, 0);
            }

            // 确保刚体设置正确
            if (_rigidbody != null)
            {
                _rigidbody.useGravity = true;
                _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            }

            // 猫不会像普通敌人行走，只通过扑跳接近玩家
            PatrolSpeed = 0f;
            chaseSpeed = 0f;

            // 禁用 NavMeshAgent 以免和手动移动冲突
            if (_navAgent != null)
            {
                _navAgent.enabled = false;
            }

            // 初始化动画参数哈希
            _animIsPouncing = Animator.StringToHash("IsPouncing");
            _animPounceProgress = Animator.StringToHash("PounceProgress");
        }

        protected override void Update()
        {
            // 如果处于扑跳中，手动推进扑跳轨迹
            if (_isPouncing)
            {
                UpdatePounce();
                return;
            }

            // 在非扑跳状态下，利用基类更新（状态机、无敌帧等）
            base.Update();

            // 如果正在播放攻击动画或血量为0（死亡），跳过AI决策
            if (_isAttacking || currentHealth <= 0)
            {
                return;
            }

            if (_player == null) return;

            float dist = GetDistanceToPlayer();

            // 优先：若玩家在近距离攻击范围，立即发动攻击（无冷却）
            if (dist <= attackRange)
            {
                PerformAttack();
                return;
            }

            // 若玩家在扑跳触发区间（不太近也不太远），且扑跳冷却完成，开始扑跳
            if (dist <= pounceDistance && dist >= minPounceDistance && Time.time - _lastPounceTime >= pounceCooldown)
            {
                StartPounceToPlayer();
            }
        }

        /// <summary>
        /// 开始扑跳：记录起点/目标，设置状态并触发起跳动画
        /// </summary>
        private void StartPounceToPlayer()
        {
            if (_player == null) return;

            // 记录扑跳时间用于冷却
            _lastPounceTime = Time.time;

            // 计算目标位置：锁定玩家当前水平位置（保留目标的y以便落地于地面高度）
            _pounceStart = transform.position;
            _pounceTarget = _player.position;

            // 保持目标与起点同一高度（着陆在地面层面）
            _pounceTarget.y = _pounceStart.y;

            _pounceTimer = 0f;
            _isPouncing = true;

            // 让刚体不受物理驱动（手动控制位置），关闭重力并停止所有速度
            if (_rigidbody != null)
            {
                _rigidbody.velocity = Vector3.zero;
                _rigidbody.useGravity = false;
                _rigidbody.isKinematic = true;
            }

            // 面向目标
            Vector3 dir = (_pounceTarget - transform.position);
            dir.y = 0f;
            if (dir != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(dir);
            }

            // 设置跳跃状态为true，让Animator进入跳跃状态
            if (_animator != null)
            {
                _animator.SetBool(_animIsPouncing, true);
                _animator.SetFloat(_animPounceProgress, 0f);
            }
        }

        /// <summary>
        /// 每帧推进扑跳轨迹，使用二次贝塞尔（抛物线近似）
        /// </summary>
        private void UpdatePounce()
        {
            _pounceTimer += Time.deltaTime;
            float t = Mathf.Clamp01(_pounceTimer / Mathf.Max(0.0001f, pounceDuration));

            // 线性位置
            Vector3 basePos = Vector3.Lerp(_pounceStart, _pounceTarget, t);

            // 抛物线高度：4*h*t*(1-t) 在 t=0.5 时达到最大值 h
            float heightOffset = 4f * pounceApexHeight * t * (1f - t);

            Vector3 pos = basePos + Vector3.up * heightOffset;
            transform.position = pos;

            // 更新动画进度参数，让Animator可以根据进度切换不同动画片段
            if (_animator != null)
            {
                _animator.SetFloat(_animPounceProgress, t);
            }

            // 结束时处理落地逻辑
            if (t >= 1f)
            {
                EndPounce();
            }
        }

        private void EndPounce()
        {
            _isPouncing = false;

            // 恢复刚体物理并开启重力
            if (_rigidbody != null)
            {
                _rigidbody.isKinematic = false;
                _rigidbody.useGravity = true;
                _rigidbody.velocity = Vector3.zero;
            }

            // 退出跳跃状态
            if (_animator != null)
            {
                _animator.SetBool(_animIsPouncing, false);
                _animator.SetFloat(_animPounceProgress, 0f);
            }

            // 在落地后短延迟内判断是否触发攻击（如果玩家仍在攻击范围内）
            if (_player != null)
            {
                float dist = GetDistanceToPlayer();
                if (dist <= attackRange && Time.time - _lastAttackTime >= AttackCooldown)
                {
                    // 延迟一点以便与落地动画衔接
                    Invoke(nameof(DelayedAttackAfterLanding), landingAttackDelay);
                }
            }
        }

        private void DelayedAttackAfterLanding()
        {
            if (_player != null)
            {
                float dist = GetDistanceToPlayer();
                if (dist <= attackRange)
                {
                    PerformAttack();
                }
            }
        }

        public override void ChasePlayer()
        {
            
        }

        /// <summary>
        /// 重写 PerformAttack 以记录攻击时间并使用基类动画触发
        /// </summary>
        public override void PerformAttack()
        {
            // 记录攻击时间（在攻击开始时就记录，避免攻击spam）
            _lastAttackTime = Time.time;

            // 面向玩家
            if (_player != null)
            {
                Vector3 dir = (_player.position - transform.position);
                dir.y = 0f;
                if (dir != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(dir);
                }
            }

            // 调用基类以处理动画触发、音效等
            base.PerformAttack();

            // 立即尝试对玩家造成伤害（如果在范围内）——如果项目使用动画事件来造成伤害，可以移除此调用
            // if (_playerStats != null && GetDistanceToPlayer() <= attackRange)
            // {
            //     AttackPlayer(_playerStats);
            // }
            
            // 确保攻击状态在一定时间后自动重置（防止动画事件未触发导致卡住）
            // 这是一个安全措施，正常情况下应该由动画事件 OnAttackEnd() 重置
            Invoke(nameof(ResetAttackState), 1.0f);
        }
        
        /// <summary>
        /// 重置攻击状态（安全措施，防止动画事件丢失导致永久卡住）
        /// </summary>
        private void ResetAttackState()
        {
            if (_isAttacking)
            {
                Debug.LogWarning($"[{gameObject.name}] 攻击状态被强制重置（可能是动画事件未正确设置）");
                _isAttacking = false;
                if (_navAgent != null && _navAgent.enabled)
                {
                    _navAgent.isStopped = false;
                }
            }
        }

        /// <summary>
        /// 重写动画事件回调：攻击结束
        /// 取消安全措施的Invoke，让动画事件正常处理
        /// </summary>
        public new void OnAttackEnd()
        {
            // 取消延迟重置（因为动画事件正确触发了）
            CancelInvoke(nameof(ResetAttackState));
            
            // 调用基类的攻击结束逻辑
            base.OnAttackEnd();
        }

        // 可视化范围
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, pounceDistance);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, minPounceDistance);
        }

        // 2.19 静影：增加猫死亡事件
        public override void OnDeath()
        {
            OnCatDeath?.Invoke();
            // 最后调用基类方法销毁对象
            base.OnDeath();
        }
    }
}