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
        public GameObject attackRange;
        
        private void Awake()
        {
            _playerStats = GetComponentInParent<PlayerStats>();
            
            attackRange.SetActive(false);
            
        }
        public void EnableAttack()
        {
            attackRange.SetActive(true);
        }
        
        public void DisableAttack()
        {
            attackRange.SetActive(false);
        }
        
        private void OnTriggerStay(Collider other)
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

