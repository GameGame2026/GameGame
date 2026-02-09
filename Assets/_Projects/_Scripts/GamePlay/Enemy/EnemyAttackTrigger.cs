using UnityEngine;

namespace _Projects.GamePlay
{
    /// <summary>
    /// 敌人攻击触发器 - 挂载在敌人攻击判定区域的Collider上
    /// 用于检测攻击命中的玩家
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class EnemyAttackTrigger : MonoBehaviour
    {
        private EnemyBase _enemy;
        private Collider _collider;

        private void Awake()
        {
            // 获取敌人的 Enemy 组件
            _enemy = GetComponentInParent<EnemyBase>();
            if (_enemy == null)
            {
                Debug.LogWarning("[EnemyAttackTrigger] 未找到父对象上的 Enemy 组件，请确保该触发器为敌人对象的子对象。", this);
            }

            _collider = GetComponent<Collider>();
            if (_collider == null)
            {
                Debug.LogError("[EnemyAttackTrigger] 缺少 Collider 组件", this);
                return;
            }

            // 确保是触发器
            _collider.isTrigger = true;

            // 默认禁用，只在动画事件中启用
            _collider.enabled = false;
        }

        /// <summary>
        /// 启用攻击判定 - 在攻击动画开始时调用
        /// </summary>
        public void EnableAttack()
        {
            if (_collider != null) _collider.enabled = true;
            Debug.Log("[EnemyAttackTrigger] 攻击判定启用", this);
        }

        /// <summary>
        /// 禁用攻击判定 - 在攻击动画结束时调用
        /// </summary>
        public void DisableAttack()
        {
            if (_collider != null) _collider.enabled = false;
            Debug.Log("[EnemyAttackTrigger] 攻击判定禁用", this);
        }

        private void OnTriggerEnter(Collider other)
        {
            // 只在触发器启用时处理
            if (_collider == null || !_collider.enabled) return;

            // 检测是否碰撞到玩家
            PlayerStats player = other.GetComponent<PlayerStats>();
            if (player != null && _enemy != null)
            {
                // 对玩家造成伤害
                _enemy.AttackPlayer(player);
            }
        }
    }
}

