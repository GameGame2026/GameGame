using UnityEngine;
using _Projects.SceneManagement;

namespace _Projects.SceneManagement
{
    /// <summary>
    /// 场景转换触发器示例
    /// 可以通过碰撞、按钮点击等方式触发场景切换
    /// </summary>
    public class SceneTransitionTrigger : MonoBehaviour
    {
        [Header("场景设置")]
        [Tooltip("要加载的场景名称")]
        public string targetSceneName;

        [Tooltip("或者使用场景索引")]
        public int targetSceneIndex = -1;

        [Header("触发方式")]
        [Tooltip("使用碰撞触发")]
        public bool useTriggerCollision = true;

        [Tooltip("需要的标签（如 Player）")]
        public string requiredTag = "Player";

        [Tooltip("是否在触发后自动转换")]
        public bool autoTransition = true;

        [Header("调试")]
        [Tooltip("显示调试信息")]
        public bool showDebugInfo = true;

        /// <summary>
        /// 当有碰撞体进入触发器时调用
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            if (!useTriggerCollision || !autoTransition)
                return;

            // 检查是否是指定标签的对象
            if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag))
                return;

            if (showDebugInfo)
                Debug.Log($"触发场景转换: {other.gameObject.name} 进入了触发器");

            TransitionToTargetScene();
        }
        
        /// <summary>
        /// 转换到目标场景
        /// 可以从按钮或其他脚本调用此方法
        /// </summary>
        public void TransitionToTargetScene()
        {
            if (SceneTransitionManager.Instance.IsTransitioning)
            {
                if (showDebugInfo)
                    Debug.LogWarning("场景转换正在进行中...");
                return;
            }

            // 优先使用场景名称
            if (!string.IsNullOrEmpty(targetSceneName))
            {
                if (showDebugInfo)
                    Debug.Log($"开始加载场景: {targetSceneName}");
                
                SceneTransitionManager.Instance.LoadScene(targetSceneName);
            }
            // 如果没有场景名称，使用场景索引
            else if (targetSceneIndex >= 0)
            {
                if (showDebugInfo)
                    Debug.Log($"开始加载场景索引: {targetSceneIndex}");
                
                SceneTransitionManager.Instance.LoadScene(targetSceneIndex);
            }
            else
            {
                Debug.LogError("未设置目标场景！请在 Inspector 中设置场景名称或索引。");
            }
        }

        /// <summary>
        /// 重新加载当前场景（用于重生、重试等）
        /// </summary>
        public void RestartCurrentScene()
        {
            if (showDebugInfo)
                Debug.Log("重新加载当前场景");
            
            SceneTransitionManager.Instance.ReloadCurrentScene();
        }

        /// <summary>
        /// 加载下一个场景
        /// </summary>
        public void LoadNextScene()
        {
            if (showDebugInfo)
                Debug.Log("加载下一个场景");
            
            SceneTransitionManager.Instance.LoadNextScene();
        }

        /// <summary>
        /// 在编辑器中显示触发区域
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!useTriggerCollision)
                return;

            Gizmos.color = new Color(0, 1, 0, 0.3f);
            
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                Gizmos.DrawCube(transform.position, col.bounds.size);
            }
        }
    }
}

