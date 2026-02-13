using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

namespace _Projects._Scripts.GamePlay.SaveSystem
{
    /// <summary>
    /// 存档成功通知UI
    /// </summary>
    public class SaveNotificationUI : Singleton<SaveNotificationUI>
    {
        [Header("UI组件")]
        [Tooltip("通知面板")]
        [SerializeField] private GameObject notificationPanel;
        
        [Tooltip("通知文本")]
        [SerializeField] private TextMeshProUGUI notificationText;
        
        [Tooltip("通知图标（可选）")]
        [SerializeField] private Image notificationIcon;
        
        [Header("显示设置")]
        [Tooltip("通知显示时长")]
        [SerializeField] private float displayDuration = 2f;
        
        [Tooltip("淡入淡出时长")]
        [SerializeField] private float fadeDuration = 0.5f;
        
        [Tooltip("存档成功提示文本")]
        [SerializeField] private string saveSuccessText = "游戏已保存";
        
        private CanvasGroup _canvasGroup;
        private Coroutine _currentNotification;

        protected override void Awake()
        {
            base.Awake();
            
            // 获取或添加 CanvasGroup
            _canvasGroup = notificationPanel?.GetComponent<CanvasGroup>();
            if (_canvasGroup == null && notificationPanel != null)
            {
                _canvasGroup = notificationPanel.AddComponent<CanvasGroup>();
            }
            
            // 初始隐藏
            if (notificationPanel != null)
            {
                notificationPanel.SetActive(false);
            }
            
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
            }
        }

        /// <summary>
        /// 显示存档成功通知
        /// </summary>
        public void ShowSaveNotification()
        {
            ShowNotification(saveSuccessText);
        }

        /// <summary>
        /// 显示自定义通知
        /// </summary>
        public void ShowNotification(string message)
        {
            if (notificationPanel == null)
            {
                Debug.LogWarning("[SaveNotificationUI] 通知面板未设置");
                return;
            }
            
            // 停止之前的通知
            if (_currentNotification != null)
            {
                StopCoroutine(_currentNotification);
            }
            
            // 显示新通知
            _currentNotification = StartCoroutine(ShowNotificationCoroutine(message));
        }

        /// <summary>
        /// 显示通知协程
        /// </summary>
        private IEnumerator ShowNotificationCoroutine(string message)
        {
            // 设置文本
            if (notificationText != null)
            {
                notificationText.text = message;
            }
            
            // 显示面板
            notificationPanel.SetActive(true);
            
            // 淡入
            yield return StartCoroutine(FadeIn());
            
            // 等待显示时长
            yield return new WaitForSeconds(displayDuration);
            
            // 淡出
            yield return StartCoroutine(FadeOut());
            
            // 隐藏面板
            notificationPanel.SetActive(false);
            
            _currentNotification = null;
        }

        /// <summary>
        /// 淡入效果
        /// </summary>
        private IEnumerator FadeIn()
        {
            if (_canvasGroup == null) yield break;
            
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
                yield return null;
            }
            
            _canvasGroup.alpha = 1f;
        }

        /// <summary>
        /// 淡出效果
        /// </summary>
        private IEnumerator FadeOut()
        {
            if (_canvasGroup == null) yield break;
            
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Clamp01(1f - (elapsed / fadeDuration));
                yield return null;
            }
            
            _canvasGroup.alpha = 0f;
        }
    }
}

