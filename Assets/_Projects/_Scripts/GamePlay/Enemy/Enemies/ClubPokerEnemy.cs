using System.Collections;
using UnityEngine;

namespace _Projects.GamePlay
{
    /// <summary>
    /// 梅花扑克敌人
    /// 特殊机制：
    /// - 造成2点伤害
    /// - 贴点后效果：变为半透明贴图，贴附到最近的场景物体表面
    /// - 使用协程和平滑过渡实现更好的视觉效果
    /// </summary>
    public class ClubPokerEnemy : EnemyBase
    {
        [Header("梅花扑克特殊设置")]
        [Tooltip("贴附检测范围")]
        public float attachSearchRadius = 10f;
        
        [Tooltip("贴附的目标层（墙壁、地面等）")]
        public LayerMask attachableLayers = -1; // 默认所有层
        
        [Tooltip("贴附状���的半透明度")]
        [Range(0f, 1f)]
        public float attachedAlpha = 0.5f;
        
        [Tooltip("移动到表面的速度")]
        public float attachMoveSpeed = 5f;
        
        [Tooltip("表面偏移距离（避免嵌入）")]
        public float surfaceOffset = 0.1f;

        [Header("视觉效果")]
        [Tooltip("材质透明度属性名")]
        public string alphaPropertyName = "_Color";
        
        [Tooltip("淡入淡出时间")]
        public float fadeTransitionTime = 0.3f;

        // 私有变量
        private Renderer[] _renderers;
        private Material[] _originalMaterials;
        private Material[] _transparentMaterials;
        private bool _isAttachedToSurface;
        private Vector3 _targetAttachPosition;
        private Quaternion _targetAttachRotation;
        private Coroutine _attachCoroutine;
        private bool _hasFoundSurface;

        protected override void Awake()
        {
            base.Awake();
            
            _renderers = GetComponentsInChildren<Renderer>();
            
            // 保存原始材质
            SaveOriginalMaterials();
            
            // 创建透明材质副本
            CreateTransparentMaterials();
        }
        
        #region 材质管理

        /// <summary>
        /// 保存原始材质
        /// </summary>
        private void SaveOriginalMaterials()
        {
            if (_renderers == null || _renderers.Length == 0) return;
            
            _originalMaterials = new Material[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null)
                {
                    _originalMaterials[i] = _renderers[i].material;
                }
            }
        }

        /// <summary>
        /// 创建透明材质副本
        /// </summary>
        private void CreateTransparentMaterials()
        {
            if (_originalMaterials == null || _originalMaterials.Length == 0) return;
            
            _transparentMaterials = new Material[_originalMaterials.Length];
            for (int i = 0; i < _originalMaterials.Length; i++)
            {
                if (_originalMaterials[i] != null)
                {
                    // 创建材质实例
                    _transparentMaterials[i] = new Material(_originalMaterials[i]);
                    
                    // 设置透明渲染模式
                    SetMaterialTransparent(_transparentMaterials[i]);
                }
            }
        }

        /// <summary>
        /// 设置材质为透明模式
        /// </summary>
        private void SetMaterialTransparent(Material mat)
        {
            if (mat == null) return;
            
            // 设置为透明渲染模式
            mat.SetFloat("_Mode", 3); // Transparent mode
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
        }

        /// <summary>
        /// 平滑过渡材质透明度
        /// </summary>
        private IEnumerator FadeToAlpha(float targetAlpha, float duration)
        {
            if (_renderers == null || _renderers.Length == 0) yield break;
            
            // 获取起始透明度
            float startAlpha = 1f;
            if (_renderers[0] != null && _renderers[0].material.HasProperty(alphaPropertyName))
            {
                startAlpha = _renderers[0].material.color.a;
            }
            
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                
                // 应用到所有渲染器
                foreach (var rend in _renderers)
                {
                    if (rend != null && rend.material.HasProperty(alphaPropertyName))
                    {
                        Color color = rend.material.color;
                        color.a = currentAlpha;
                        rend.material.color = color;
                    }
                }
                
                yield return null;
            }
            
            // 确保最终值准确
            foreach (var rend in _renderers)
            {
                if (rend != null && rend.material.HasProperty(alphaPropertyName))
                {
                    Color color = rend.material.color;
                    color.a = targetAlpha;
                    rend.material.color = color;
                }
            }
        }

        #endregion

        #region 贴点系统重写

        /// <summary>
        /// 被贴点 - 附着到最近的场景物体表面
        /// </summary>
        public override void OnPointAttached()
        {
            base.OnPointAttached();
            
            Debug.Log($"[ClubPoker] {gameObject.name} 开始贴附流程");
            
            // 停止之前的协程
            if (_attachCoroutine != null)
            {
                StopCoroutine(_attachCoroutine);
            }
            
            // 开始贴附协程
            _attachCoroutine = StartCoroutine(AttachToSurfaceCoroutine());
        }

        /// <summary>
        /// 点被回收 - 恢复正常状态
        /// </summary>
        public override void OnPointDetached()
        {
            base.OnPointDetached();
            
            Debug.Log($"[ClubPoker] {gameObject.name} 脱离表面");
            
            // 停止贴附协程
            if (_attachCoroutine != null)
            {
                StopCoroutine(_attachCoroutine);
                _attachCoroutine = null;
            }
            
            // 恢复状态
            _isAttachedToSurface = false;
            _hasFoundSurface = false;
            
            // 恢复材质
            RestoreOriginalMaterials();
            
            // 恢复物理
            if (_rigidbody != null)
            {
                _rigidbody.isKinematic = false;
            }
            
            // 恢复碰撞
            if (_collider != null)
            {
                _collider.enabled = true;
            }
        }

        #endregion

        #region 表面附着逻辑

        /// <summary>
        /// 贴附到表面的协程
        /// </summary>
        private IEnumerator AttachToSurfaceCoroutine()
        {
            // 查找最近的表面
            if (!FindNearestSurface())
            {
                Debug.LogWarning($"[ClubPoker] {gameObject.name} 没有找到可附着的表面，保持原位");
                _hasFoundSurface = false;
                
                // 即使没找到表面，也要变透明
                yield return StartCoroutine(TransitionToAttachedState());
                yield break;
            }
            
            _hasFoundSurface = true;
            
            // 第二步：平滑移动到表面
            yield return StartCoroutine(MoveToSurface());
            
            // 第三步：切换到透明状态
            yield return StartCoroutine(TransitionToAttachedState());
            
            _isAttachedToSurface = true;
            Debug.Log($"[ClubPoker] {gameObject.name} 成功贴附到表面");
        }

        /// <summary>
        /// 查找最近的可附着表面
        /// </summary>
        private bool FindNearestSurface()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, attachSearchRadius, attachableLayers);
            
            if (colliders.Length == 0)
            {
                return false;
            }
            
            float closestDistance = float.MaxValue;
            Vector3 closestPoint = transform.position;
            Vector3 surfaceNormal = Vector3.up;
            bool foundValid = false;
            
            foreach (var col in colliders)
            {
                // 跳过自己和子对象
                if (col.transform == transform || col.transform.IsChildOf(transform) || transform.IsChildOf(col.transform))
                    continue;
                
                // 跳过其他敌人
                if (col.GetComponent<EnemyBase>() != null)
                    continue;
                
                Vector3 closestPointOnCollider = col.ClosestPoint(transform.position);
                float distance = Vector3.Distance(transform.position, closestPointOnCollider);
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPoint = closestPointOnCollider;
                    
                    // 使用 Raycast 获取更准确的法线
                    Vector3 direction = closestPointOnCollider - transform.position;
                    if (direction.magnitude > 0.01f)
                    {
                        RaycastHit hit;
                        if (Physics.Raycast(transform.position, direction.normalized, out hit, attachSearchRadius, attachableLayers))
                        {
                            surfaceNormal = hit.normal;
                            closestPoint = hit.point;
                        }
                        else
                        {
                            // 后备方案：使用从表面指向自己的方向作为法线
                            surfaceNormal = direction.normalized;
                        }
                    }
                    
                    foundValid = true;
                }
            }
            
            if (foundValid)
            {
                _targetAttachPosition = closestPoint + surfaceNormal * surfaceOffset;
                _targetAttachRotation = Quaternion.LookRotation(-surfaceNormal);
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// 平滑移动到表面
        /// </summary>
        private IEnumerator MoveToSurface()
        {
            Vector3 startPosition = transform.position;
            Quaternion startRotation = transform.rotation;
            float elapsed = 0f;
            float duration = Vector3.Distance(startPosition, _targetAttachPosition) / attachMoveSpeed;
            duration = Mathf.Clamp(duration, 0.1f, 1f); // 限制在合理范围
            
            // 禁用 NavMeshAgent（如果存在）
            if (_navAgent != null && _navAgent.enabled)
            {
                _navAgent.enabled = false;
            }
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                
                // 使用平滑曲线
                t = Mathf.SmoothStep(0f, 1f, t);
                
                transform.position = Vector3.Lerp(startPosition, _targetAttachPosition, t);
                transform.rotation = Quaternion.Slerp(startRotation, _targetAttachRotation, t);
                
                yield return null;
            }
            
            // 确���最终位置准确
            transform.position = _targetAttachPosition;
            transform.rotation = _targetAttachRotation;
            
            // 完全禁用物理
            if (_rigidbody != null)
            {
                _rigidbody.isKinematic = true;
                _rigidbody.velocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
            }
        }

        /// <summary>
        /// 过渡到附着状态
        /// </summary>
        private IEnumerator TransitionToAttachedState()
        {
            // 切换到透明材质
            ApplyTransparentMaterials();
            
            // 淡入透明效果
            yield return StartCoroutine(FadeToAlpha(attachedAlpha, fadeTransitionTime));
        }

        /// <summary>
        /// 应用透明材质
        /// </summary>
        private void ApplyTransparentMaterials()
        {
            if (_renderers == null || _transparentMaterials == null) return;
            
            for (int i = 0; i < _renderers.Length && i < _transparentMaterials.Length; i++)
            {
                if (_renderers[i] != null && _transparentMaterials[i] != null)
                {
                    _renderers[i].material = _transparentMaterials[i];
                }
            }
        }

        /// <summary>
        /// 恢复原始材质
        /// </summary>
        private void RestoreOriginalMaterials()
        {
            if (_renderers == null || _originalMaterials == null) return;
            
            for (int i = 0; i < _renderers.Length && i < _originalMaterials.Length; i++)
            {
                if (_renderers[i] != null && _originalMaterials[i] != null)
                {
                    _renderers[i].material = _originalMaterials[i];
                    
                    // 确保透明度恢复为1
                    if (_renderers[i].material.HasProperty(alphaPropertyName))
                    {
                        Color color = _renderers[i].material.color;
                        color.a = 1f;
                        _renderers[i].material.color = color;
                    }
                }
            }
        }

        #endregion

        #region Update 逻辑

        protected override void Update()
        {
            // 如果已附着到表面，不执行常规AI更新
            if (_isAttachedToSurface && IsAttached)
            {
                // 附着状态下仍然可以被攻击，但不移动
                return;
            }
            
            base.Update();
        }

        #endregion

        #region 清理

        private void OnDisable()
        {
            // 停止所有协程
            if (_attachCoroutine != null)
            {
                StopCoroutine(_attachCoroutine);
                _attachCoroutine = null;
            }
            
            // 清理创建的材质实例
            if (_transparentMaterials != null)
            {
                foreach (var mat in _transparentMaterials)
                {
                    if (mat != null)
                    {
                        Destroy(mat);
                    }
                }
            }
        }

        #endregion
    }
}
