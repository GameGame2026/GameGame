using UnityEngine;
using TMPro;

namespace _Projects.GamePlay.UI
{
    /// <summary>
    /// 伤害数字漂浮文本组件
    /// </summary>
    public class DamageNumber : MonoBehaviour
    {
        [Header("移动设置")]
        [SerializeField] private float floatSpeed = 2f; // 上升速度
        [SerializeField] private float lifetime = 1.5f; // 持续时间
        
        [Header("缩放动画")]
        [SerializeField] private float startScale = 0.5f; // 起始缩放
        [SerializeField] private float maxScale = 1.2f; // 最大缩放
        [SerializeField] private float scaleTime = 0.2f; // 缩放时间
        
        [Header("淡出设置")]
        [SerializeField] private float fadeStartTime = 0.5f; // 开始淡出的时间
        
        private TextMeshProUGUI _textMesh;
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private float _timer;
        private UnityEngine.Camera _mainCamera;
        
        private void Awake()
        {
            _textMesh = GetComponent<TextMeshProUGUI>();
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _mainCamera = UnityEngine.Camera.main;
            
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        /// <summary>
        /// 初始化伤害数字
        /// </summary>
        public void Initialize(float damage, Color color)
        {
            _textMesh.text = Mathf.RoundToInt(damage).ToString();
            _textMesh.color = color;
            _timer = 0f;
            transform.localScale = Vector3.one * startScale;
        }
        
        private void Update()
        {
            _timer += Time.deltaTime;
            
            // 使用世界坐标向上移动
            transform.position += Vector3.up * (floatSpeed * Time.deltaTime);
            
            // 缩放动画
            if (_timer < scaleTime)
            {
                float t = _timer / scaleTime;
                float scale = Mathf.Lerp(startScale, maxScale, t);
                transform.localScale = Vector3.one * scale;
            }
            else if (_timer < scaleTime * 2)
            {
                float t = (_timer - scaleTime) / scaleTime;
                float scale = Mathf.Lerp(maxScale, 1f, t);
                transform.localScale = Vector3.one * scale;
            }
            
            // 淡出效果
            if (_timer > fadeStartTime)
            {
                float fadeProgress = (_timer - fadeStartTime) / (lifetime - fadeStartTime);
                _canvasGroup.alpha = 1f - fadeProgress;
            }
            
            // 销毁
            if (_timer >= lifetime)
            {
                Destroy(gameObject);
            }
        }
        
        private void LateUpdate()
        {
            // 让Canvas始终面向摄像机
            if (_mainCamera != null && transform.parent != null)
            {
                transform.parent.rotation = _mainCamera.transform.rotation;
            }
        }
    }
}

