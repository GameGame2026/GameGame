using UnityEngine;
using UnityEngine.UI;

namespace _Projects.GamePlay
{
    /// <summary>
    /// 简单的互动提示UI - 可以作为InteractableNPC的UI预制体
    /// 使用方法：
    /// 1. 创建一个Canvas（World Space）
    /// 2. 添加一个Image或Text作为子对象
    /// 3. 将此脚本添加到Canvas上
    /// 4. 将此Canvas保存为预制体
    /// 5. 在InteractableNPC的interactUIPrefab字段中引用此预制体
    /// </summary>
    public class SimpleInteractUI : MonoBehaviour
    {
        [Header("UI Settings")]
        [Tooltip("UI图标（可选）")]
        public Image iconImage;
        
        [Tooltip("提示文本（可选）")]
        public Text promptText = null;
        
        [Tooltip("是否始终面向相机")]
        public bool faceCamera = true;
        
        [Tooltip("浮动动画")]
        public bool enableFloatAnimation = true;
        
        [Tooltip("浮动速度")]
        public float floatSpeed = 1f;
        
        [Tooltip("浮动幅度")]
        public float floatAmount = 0.2f;
        
        private Vector3 _startPosition;
        private UnityEngine.Camera _mainCamera;

        private void Start()
        {
            _startPosition = transform.localPosition;
            _mainCamera = UnityEngine.Camera.main;
            
            // 设置默认提示文本
            if (promptText != null && string.IsNullOrEmpty(promptText.text))
            {
                promptText.text = "Press E to Interact";
            }
        }

        private void Update()
        {
            // 面向相机
            if (faceCamera && _mainCamera != null)
            {
                transform.rotation = Quaternion.LookRotation(transform.position - _mainCamera.transform.position);
            }
            
            // 浮动动画
            if (enableFloatAnimation)
            {
                float newY = _startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmount;
                transform.localPosition = new Vector3(_startPosition.x, newY, _startPosition.z);
            }
        }
    }
}

