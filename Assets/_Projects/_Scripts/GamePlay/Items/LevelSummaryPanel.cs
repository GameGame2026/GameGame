using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using _Projects._Scripts.SceneManagement;

namespace _Projects._Scripts.GamePlay.Items
{
    /// <summary>
    /// 关卡结算面板 - 显示收集统计和属性提升
    /// </summary>
    public class LevelSummaryPanel : MonoBehaviour
    {
        [Header("面板引用")]
        [SerializeField] private GameObject summaryPanel;

        [Header("文本显示")]
        [SerializeField] private TextMeshProUGUI attackBoostCountText;
        [SerializeField] private TextMeshProUGUI healthBoostCountText;
        [SerializeField] private TextMeshProUGUI attackIncreaseText;
        [SerializeField] private TextMeshProUGUI healthIncreaseText;
        [SerializeField] private TextMeshProUGUI newAttackText;
        [SerializeField] private TextMeshProUGUI newHealthText;

        [Header("按钮")]
        [SerializeField] private Button continueButton;

        [Header("下一关设置")]
        [SerializeField] private string nextSceneName = "Scene2";

        [Header("动画设置")]
        [SerializeField] private float displayDelay = 0.5f;
        [SerializeField] private float textAnimationDuration = 1f;

        private CollectionData _levelData;
        private PlayerStats _playerStats;

        private void Awake()
        {
            // 隐藏面板
            if (summaryPanel != null)
            {
                summaryPanel.SetActive(false);
            }

            // 绑定按钮
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueClicked);
            }
        }

        private void Start()
        {
            // 查找玩家
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerStats = player.GetComponent<PlayerStats>();
            }
        }

        /// <summary>
        /// 显示结算面板
        /// </summary>
        public void ShowSummary()
        {
            // 获取收集数据
            if (CollectibleManager.Instance != null)
            {
                _levelData = CollectibleManager.Instance.GetCurrentLevelData();
                
                // 应用提升效果
                CollectibleManager.Instance.ApplyCollectionBoosts();
            }
            else
            {
                Debug.LogWarning("[LevelSummaryPanel] 未找到 CollectibleManager！");
                _levelData = new CollectionData();
            }

            // 显示面板
            if (summaryPanel != null)
            {
                summaryPanel.SetActive(true);
            }

            // 暂停游戏
            Time.timeScale = 0f;

            // 显示数据
            StartCoroutine(DisplaySummaryCoroutine());
        }

        /// <summary>
        /// 隐藏结算面板
        /// </summary>
        public void HideSummary()
        {
            if (summaryPanel != null)
            {
                summaryPanel.SetActive(false);
            }

            // 恢复游戏时间
            Time.timeScale = 1f;
        }

        /// <summary>
        /// 显示统计数据（带动画）
        /// </summary>
        private System.Collections.IEnumerator DisplaySummaryCoroutine()
        {
            yield return new WaitForSecondsRealtime(displayDelay);

            // 显示收集数量
            if (attackBoostCountText != null)
            {
                attackBoostCountText.text = $"⚡ 攻击提升收集: ×{_levelData.attackBoostCount}";
            }

            if (healthBoostCountText != null)
            {
                healthBoostCountText.text = $"❤ 生命提升收集: ×{_levelData.healthBoostCount}";
            }

            yield return new WaitForSecondsRealtime(0.3f);

            // 显示提升数值
            if (_playerStats != null)
            {
                if (attackIncreaseText != null)
                {
                    if (_levelData.totalAttackIncrease > 0)
                    {
                        attackIncreaseText.text = $"攻击力 +{_levelData.totalAttackIncrease:F2}";
                        attackIncreaseText.color = Color.green;
                    }
                    else
                    {
                        attackIncreaseText.text = "攻击力 无变化";
                        attackIncreaseText.color = Color.gray;
                    }
                }

                if (healthIncreaseText != null)
                {
                    if (_levelData.totalHealthIncrease > 0)
                    {
                        healthIncreaseText.text = $"最大生命 +{_levelData.totalHealthIncrease:F2}";
                        healthIncreaseText.color = Color.green;
                    }
                    else
                    {
                        healthIncreaseText.text = "最大生命 无变化";
                        healthIncreaseText.color = Color.gray;
                    }
                }

                yield return new WaitForSecondsRealtime(0.3f);

                // 显示新的总属性
                if (newAttackText != null)
                {
                    newAttackText.text = $"当前攻击力: {_playerStats.AttackDamage:F2}";
                }

                if (newHealthText != null)
                {
                    newHealthText.text = $"当前最大生命: {_playerStats.MaxHealth:F2}";
                }
            }
        }

        /// <summary>
        /// 点击继续按钮
        /// </summary>
        private void OnContinueClicked()
        {
            HideSummary();
            LoadNextLevel();
        }

        /// <summary>
        /// 加载下一关
        /// </summary>
        private void LoadNextLevel()
        {
            Debug.Log($"[LevelSummaryPanel] 加载下一关: {nextSceneName}");

            // 使用场景转换管理器（如果存在）
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadScene(nextSceneName);
            }
            else
            {
                // 直接加载场景
                SceneManager.LoadScene(nextSceneName);
            }
        }

        /// <summary>
        /// 设置下一关场景名称（从外部调用）
        /// </summary>
        public void SetNextSceneName(string sceneName)
        {
            nextSceneName = sceneName;
        }

        private void OnDestroy()
        {
            // 清理按钮事件
            if (continueButton != null)
            {
                continueButton.onClick.RemoveListener(OnContinueClicked);
            }

            // 确保恢复时间
            Time.timeScale = 1f;
        }
    }
}

