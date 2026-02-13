using UnityEngine;

namespace _Projects.GamePlay
{
    public class DeathZone : MonoBehaviour
    {
        [Header("设置")]
        [Tooltip("是否在Scene视图中显示红色半透明区域")]
        [SerializeField] private bool showGizmos = true;
        
        [Tooltip("是否启用")]
        [SerializeField] private bool isEnabled = true;
        
        
        private void OnTriggerEnter(Collider other)
        {
            if (!isEnabled) return;
            
            // 检查是否是玩家
            if (other.CompareTag("Player"))
            {
                PlayerStats playerStats = other.GetComponent<PlayerStats>();
                if (playerStats != null)
                {
                    KillPlayer(playerStats);
                }
                else
                {
                    Debug.LogWarning($"[DeathZone] 玩家 {other.name} 没有 PlayerStats 组件！");
                }
            }
        }
        
        private void KillPlayer(PlayerStats playerStats)
        {
            Debug.Log($"[DeathZone] 玩家触碰死亡区域：{gameObject.name}");
            // 造成足够致命的伤害
            playerStats.TakeDamage(999999f);
        }
        
        // 在Scene视图中显示红色半透明区域
        private void OnDrawGizmos()
        {
            if (!showGizmos) return;
            
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Collider col = GetComponent<Collider>();
            
            if (col != null)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                
                if (col is BoxCollider box)
                {
                    Gizmos.DrawCube(box.center, box.size);
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(box.center, box.size);
                }
                else if (col is SphereCollider sphere)
                {
                    Gizmos.DrawSphere(sphere.center, sphere.radius);
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(sphere.center, sphere.radius);
                }
                else if (col is CapsuleCollider capsule)
                {
                    // Unity没有DrawCapsule，用球体近似
                    Gizmos.DrawSphere(capsule.center, capsule.radius);
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(capsule.center, capsule.radius);
                }
            }
        }
    }
}

