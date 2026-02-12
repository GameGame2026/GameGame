using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace _Projects._Scripts.SceneManagement
{
    /// <summary>
    /// 制作人员面板控制器
    /// </summary>
    public class CreditsPanel : MonoBehaviour
    {
        [Header("UI引用")]
        [Tooltip("面板根对象")]
        [SerializeField] private GameObject panelRoot;
        
        [Tooltip("关闭按钮")]
        [SerializeField] private Button closeButton;
        
        [Tooltip("背景遮罩（点击关闭）")]
        [SerializeField] private Button backgroundMask;
        
        [Tooltip("制作人员文本")]
        [SerializeField] private TextMeshProUGUI creditsText;

        [Header("制作人员信息")]
        [TextArea(10, 20)]
        [SerializeField] private string creditsContent = 
@"游戏制作团队

策划：
xx

程序：
xx

美术：
xx

音效：
xx

特别感谢：
所有参与测试的玩家

© 2026 Game Jam Team";

        [Header("动画设置")]
        [Tooltip("是否启用淡入淡出动画")]
        [SerializeField] private bool useAnimation = true;
        
        [Tooltip("动画持续时间")]
        [SerializeField] private float animationDuration = 0.3f;

        private CanvasGroup _canvasGroup;
        private bool _isVisible;

        private void Awake()
        {
            // 获取或添加CanvasGroup组件
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null && useAnimation)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // 绑定按钮事件
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Hide);
            }

            if (backgroundMask != null)
            {
                backgroundMask.onClick.AddListener(Hide);
            }

            // 设置制作人员文本
            if (creditsText != null)
            {
                creditsText.text = creditsContent;
            }
        }

        private void Start()
        {
            // 初始状态为隐藏
            Hide();
        }

        private void Update()
        {
            // ESC键关闭
            if (_isVisible && Input.GetKeyDown(KeyCode.Escape))
            {
                Hide();
            }
        }

        private void OnDestroy()
        {
            // 取消绑定按钮事件
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Hide);
            }

            if (backgroundMask != null)
            {
                backgroundMask.onClick.RemoveListener(Hide);
            }
        }

        /// <summary>
        /// 显示面板
        /// </summary>
        public void Show()
        {
            _isVisible = true;

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }
            else
            {
                gameObject.SetActive(true);
            }

            if (useAnimation && _canvasGroup != null)
            {
                StartCoroutine(FadeIn());
            }
            else
            {
                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = 1f;
                    _canvasGroup.interactable = true;
                    _canvasGroup.blocksRaycasts = true;
                }
            }
        }

        /// <summary>
        /// 隐藏面板
        /// </summary>
        public void Hide()
        {
            _isVisible = false;

            if (useAnimation && _canvasGroup != null)
            {
                StartCoroutine(FadeOut());
            }
            else
            {
                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = 0f;
                    _canvasGroup.interactable = false;
                    _canvasGroup.blocksRaycasts = false;
                }

                if (panelRoot != null)
                {
                    panelRoot.SetActive(false);
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 淡入动画
        /// </summary>
        private System.Collections.IEnumerator FadeIn()
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = true;

            float elapsed = 0f;
            while (elapsed < animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / animationDuration);
                yield return null;
            }

            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
        }

        /// <summary>
        /// 淡出动画
        /// </summary>
        private System.Collections.IEnumerator FadeOut()
        {
            _canvasGroup.interactable = false;

            float elapsed = 0f;
            while (elapsed < animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / animationDuration);
                yield return null;
            }

            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}

