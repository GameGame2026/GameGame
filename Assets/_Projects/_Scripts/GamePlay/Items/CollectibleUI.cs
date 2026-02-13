using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace _Projects._Scripts.GamePlay.Items
{
    /// <summary>
    /// 收集物UI - 显示当前收集数量
    /// </summary>
    public class CollectibleUI : MonoBehaviour
    {
        [Header("UI元素")]
        [SerializeField] private TextMeshProUGUI attackBoostText;
        [SerializeField] private TextMeshProUGUI healthBoostText;

        [Header("图标（可选）")]
        [SerializeField] private Image attackBoostIcon;
        [SerializeField] private Image healthBoostIcon;

        [Header("显示设置")]
        [SerializeField] private bool hideWhenZero;

        private void Start()
        {
            // 初始化显示
            UpdateCollectionCount(0, 0);
        }

        /// <summary>
        /// 更新收集数量显示
        /// </summary>
        public void UpdateCollectionCount(int attackCount, int healthCount)
        {
            // 更新攻击提升文本
            if (attackBoostText != null)
            {
                attackBoostText.text = $"⚡ ×{attackCount}";
                if (hideWhenZero)
                {
                    attackBoostText.gameObject.SetActive(attackCount > 0);
                }
            }

            // 更新血量提升文本
            if (healthBoostText != null)
            {
                healthBoostText.text = $"❤ ×{healthCount}";
                if (hideWhenZero)
                {
                    healthBoostText.gameObject.SetActive(healthCount > 0);
                }
            }

            // 更新图标显示（如果有）
            if (attackBoostIcon != null && hideWhenZero)
            {
                attackBoostIcon.gameObject.SetActive(attackCount > 0);
            }

            if (healthBoostIcon != null && hideWhenZero)
            {
                healthBoostIcon.gameObject.SetActive(healthCount > 0);
            }
        }
    }
}

