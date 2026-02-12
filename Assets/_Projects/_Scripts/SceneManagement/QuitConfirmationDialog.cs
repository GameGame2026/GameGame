using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace _Projects._Scripts.SceneManagement
{
    /// <summary>
    /// 退出游戏确认对话框
    /// </summary>
    public class QuitConfirmationDialog : MonoBehaviour
    {
        [Header("UI引用")]
        [Tooltip("对话框根对象")]
        [SerializeField] private GameObject dialogRoot;
        
        [Tooltip("确认按钮")]
        [SerializeField] private Button confirmButton;
        
        [Tooltip("取消按钮")]
        [SerializeField] private Button cancelButton;
        
        [Tooltip("背景遮罩（可选，点击取消）")]
        [SerializeField] private Button backgroundMask;
        
        [Tooltip("提示文本")]
        [SerializeField] private TextMeshProUGUI messageText;

        [Header("对话框设置")]
        [Tooltip("提示消息")]
        [SerializeField] private string message = "确定要退出游戏吗？";

        [Header("动画设置")]
        [Tooltip("是否启用缩放动画")]
        [SerializeField] private bool useAnimation = true;
        
        [Tooltip("动画持续时间")]
        [SerializeField] private float animationDuration = 0.2f;
        
        [Tooltip("弹出时的缩放效果")]
        [SerializeField] private AnimationCurve popupCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private CanvasGroup _canvasGroup;
        private RectTransform _dialogTransform;
        private MainMenuController _mainMenuController;
        private bool _isVisible;

        private void Awake()
        {
            // 获取组件
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null && useAnimation)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // 获取对话框的RectTransform（用于缩放动画）
            if (dialogRoot != null)
            {
                _dialogTransform = dialogRoot.GetComponent<RectTransform>();
            }
            else
            {
                _dialogTransform = GetComponent<RectTransform>();
            }

            // 绑定按钮事件
            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(OnConfirmClicked);
            }
            else
            {
                Debug.LogError("ConfirmButton 未设置！");
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(OnCancelClicked);
            }
            else
            {
                Debug.LogError("CancelButton 未设置！");
            }

            if (backgroundMask != null)
            {
                backgroundMask.onClick.AddListener(OnCancelClicked);
            }

            // 设置提示文本
            if (messageText != null)
            {
                messageText.text = message;
            }

            // 查找MainMenuController
            _mainMenuController = FindObjectOfType<MainMenuController>();
        }

        private void Start()
        {
            // 初始状态为隐藏
            Hide();
        }

        private void Update()
        {
            // ESC键取消
            if (_isVisible && Input.GetKeyDown(KeyCode.Escape))
            {
                OnCancelClicked();
            }
        }

        private void OnDestroy()
        {
            // 取消绑定按钮事件
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveListener(OnConfirmClicked);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveListener(OnCancelClicked);
            }

            if (backgroundMask != null)
            {
                backgroundMask.onClick.RemoveListener(OnCancelClicked);
            }
        }

        /// <summary>
        /// 显示对话框
        /// </summary>
        public void Show()
        {
            _isVisible = true;

            if (dialogRoot != null)
            {
                dialogRoot.SetActive(true);
            }
            else
            {
                gameObject.SetActive(true);
            }

            if (useAnimation)
            {
                StartCoroutine(ShowAnimation());
            }
            else
            {
                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = 1f;
                    _canvasGroup.interactable = true;
                    _canvasGroup.blocksRaycasts = true;
                }

                if (_dialogTransform != null)
                {
                    _dialogTransform.localScale = Vector3.one;
                }
            }
        }

        /// <summary>
        /// 隐藏对话框
        /// </summary>
        public void Hide()
        {
            _isVisible = false;

            if (useAnimation)
            {
                StartCoroutine(HideAnimation());
            }
            else
            {
                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = 0f;
                    _canvasGroup.interactable = false;
                    _canvasGroup.blocksRaycasts = false;
                }

                if (_dialogTransform != null)
                {
                    _dialogTransform.localScale = Vector3.zero;
                }

                if (dialogRoot != null)
                {
                    dialogRoot.SetActive(false);
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 点击确认按钮
        /// </summary>
        private void OnConfirmClicked()
        {
            Debug.Log("确认退出游戏");
            
            // 调用MainMenuController的退出方法
            if (_mainMenuController != null)
            {
                _mainMenuController.ConfirmQuitGame();
            }
            else
            {
                // 如果找不到MainMenuController，直接退出
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }

        /// <summary>
        /// 点击取消按钮
        /// </summary>
        private void OnCancelClicked()
        {
            Debug.Log("取消退出");
            Hide();
        }

        /// <summary>
        /// 显示动画（弹出效果）
        /// </summary>
        private System.Collections.IEnumerator ShowAnimation()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = true;
            }

            float elapsed = 0f;
            while (elapsed < animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / animationDuration;
                float curveValue = popupCurve.Evaluate(t);

                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = t;
                }

                if (_dialogTransform != null)
                {
                    _dialogTransform.localScale = Vector3.one * curveValue;
                }

                yield return null;
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = true;
            }

            if (_dialogTransform != null)
            {
                _dialogTransform.localScale = Vector3.one;
            }
        }

        /// <summary>
        /// 隐藏动画（缩小效果）
        /// </summary>
        private System.Collections.IEnumerator HideAnimation()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = false;
            }

            float elapsed = 0f;
            while (elapsed < animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = 1f - (elapsed / animationDuration);

                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = t;
                }

                if (_dialogTransform != null)
                {
                    _dialogTransform.localScale = Vector3.one * t;
                }

                yield return null;
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
            }

            if (_dialogTransform != null)
            {
                _dialogTransform.localScale = Vector3.zero;
            }

            if (dialogRoot != null)
            {
                dialogRoot.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}

