using UnityEngine;

namespace _Projects.SceneManagement
{
    /// <summary>
    /// 场景持久化管理器
    /// 用于在场景切换时保留特定的游戏对象（如 Player）
    /// </summary>
    public class DontDestroyOnLoadManager : MonoBehaviour
    {
        [Header("场景切换保留设置")]
        [Tooltip("是否在场景切换时保留此对象")]
        public bool persistAcrossScenes = true;

        [Tooltip("是否为单例对象")]
        public bool isSingleton = false;

        [Tooltip("单例标签")]
        public string singletonTag = "";

        private void Awake()
        {
            if (!persistAcrossScenes)
                return;

            // 如果是单例模式，检查是否已经存在相同的对象
            if (isSingleton && !string.IsNullOrEmpty(singletonTag))
            {
                GameObject[] existingObjects = GameObject.FindGameObjectsWithTag(singletonTag);
                
                // 如果已经存在同标签的对象，销毁当前对象
                if (existingObjects.Length > 1)
                {
                    Destroy(gameObject);
                    return;
                }
            }

            // 标记为场景切换时不销毁
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 手动销毁此持久化对象
        /// </summary>
        public void DestroyPersistentObject()
        {
            Destroy(gameObject);
        }
    }
}

