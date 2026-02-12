using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace _Projects._Scripts.SceneManagement
{
    /// <summary>
    /// 主菜单按钮 - 处理按钮的视觉反馈效果（放大/变色）
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class MainMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("缩放效果")]
        [Tooltip("鼠标悬停时的放大倍数")]
        [SerializeField] private float hoverScale = 1.1f;
        
        [Tooltip("点击时的缩小倍数")]
        [SerializeField] private float clickScale = 0.95f;
        
        [Tooltip("缩放动画持续时间")]
        [SerializeField] private float scaleDuration = 0.2f;

        [Header("颜色效果")]
        [Tooltip("是否启用颜色变化")]
        [SerializeField] private bool useColorChange = true;
        
        [Tooltip("鼠标悬停时的颜色")]
        [SerializeField] private Color hoverColor = new Color(1f, 0.9f, 0.7f, 1f);
        
        [Tooltip("点击时的颜色")]
        [SerializeField] private Color clickColor = new Color(1f, 0.8f, 0.5f, 1f);

        [Header("音效")]
        [Tooltip("鼠标悬停音效")]
        [SerializeField] private AudioClip hoverSound;
        
        [Tooltip("音频源（可选）")]
        [SerializeField] private AudioSource audioSource;

        private Button _button;
        private Image _buttonImage;
        private Color _originalColor;
        private Vector3 _originalScale;
        private Coroutine _scaleCoroutine;
        private bool _isPointerOver;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _buttonImage = GetComponent<Image>();
            
            if (_buttonImage != null)
            {
                _originalColor = _buttonImage.color;
            }
            
            _originalScale = transform.localScale;

            if (audioSource == null)
            {
                audioSource = UnityEngine.Camera.main?.GetComponent<AudioSource>();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_button != null && !_button.interactable) return;
            
            _isPointerOver = true;
            
            PlayHoverSound();
            AnimateScale(hoverScale);
            
            if (useColorChange && _buttonImage != null)
            {
                _buttonImage.color = hoverColor;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_button != null && !_button.interactable) return;
            
            _isPointerOver = false;
            AnimateScale(1f);
            
            if (useColorChange && _buttonImage != null)
            {
                _buttonImage.color = _originalColor;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_button != null && !_button.interactable) return;
            
            AnimateScale(clickScale);
            
            if (useColorChange && _buttonImage != null)
            {
                _buttonImage.color = clickColor;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_button != null && !_button.interactable) return;
            
            if (_isPointerOver)
            {
                AnimateScale(hoverScale);
                if (useColorChange && _buttonImage != null)
                {
                    _buttonImage.color = hoverColor;
                }
            }
            else
            {
                AnimateScale(1f);
                if (useColorChange && _buttonImage != null)
                {
                    _buttonImage.color = _originalColor;
                }
            }
        }

        private void AnimateScale(float targetScale)
        {
            if (_scaleCoroutine != null)
            {
                StopCoroutine(_scaleCoroutine);
            }
            
            _scaleCoroutine = StartCoroutine(ScaleCoroutine(targetScale));
        }

        private IEnumerator ScaleCoroutine(float targetScale)
        {
            Vector3 startScale = transform.localScale;
            Vector3 endScale = _originalScale * targetScale;
            float elapsed = 0f;

            while (elapsed < scaleDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / scaleDuration);
                float easedT = EaseOutBack(t);
                
                transform.localScale = Vector3.Lerp(startScale, endScale, easedT);
                yield return null;
            }

            transform.localScale = endScale;
        }

        private float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        private void PlayHoverSound()
        {
            if (hoverSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(hoverSound);
            }
        }

        private void OnDisable()
        {
            transform.localScale = _originalScale;
            if (_buttonImage != null)
            {
                _buttonImage.color = _originalColor;
            }
        }
    }
}

