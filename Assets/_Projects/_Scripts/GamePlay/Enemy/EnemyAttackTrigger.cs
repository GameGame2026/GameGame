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
        public GameObject attackRange;

        private void Awake()
        {
            // 获取敌人的 Enemy 组件
            _enemy = GetComponentInParent<EnemyBase>();
            
            attackRange.SetActive(false);
        }

        /// <summary>
        /// 启用攻击判定 - 在攻击动画开始时调用
        /// </summary>
        public void EnableAttack()
        {
            attackRange.SetActive(true);
        }

        /// <summary>
        /// 禁用攻击判定 - 在攻击动画结束时调用
        /// </summary>
        public void DisableAttack()
        {
            attackRange.SetActive(false);
        }

        private void OnTriggerStay(Collider other)
        {
            
            PlayerStats player = other.GetComponent<PlayerStats>();
            if (player != null && _enemy != null)
            {
                _enemy.AttackPlayer(player);
            }
        }
    }
}

