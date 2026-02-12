using UnityEngine;
using System.Collections;

namespace _Projects._Scripts.SceneManagement
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    public class SceneTransitionUI : Singleton<SceneTransitionUI>
    {
        private CanvasGroup _canvasGroup;
        private Canvas _canvas;

        protected override void Awake()
        {
            base.Awake();
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvas = GetComponent<Canvas>();
            
            _canvas.sortingOrder = 9999;
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
        }

        /// <summary>
        /// 淡出效果
        /// </summary>
        public IEnumerator FadeOut(float duration)
        {
            _canvasGroup.blocksRaycasts = true;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

            _canvasGroup.alpha = 1f;
        }

        /// <summary>
        /// 淡入效果
        /// </summary>
        public IEnumerator FadeIn(float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Clamp01(1f - (elapsed / duration));
                yield return null;
            }

            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
        }
    }
}

