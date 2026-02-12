using UnityEngine;

namespace _Projects._Scripts.SceneManagement
{
    /// <summary>
    /// 简单的DontDestroyOnLoad标记组件
    /// 用于标识哪些GameObject应该在场景切换时保持存在
    /// </summary>
    public class DontDestroyOnLoadManager : MonoBehaviour
    {
        [Header("设置")]
        [Tooltip("是否在Awake时自动应用DontDestroyOnLoad")]
        [SerializeField] private bool applyOnAwake = true;

        private void Awake()
        {
            if (applyOnAwake)
            {
                DontDestroyOnLoad(gameObject);
                Debug.Log($"{gameObject.name} 已设置为 DontDestroyOnLoad");
            }
        }

        /// <summary>
        /// 手动应用DontDestroyOnLoad
        /// </summary>
        public void ApplyDontDestroyOnLoad()
        {
            DontDestroyOnLoad(gameObject);
            Debug.Log($"{gameObject.name} 已设置为 DontDestroyOnLoad");
        }

        /// <summary>
        /// 移除DontDestroyOnLoad状态（通过将对象移动到当前场景）
        /// </summary>
        public void RemoveDontDestroyOnLoad()
        {
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(
                gameObject, 
                UnityEngine.SceneManagement.SceneManager.GetActiveScene()
            );
            Debug.Log($"{gameObject.name} 已移回当前场景");
        }
    }
}

