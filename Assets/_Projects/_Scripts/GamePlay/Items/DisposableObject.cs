using UnityEngine;


namespace _Projects.GamePlay
{
   
    public class DisposableObject : MonoBehaviour
    {
        [Header("贴附设置")]
        [Tooltip("要贴附的预制体")]
        public GameObject attachPrefab;
        
        [Tooltip("是否已被贴附")]
        public bool IsAttached { get; protected set; }
        
        protected GameObject _attachedPrefabInstance;

        
        public virtual void ChangeState()
        {
            if (IsAttached) return;
            
            IsAttached = true;
            
            if (attachPrefab != null)
            {
                _attachedPrefabInstance = Instantiate(attachPrefab, transform.position, transform.rotation, transform);
            }
            
            Debug.Log($"{gameObject.name} 已贴上prefab");
        }
        
        public virtual void Recycle()
        {
            if (!IsAttached) return;
            
            IsAttached = false;
            
            if (_attachedPrefabInstance != null)
            {
                Destroy(_attachedPrefabInstance);
                _attachedPrefabInstance = null;
            }
            
            Debug.Log($"{gameObject.name} 已回收prefab，恢复原状");
        }
    }
}

