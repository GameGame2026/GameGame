using UnityEngine;

namespace _Projects.GamePlay
{
    /// <summary>
    /// 茶壶敌人（远程）
    /// 特殊机制：
    /// - 攻击方式：吐出抛物线运动泡泡
    /// - 泡泡机制：可被玩家贴点，贴点后泡泡原路返回
    /// - 伤害判定：泡泡命中玩家造成伤害
    /// - 站桩单位：不会移动，仅有攻击/受击/死亡动画
    /// </summary>
    public class TeapotEnemy : EnemyBase
    {
        [Header("茶壶特殊设置")] [Tooltip("泡泡预制体")] public GameObject bubblePrefab;

        [Tooltip("泡泡发射点")] public Transform firePoint;

        [Tooltip("泡泡发射力度")] public float bubbleForce = 10f;

        [Tooltip("泡泡发射角度")] public float bubbleAngle = 45f;

        [Tooltip("泡泡伤害")] public float bubbleDamage = 1f;

        protected override void Awake()
        {
            base.Awake();

            // 茶壶是远程敌人，攻击范围较大
            attackRange = 8f;

            // 站桩：不移动
            PatrolSpeed = 0f;
            chaseSpeed = 0f;

            // 如果有 NavMeshAgent，禁用以防止自动导航
            if (_navAgent != null)
            {
                _navAgent.enabled = false;
            }
        }

        /// <summary>
        /// 执行攻击 - 发射泡泡
        /// </summary>
        public override void PerformAttack()
        {
            base.PerformAttack();

            Debug.Log($"[Teapot] {gameObject.name} 发射泡泡");

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
        
        public override void AttackPlayer(PlayerStats player)
        {
            // 茶壶不直接攻击，通过泡泡造成伤害
        }

        
        public override void MoveTo(Vector3 destination, float speed)
        {
            // 保持原地不动，停止刚体/agent并确保动画速度为0
            if (_navAgent != null && _navAgent.enabled)
            {
                _navAgent.isStopped = true;
                _navAgent.ResetPath();
            }
            if (_rigidbody != null)
            {
                _rigidbody.velocity = Vector3.zero;
            }
            if (_animator != null)
            {
                _animator.SetFloat(_animSpeed, 0f);
            }
        }

        /// <summary>
        /// 追击玩家时只朝向玩家，不移动
        /// </summary>
        public override void ChasePlayer()
        {
            // 面向玩家，但不移动
            LookAtPlayer();
        }

        /// <summary>
        /// 友方行为对茶壶无效
        /// </summary>
        public override void FriendlyBehavior()
        {
            // 不执行跟随或移动逻辑
        }
    }
}
