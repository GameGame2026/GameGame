using UnityEngine;

namespace _Projects.GamePlay
{
    /// <summary>
    /// 梅花扑克敌人
    /// 特殊机制：
    /// - 造成2点伤害
    /// - 贴点后效果：变为半透明贴图，附着到最近的场景物体表面
    /// - 贴附逻辑：计算最近物体的表面位置并固定
    /// </summary>
    public class ClubPokerEnemy : EnemyBase
    {
        [Header("梅花扑克特殊设置")]
        [Tooltip("贴附检测范围")]
        public float attachSearchRadius = 5f;
        
        [Tooltip("贴附的目标层")]
        public LayerMask attachableLayers;
        
        [Tooltip("半透明度")]
        [Range(0f, 1f)]
        public float transparentAlpha = 0.5f;

        [Header("视觉效果")]
        [Tooltip("正常态材质")]
        public Material normalMaterial;
        
        [Tooltip("贴附态材质（半透明）")]
        public Material attachedMaterial;

        private Renderer _renderer;
        private Vector3 _originalPosition;
        private Quaternion _originalRotation;
        private Transform _attachedSurface;
        private bool _isAttachedToSurface;

        protected override void Awake()
        {
            base.Awake();
            _renderer = GetComponentInChildren<Renderer>();
            
            // 设置攻击力为2
            attackDamage = 2f;
        }
        

        /// <summary>
        /// 被贴点 - 附着到最近的场景物体表面
        /// </summary>
        public override void OnPointAttached()
        {
            base.OnPointAttached();
            
            // 保存原始位置
            _originalPosition = transform.position;
            _originalRotation = transform.rotation;
            
            // 查找最近的可附着表面
            AttachToNearestSurface();
            
            // 切换到半透明材质
            SetTransparent(true);
            
            // 禁用移动
            if (_navAgent != null)
            {
                _navAgent.enabled = false;
            }
            
            Debug.Log($"[ClubPoker] {gameObject.name} 贴附到表面");
        }

        /// <summary>
        /// 点被回收 - 恢复正常状态
        /// </summary>
        public override void OnPointDetached()
        {
            base.OnPointDetached();
            
            // 恢复透明度
            SetTransparent(false);
            
            // 脱离附着
            _isAttachedToSurface = false;
            _attachedSurface = null;
            
            // 重新启用移动
            if (_navAgent != null)
            {
                _navAgent.enabled = true;
            }
            
            // 恢复物理
            if (_rigidbody != null)
            {
                _rigidbody.isKinematic = false;
            }
            
            Debug.Log($"[ClubPoker] {gameObject.name} 脱离表面");
        }

        /// <summary>
        /// 附着到最近的表面
        /// </summary>
        private void AttachToNearestSurface()
        {
            // 查找附近的碰撞体
            Collider[] colliders = Physics.OverlapSphere(transform.position, attachSearchRadius, attachableLayers);
            
            if (colliders.Length == 0)
            {
                Debug.Log($"[ClubPoker] 没有找到可附着的表面");
                return;
            }
            
            // 找到最近的表面
            float closestDistance = float.MaxValue;
            Vector3 closestPoint = transform.position;
            Transform closestTransform = null;
            Vector3 surfaceNormal = Vector3.up;
            
            foreach (var col in colliders)
            {
                // 跳过自己
                if (col.transform == transform || col.transform.IsChildOf(transform))
                    continue;
                
                Vector3 closestPointOnCollider = col.ClosestPoint(transform.position);
                float distance = Vector3.Distance(transform.position, closestPointOnCollider);
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPoint = closestPointOnCollider;
                    closestTransform = col.transform;
                    
                    // 计算表面法线
                    surfaceNormal = (transform.position - closestPointOnCollider).normalized;
                    if (surfaceNormal == Vector3.zero)
                    {
                        surfaceNormal = Vector3.up;
                    }
                }
            }
            
            if (closestTransform != null)
            {
                // 移动到表面位置
                transform.position = closestPoint + surfaceNormal * 0.1f;
                
                // 旋转使其贴合表面
                transform.rotation = Quaternion.LookRotation(-surfaceNormal);
                
                _attachedSurface = closestTransform;
                _isAttachedToSurface = true;
                
                // 禁用物理
                if (_rigidbody != null)
                {
                    _rigidbody.isKinematic = true;
                }
            }
        }

        /// <summary>
        /// 设置透明度
        /// </summary>
        private void SetTransparent(bool transparent)
        {
            if (_renderer == null) return;
            
            if (transparent && attachedMaterial != null)
            {
                _renderer.material = attachedMaterial;
            }
            else if (!transparent && normalMaterial != null)
            {
                _renderer.material = normalMaterial;
            }
            
            // 或者直接修改材质透明度
            if (_renderer.material.HasProperty("_Color"))
            {
                Color color = _renderer.material.color;
                color.a = transparent ? transparentAlpha : 1f;
                _renderer.material.color = color;
            }
        }

        protected override void Update()
        {
            // 如果已附着，不执行常规AI更新
            if (_isAttachedToSurface && IsAttached)
            {
                // 附着状态下仍然可以被攻击
                return;
            }
            
            base.Update();
        }
    }
}
