using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Projects.GamePlay
{
    public class DisposableObject : MonoBehaviour
    {
        [Header("贴附设置")]
        [Tooltip("要贴附的Point")]
        public GameObject point;
        
        [Tooltip("是否已被贴附")]
        public bool IsAttached { get; protected set; }
        
        [Header("点数设置")]
        [Tooltip("贴附所需的点数")]
        public int pointCost = 1;
        
        public PlayerStats playerStats;

        private void Awake()
        {
            point.SetActive(false);
        }


        // 确保 playerStats 可用
        private PlayerStats GetPlayerStats()
        {
            if (playerStats != null) return playerStats;
            playerStats = FindObjectOfType<PlayerStats>();
            return playerStats;
        }

        /// <summary>
        /// 检查玩家是否有足够点数（不会修改点数）
        /// 返回 true 表示足够或无法检测到 PlayerStats（默认允许），false 表示点数不足
        /// </summary>
        public bool HasEnoughPoints(PlayerStats stats = null)
        {
            var s = stats ?? GetPlayerStats();
            if (s == null)
            {
                Debug.LogWarning($"[{gameObject.name}] 未找到 PlayerStats，HasEnoughPoints 返回 true（允许贴附）。");
                return true;
            }
            return s.Points >= pointCost;
        }

        /// <summary>
        /// 尝试消耗点数，成功返回 true，失败返回 false
        /// 如果无法找到 PlayerStats 则返回 true
        /// </summary>
        public bool TryConsumePoints(PlayerStats stats = null)
        {
            var s = stats ?? GetPlayerStats();
            if (s == null)
            {
                Debug.LogWarning($"[{gameObject.name}] 未找到 PlayerStats，TryConsumePoints 不执行消耗并返回 true。");
                return true;
            }

            if (s.Points < pointCost)
            {
                return false;
            }

            s.RemovePoints(pointCost);
            return true;
        }

        /// <summary>
        /// 返还点数
        /// </summary>
        public void RefundPoints(PlayerStats stats = null)
        {
            var s = stats ?? GetPlayerStats();
            if (s == null)
            {
                Debug.LogWarning($"[{gameObject.name}] 未找到 PlayerStats，RefundPoints 未执行。");
                return;
            }

            s.AddPoints(pointCost);
        }

        public virtual void ChangeState()
        {
            if (IsAttached) return;
            
            // 先尝试消耗点数，失败则返回
            if (!TryConsumePoints())
            {
                Debug.Log($"[{gameObject.name}] 点数不足，无法贴附（需要 {pointCost} 点）。");
                return;
            }

            IsAttached = true;
            
            if (point != null)
            {
                point.SetActive(true);
            }
            
            Debug.Log($"{gameObject.name} 已贴上prefab");
        }
        
        public virtual void Recycle()
        {
            if (!IsAttached) return;

            // 回收时返还点数
            RefundPoints();

            IsAttached = false;
            
            if (point != null)
            {
                point.SetActive(false);
            }
            
            Debug.Log($"{gameObject.name} 已回收prefab，恢复原状");
        }
    }
}
