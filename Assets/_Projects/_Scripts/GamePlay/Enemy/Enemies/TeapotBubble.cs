using _Projects.GamePlay;
using UnityEngine;

    /// <summary>
    /// 茶壶泡泡 - 抛物线运动，可被贴点后原路返回
    /// </summary>
    public class TeapotBubble : DisposableObject
    {
        [Header("泡泡属性")]
        public float damage = 1f;
        public float lifetime = 5f;
        public bool IsReturning { get; private set; }

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

        public override void ChangeState()
        {
            base.ChangeState();
            OnPointAttached();
        }
        

        /// <summary>
        /// 被贴点 - 开始原路返回
        /// </summary>
        public void OnPointAttached()
        {
            // if (IsAttached) return;
            
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

                if (IsAttached)
                {
                    RefundPoints(playerStats);
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
                if (teapot != null && teapot == _owner && other.CompareTag("Enemy"))
                {
                    teapot.TakeDamage(damage);
                    if (IsAttached)
                    {
                        RefundPoints(playerStats);
                    }
                    Destroy(gameObject);
                    return;
                }
            }
            else
            {
                // 正常模式下检测玩家
                PlayerStats player = other.GetComponent<PlayerStats>();
                if (player != null && other.CompareTag("Player"))
                {
                    player.TakeDamage(damage);
                    if (IsAttached)
                    {
                        RefundPoints(playerStats);
                    }
                    Destroy(gameObject);
                }
            }
        }
    }

