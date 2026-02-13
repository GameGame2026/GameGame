using UnityEngine;
using _Projects._Scripts.SceneManagement;

namespace _Projects._Scripts.GamePlay.Items
{
    /// <summary>
    /// 关卡结束触发器 - 当玩家进入触发区域时显示结算界面
    /// 支持两种模式：
    /// 1. 显示结算界面 - 玩家查看收集情况后点击继续
    /// 2. 直接切换场景 - 不显示结算界面
    /// </summary>
    public class LevelEndTrigger : MonoBehaviour
    {
        [Header("触发模式")]
        [Tooltip("结算模式：显示结算界面")]
        [SerializeField] private bool showSummaryPanel = true;

        [Header("结算面板（结算模式需要）")]
        [SerializeField] private LevelSummaryPanel summaryPanel;

        [Header("场景转换（直接切换模式需要）")]
        [Tooltip("下一关场景名称（仅在不显示结算界面时使用）")]
        [SerializeField] private string nextSceneName = "Scene2";

        [Header("触发设置")]
        [Tooltip("是否需要按键确认")]
        [SerializeField] private bool requireInput;

        [Tooltip("确认按键")]
        [SerializeField] private KeyCode confirmKey = KeyCode.E;

        [Header("提示UI")]
        [SerializeField] private GameObject interactPrompt;

        private bool _playerInRange;
        private bool _hasTriggered;

        private void Start()
        {
            // 隐藏提示
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }

            // 如果是结算模式，查找结算面板
            if (showSummaryPanel && summaryPanel == null)
            {
                summaryPanel = FindObjectOfType<LevelSummaryPanel>();
                if (summaryPanel == null)
                {
                    Debug.LogWarning("[LevelEndTrigger] 结算模式但未找到 LevelSummaryPanel，将切换为直接场景转换模式");
                    showSummaryPanel = false;
                }
            }
        }

        private void Update()
        {
            if (_playerInRange && !_hasTriggered && requireInput)
            {
                if (Input.GetKeyDown(confirmKey))
                {
                    TriggerLevelEnd();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_hasTriggered) return;

            if (other.CompareTag("Player"))
            {
                _playerInRange = true;

                if (requireInput)
                {
                    // 显示提示
                    if (interactPrompt != null)
                    {
                        interactPrompt.SetActive(true);
                    }
                }
                else
                {
                    // 自动触发
                    TriggerLevelEnd();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playerInRange = false;

                // 隐藏提示
                if (interactPrompt != null)
                {
                    interactPrompt.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 触发关卡结束
        /// </summary>
        private void TriggerLevelEnd()
        {
            if (_hasTriggered) return;
            _hasTriggered = true;

            Debug.Log("[LevelEndTrigger] 关卡结束触发");

            // 隐藏提示
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }

            if (showSummaryPanel)
            {
                // 模式1：显示结算界面（推荐）
                ShowSummary();
            }
            else
            {
                // 模式2：直接切换场景
                LoadNextScene();
            }
        }

        /// <summary>
        /// 显示结算界面
        /// </summary>
        private void ShowSummary()
        {
            if (summaryPanel != null)
            {
                Debug.Log("[LevelEndTrigger] 显示结算界面");
                summaryPanel.ShowSummary();
            }
            else
            {
                Debug.LogError("[LevelEndTrigger] 未找到 LevelSummaryPanel，无法显示结算界面！");
                // 降级处理：直接切换场景
                LoadNextScene();
            }
        }

        /// <summary>
        /// 直接加载下一关（不显示结算界面）
        /// </summary>
        private void LoadNextScene()
        {
            Debug.Log($"[LevelEndTrigger] 直接加载下一关: {nextSceneName}");

            // 先应用收集提升效果
            if (CollectibleManager.Instance != null)
            {
                CollectibleManager.Instance.ApplyCollectionBoosts();
            }

            // 使用场景转换管理器
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadScene(nextSceneName);
            }
            else
            {
                // 降级处理：直接加载
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
            }
        }

        /// <summary>
        /// 设置下一关场景名称（可从外部调用）
        /// </summary>
        public void SetNextSceneName(string sceneName)
        {
            nextSceneName = sceneName;
        }
    }
}

