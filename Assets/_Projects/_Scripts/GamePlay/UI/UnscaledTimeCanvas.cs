using UnityEngine;
using UnityEngine.UI;

namespace _Projects._Scripts.GamePlay.UI
{
    /// <summary>
    /// 确保Canvas在Time.timeScale=0时仍然可以交互
    /// 添加此脚本到包含DeathUI的Canvas上
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class UnscaledTimeCanvas : MonoBehaviour
    {
        private Canvas _canvas;
        private GraphicRaycaster _raycaster;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _raycaster = GetComponent<GraphicRaycaster>();
            
            if (_raycaster == null)
            {
                _raycaster = gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        private void Start()
        {
            // 确保Canvas设置正确
            if (_canvas != null)
            {
                // 使用Screen Space - Overlay模式确保UI始终显示在最上层
                if (_canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    Debug.LogWarning("[UnscaledTimeCanvas] Canvas应该使用Screen Space - Overlay模式以确保UI正常工作");
                }
            }
        }
    }
}

