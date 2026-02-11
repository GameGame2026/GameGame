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

        private void Awake()
        {
            // 获取敌人的 Enemy 组件
            _enemy = GetComponentInParent<EnemyBase>();
            
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

