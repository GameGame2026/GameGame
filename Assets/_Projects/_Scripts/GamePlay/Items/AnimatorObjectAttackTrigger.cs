using UnityEngine;

namespace _Projects.GamePlay
{
    /// <summary>
    /// AnimatorObject 攻击触发器 - 挂载在攻击判定区域的Collider上
    /// 用于检测攻击命中的玩家
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class AnimatorObjectAttackTrigger : MonoBehaviour
    {
        private AnimatorObject _animatorObject;

        private void Awake()
        {
            // 获取父物体的 AnimatorObject 组件
            _animatorObject = GetComponentInParent<AnimatorObject>();
            
            if (_animatorObject == null)
            {
                Debug.LogWarning($"[AnimatorObjectAttackTrigger] 未找到 AnimatorObject 组件！请确保此脚本在 AnimatorObject 的子物体上", this);
            }

            // 确保 Collider 是 Trigger
            Collider col = GetComponent<Collider>();
            if (col != null && !col.isTrigger)
            {
                col.isTrigger = true;
                Debug.Log($"[AnimatorObjectAttackTrigger] 已自动将 Collider 设置为 Trigger", this);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            // 检测到玩家时攻击
            PlayerStats player = other.GetComponent<PlayerStats>();
            if (player != null && _animatorObject != null)
            {
                _animatorObject.AttackPlayer(player);
            }
        }
    }
}

