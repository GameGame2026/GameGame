using UnityEngine;

namespace _Projects.GamePlay
{
    /// <summary>
    /// 鬼魂敌人
    /// 特殊机制：
    /// - 初始状态：半透明、不可被攻击
    /// - 贴点后：变为不透明、可被攻击
    /// - 特殊属性：空中漂浮，无视地面虚实状态
    /// </summary>
    public class GhostEnemy : EnemyBase
    {
        [Header("鬼魂特殊设置")]
        [Tooltip("漂浮高度")]
        public float floatHeight = 1.5f;
        
        [Tooltip("漂浮波动幅度")]
        public float floatAmplitude = 0.3f;
        
        [Tooltip("漂浮波动频率")]
        public float floatFrequency = 1f;
        
        [Tooltip("半透明度（初始状态）")]
        [Range(0f, 1f)]
        public float ghostAlpha = 0.4f;

        [Header("视觉效果")]
        [Tooltip("半透明材质")]
        public Material ghostMaterial;
        
        [Tooltip("实体材质")]
        public Material solidMaterial;

        private Renderer _renderer;
        private float _floatTimer;
        private float _baseHeight;

        protected override void Awake()
        {
            base.Awake();
            _renderer = GetComponentInChildren<Renderer>();
            
            // 初始状态：不可被攻击
            CanBeAttacked = false;
        }

        protected override void Start()
        {
            base.Start();
            
            // 设置初始漂浮高度
            _baseHeight = transform.position.y + floatHeight;
            
            // 设置初始半透明状态
            SetGhostMode(true);
            
            // 禁用NavMeshAgent（鬼魂漂浮移动）
            if (_navAgent != null)
            {
                _navAgent.enabled = false;
            }
        }

        /// <summary>
        /// 被贴点 - 变为实体可攻击
        /// </summary>
        public override void OnPointAttached()
        {
            base.OnPointAttached();
            
            // 变为实体
            SetGhostMode(false);
            CanBeAttacked = true;
            
            Debug.Log($"[Ghost] {gameObject.name} 变为实体，可被攻击");
        }

        /// <summary>
        /// 点被回收 - 恢复半透明状态
        /// </summary>
        public override void OnPointDetached()
        {
            base.OnPointDetached();
            
            // 恢复半透明
            SetGhostMode(true);
            CanBeAttacked = false;
            
            Debug.Log($"[Ghost] {gameObject.name} 恢复半透明，不可被攻击");
        }

        /// <summary>
        /// 设置鬼魂模式（半透明/实体）
        /// </summary>
        private void SetGhostMode(bool isGhost)
        {
            if (_renderer == null) return;
            
            if (isGhost && ghostMaterial != null)
            {
                _renderer.material = ghostMaterial;
            }
            else if (!isGhost && solidMaterial != null)
            {
                _renderer.material = solidMaterial;
            }
            
            // 直接修改透明度
            if (_renderer.material.HasProperty("_Color"))
            {
                Color color = _renderer.material.color;
                color.a = isGhost ? ghostAlpha : 1f;
                _renderer.material.color = color;
            }
        }

        protected override void Update()
        {
            base.Update();
            
            // 漂浮效果
            UpdateFloat();
        }

        /// <summary>
        /// 更新漂浮效果
        /// </summary>
        private void UpdateFloat()
        {
            _floatTimer += Time.deltaTime * floatFrequency;
            
            // 使用正弦波实现上下漂浮
            float newY = _baseHeight + Mathf.Sin(_floatTimer * Mathf.PI * 2f) * floatAmplitude;
            
            Vector3 pos = transform.position;
            pos.y = newY;
            transform.position = pos;
        }

        /// <summary>
        /// 重写移动方法 - 鬼魂漂浮移动
        /// </summary>
        public override void MoveTo(Vector3 destination, float speed)
        {
            // 忽略Y轴，保持漂浮
            destination.y = transform.position.y;
            
            Vector3 direction = (destination - transform.position).normalized;
            
            if (direction != Vector3.zero)
            {
                transform.position += direction * speed * Time.deltaTime;
                
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            
            // 更新动画
            if (_animator != null)
            {
                _animator.SetFloat(_animSpeed, speed > 0 ? 1f : 0f);
            }
        }

        /// <summary>
        /// 追击玩家 - 鬼魂版本
        /// </summary>
        public override void ChasePlayer()
        {
            if (_player != null)
            {
                Vector3 targetPos = _player.position;
                targetPos.y = transform.position.y; // 保持漂浮高度
                MoveTo(targetPos, chaseSpeed);
            }
        }

        /// <summary>
        /// 重写巡逻点获取 - 忽略NavMesh
        /// </summary>
        public override Vector3 GetRandomPatrolPoint()
        {
            Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
            randomDirection.y = 0; // 只在水平面移动
            randomDirection += _spawnPosition;
            randomDirection.y = _baseHeight;
            
            return randomDirection;
        }
    }
}

