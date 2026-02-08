using UnityEngine;


namespace _Projects.GamePlay
{
    /// <summary>
    /// 可贴附对象 - 场景中可以被贴上prefab的物体（基类）
    /// </summary>
    public class DisposableObject : MonoBehaviour
    {
        [Header("贴附设置")]
        [Tooltip("要贴附的预制体")]
        public GameObject attachPrefab;
        
        [Tooltip("是否已被贴附")]
        public bool IsAttached { get; protected set; }
        
        protected GameObject _attachedPrefabInstance;


        /// <summary>
        /// 贴上prefab，改变物体状态（可被子类重写）
        /// </summary>
        public virtual void ChangeState()
        {
            if (IsAttached) return;
            
            IsAttached = true;
            
            // 如果有预制体，实例化它
            if (attachPrefab != null)
            {
                _attachedPrefabInstance = Instantiate(attachPrefab, transform.position, transform.rotation, transform);
            }
            
            Debug.Log($"{gameObject.name} 已贴上prefab");
        }

        /// <summary>
        /// 回收prefab，恢复物体原状（可被子类重写）
        /// </summary>
        public virtual void Recycle()
        {
            if (!IsAttached) return;
            
            IsAttached = false;
            
            // 销毁贴附的prefab
            if (_attachedPrefabInstance != null)
            {
                Destroy(_attachedPrefabInstance);
                _attachedPrefabInstance = null;
            }
            
            Debug.Log($"{gameObject.name} 已回收prefab，恢复原状");
        }
    }
}

