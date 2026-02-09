using UnityEngine;

namespace _Projects.GamePlay
{
    /// <summary>
    /// 玩家攻击触发器 - 挂载在攻击判定区域的Collider上
    /// 用于检测攻击命中的敌人
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PlayerAttackTrigger : MonoBehaviour
    {
        private PlayerStats _playerStats;
        private Collider _collider;
        
        private void Awake()
        {
            // 获取玩家的PlayerStats组件
            _playerStats = GetComponentInParent<PlayerStats>();
            _collider = GetComponent<Collider>();
            
            // 确保是触发器
            _collider.isTrigger = true;
            
            // 默认禁用，只在攻击时启用
            _collider.enabled = false;
        }
        
        /// <summary>
        /// 启用攻击判定 - 在攻击动画开始时调用
        /// </summary>
        public void EnableAttack()
        {
            _collider.enabled = true;
            Debug.Log("[PlayerAttackTrigger] 攻击判定启用");
        }
        
        /// <summary>
        /// 禁用攻击判定 - 在攻击动画结束时调用
        /// </summary>
        public void DisableAttack()
        {
            _collider.enabled = false;
            Debug.Log("[PlayerAttackTrigger] 攻击判定禁用");
        }
        
        private void OnTriggerEnter(Collider other)
        {
            // 检测是否碰撞到敌人
            EnemyBase enemy = other.GetComponent<EnemyBase>();
            if (enemy != null && _playerStats != null)
            {
                // 对敌人造成伤害
                _playerStats.AttackEnemy(enemy);
            }
        }
    }
}

