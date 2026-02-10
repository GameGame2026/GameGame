using UnityEngine;

namespace _Projects.GamePlay
{
    /// <summary>
    /// 茶壶敌人（远程）
    /// 特殊机制：
    /// - 攻击方式：吐出抛物线运动泡泡
    /// - 泡泡机制：可被玩家贴点，贴点后泡泡原路返回
    /// - 伤害判定：泡泡命中玩家造成伤害
    /// </summary>
    public class TeapotEnemy : EnemyBase
    {
        [Header("茶壶特殊设置")]
        [Tooltip("泡泡预制体")]
        public GameObject bubblePrefab;
        
        [Tooltip("泡泡发射点")]
        public Transform firePoint;
        
        [Tooltip("泡泡发射力度")]
        public float bubbleForce = 10f;
        
        [Tooltip("泡泡发射角度")]
        public float bubbleAngle = 45f;
        
        [Tooltip("泡泡伤害")]
        public float bubbleDamage = 1f;

        protected override void Awake()
        {
            base.Awake();
            
            // 茶壶是远程敌人，攻击范围较大
            attackRange = 8f;
        }

        /// <summary>
        /// 执行攻击 - 发射泡泡
        /// </summary>
        public override void PerformAttack()
        {
            Debug.Log($"[Teapot] {gameObject.name} 发射泡泡");

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

            // 发射泡泡
            FireBubble();
        }

        /// <summary>
        /// 发射泡泡
        /// </summary>
        private void FireBubble()
        {
            if (bubblePrefab == null)
            {
                Debug.LogWarning($"[Teapot] 泡泡预制体未设置");
                return;
            }

            Transform spawnPoint = firePoint != null ? firePoint : transform;
            
            // 创建泡泡
            GameObject bubble = Object.Instantiate(bubblePrefab, spawnPoint.position, Quaternion.identity);
            
            // 获取或添加泡泡组件
            TeapotBubble bubbleComponent = bubble.GetComponent<TeapotBubble>();
            if (bubbleComponent == null)
            {
                bubbleComponent = bubble.AddComponent<TeapotBubble>();
            }
            
            // 设置泡泡属性
            bubbleComponent.Initialize(this, _player, bubbleDamage, bubbleForce, bubbleAngle);
        }

        /// <summary>
        /// 重写攻击判定 - 茶壶通过泡泡攻击
        /// </summary>
        public override void AttackPlayer(PlayerStats player)
        {
            // 茶壶不直接攻击，通过泡泡造成伤害
        }
    }

    /// <summary>
    /// 茶壶泡泡 - 抛物线运动，可被贴点后原路返回
    /// </summary>
    public class TeapotBubble : MonoBehaviour
    {
        [Header("泡泡属性")]
        public float damage = 1f;
        public float lifetime = 5f;
        public bool IsReturning { get; private set; }
        // 使用统一的命名：IsAttached 表示是否被贴点
        public bool IsAttached { get; private set; }

        private TeapotEnemy _owner;
        private Transform _target;
        private Rigidbody _rigidbody;
        
        // 路径记录（用于返回）
        private Vector3[] _pathPoints;
        private int _pathIndex;
        private float _recordInterval = 0.1f;
        private float _recordTimer;
        private int _maxPathPoints = 100;
        private int _currentPathIndex;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            if (_rigidbody == null)
            {
                _rigidbody = gameObject.AddComponent<Rigidbody>();
            }
            
            _pathPoints = new Vector3[_maxPathPoints];
        }

        /// <summary>
        /// 初始化泡泡
        /// </summary>
        public void Initialize(TeapotEnemy owner, Transform target, float dmg, float force, float angle)
        {
            _owner = owner;
            _target = target;
            damage = dmg;

            // 计算发射方向（抛物线）
            if (_target != null)
            {
                Vector3 direction = (_target.position - transform.position).normalized;
                direction.y = 0;
                
                // 添加抛物线角度
                Vector3 launchDirection = Quaternion.AngleAxis(-angle, transform.right) * direction;
                
                // 应用力
                if (_rigidbody != null)
                {
                    _rigidbody.AddForce(launchDirection * force, ForceMode.Impulse);
                }
            }

            // 记录初始位置
            _pathPoints[0] = transform.position;
            _currentPathIndex = 1;
            
            // 设置生命周期
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            // 记录路径点
            if (!IsReturning)
            {
                _recordTimer += Time.deltaTime;
                if (_recordTimer >= _recordInterval && _currentPathIndex < _maxPathPoints)
                {
                    _pathPoints[_currentPathIndex] = transform.position;
                    _currentPathIndex++;
                    _recordTimer = 0f;
                }
            }
            else
            {
                // 返回模式：沿路径返回
                ReturnAlongPath();
            }
        }

        /// <summary>
        /// 被贴点 - 开始原路返回
        /// </summary>
        public void OnPointAttached()
        {
            if (IsAttached) return;
            
            IsAttached = true;
            IsReturning = true;
            _pathIndex = _currentPathIndex - 1;
            
            // 停止物理运动
            if (_rigidbody != null)
            {
                _rigidbody.isKinematic = true;
            }
            
            Debug.Log("[TeapotBubble] 泡泡被贴点，开始返回");
        }

        /// <summary>
        /// 沿路径返回
        /// </summary>
        private void ReturnAlongPath()
        {
            if (_pathIndex < 0)
            {
                // 返回到起点，攻击茶壶
                if (_owner != null)
                {
                    _owner.TakeDamage(damage);
                }
                Destroy(gameObject);
                return;
            }

            // 移动到下一个路径点
            Vector3 targetPoint = _pathPoints[_pathIndex];
            transform.position = Vector3.MoveTowards(transform.position, targetPoint, 15f * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPoint) < 0.1f)
            {
                _pathIndex--;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // 返回模式下检测茶壶
            if (IsReturning)
            {
                TeapotEnemy teapot = other.GetComponent<TeapotEnemy>();
                if (teapot != null && teapot == _owner)
                {
                    teapot.TakeDamage(damage);
                    Destroy(gameObject);
                    return;
                }
            }
            else
            {
                // 正常模式下检测玩家
                PlayerStats player = other.GetComponent<PlayerStats>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                    Destroy(gameObject);
                }
            }
        }
    }
}
