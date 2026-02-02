using UnityEngine;


namespace _Projects.GamePlay
{
    /// <summary>
    /// 可贴附对象 - 场景中可以被贴上prefab的物体
    /// </summary>
    public class DisposableObject : MonoBehaviour
    {
        [Header("贴附设置")]
        [Tooltip("要贴附的预制体")]
        public GameObject attachPrefab;
        
        [Tooltip("贴附后的材质")]
        public Material attachedMaterial;
        
        [Tooltip("prefab贴附的位置偏移")]
        public Vector3 attachOffset = Vector3.zero;
        
        [Tooltip("是否已被贴附")]
        public bool IsAttached { get; private set; }
        
        private Material _originalMaterial;
        private Renderer _renderer;
        private GameObject _attachedPrefabInstance;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            if (_renderer != null)
            {
                _originalMaterial = _renderer.material;
            }
        }

        /// <summary>
        /// 贴上prefab，改变物体状态
        /// </summary>
        public void ChangeState()
        {
            if (IsAttached) return;
            
            IsAttached = true;
            
            // 在物体上实例化prefab作为子物体
            if (attachPrefab != null)
            {
                _attachedPrefabInstance = Instantiate(attachPrefab, transform);
                _attachedPrefabInstance.transform.localPosition = attachOffset;
            }
            
            // 改变材质
            if (_renderer != null && attachedMaterial != null)
            {
                _renderer.material = attachedMaterial;
            }
            
            Debug.Log($"{gameObject.name} 已贴上prefab");
        }

        /// <summary>
        /// 回收prefab，恢复物体原状
        /// </summary>
        public void Recycle()
        {
            if (!IsAttached) return;
            
            IsAttached = false;
            
            // 销毁贴附的prefab
            if (_attachedPrefabInstance != null)
            {
                Destroy(_attachedPrefabInstance);
                _attachedPrefabInstance = null;
            }
            
            // 恢复原始材质
            if (_renderer != null && _originalMaterial != null)
            {
                _renderer.material = _originalMaterial;
            }
            
            Debug.Log($"{gameObject.name} 已回收prefab，恢复原状");
        }
    }
}

