using System.Collections.Generic;
using UnityEngine;

namespace _Projects.GamePlay.Player.Controller
{
    public class DisposeCollider : MonoBehaviour
    {
        // 范围内的所有可贴附物体
        private HashSet<DisposableObject> _objectsInRange = new HashSet<DisposableObject>();
        
        // 本地检测器引用，用于在运行时二次验证对象是否仍在范围内
        private Collider _localCollider;
        
        public DisposableObject GetDisposable()
        {
            // 清理无效引用并返回第一个仍然在范围内且未贴附的对象
            List<DisposableObject> toRemove = new List<DisposableObject>();
            foreach (var obj in _objectsInRange)
            {
                if (obj == null)
                {
                    toRemove.Add(obj);
                    continue;
                }

                // 如果已经被贴附，移除
                if (obj.IsAttached)
                {
                    toRemove.Add(obj);
                    continue;
                }

                // 如果本地 Collider 可用，二次验证对象是否仍然在触发范围内
                if (_localCollider != null && !IsObjectInsideLocalCollider(obj))
                {
                    toRemove.Add(obj);
                    continue;
                }

                // 有效且可贴附，立即返回
                return obj;
            }

            // 执行移除
            foreach (var r in toRemove)
            {
                _objectsInRange.Remove(r);
            }

            return null;
        }

        /// <summary>
        /// 触发范围内所有未贴附物体的贴附操作（如果需要的话）
        /// </summary>
        public void TriggerAllDisposables()
        {
            // 遍历时收集需要移除的项以避免在枚举时修改集合
            List<DisposableObject> toRemove = new List<DisposableObject>();
            foreach (var obj in _objectsInRange)
            {
                if (obj == null)
                {
                    toRemove.Add(obj);
                    continue;
                }

                if (obj.IsAttached)
                {
                    toRemove.Add(obj);
                    continue;
                }

                if (_localCollider != null && !IsObjectInsideLocalCollider(obj))
                {
                    toRemove.Add(obj);
                    continue;
                }

                obj.ChangeState();
            }

            foreach (var r in toRemove)
            {
                _objectsInRange.Remove(r);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Disposeable"))
            {
                // 使用 GetComponentInParent 以提高在模型层级中寻找组件的可靠性
                var disposable = other.GetComponentInParent<DisposableObject>();
                if (disposable != null)
                {
                    _objectsInRange.Add(disposable);
                    Debug.Log($"[DisposeCollider] 可贴附物体进入范围: {disposable.name}");
                }
            }
           
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Disposeable"))
            {
                var disposable = other.GetComponentInParent<DisposableObject>();
                if (disposable != null)
                {
                    _objectsInRange.Remove(disposable);
                    Debug.Log($"[DisposeCollider] 可贴附物体离开范围: {disposable.name}");
                }
            }
            
        }

        private void Awake()
        {
            _localCollider = GetComponent<Collider>();
        }
        
        /// <summary>
        /// 判断对象大致是否仍在此检测器的触发范围内（基于 world bounds）
        /// </summary>
        private bool IsObjectInsideLocalCollider(DisposableObject obj)
        {
            if (obj == null) return false;
            if (_localCollider == null) return false;

            // 使用 Collider.bounds 来近似检测（世界坐标轴对齐包围盒）
            return _localCollider.bounds.Contains(obj.transform.position);
        }

        /// <summary>
        /// 对外公开的判断接口：判断指定 DisposableObject 是否仍在检测范围内（并且未贴附且未被销毁）
        /// </summary>
        public bool IsInRange(DisposableObject obj)
        {
            if (obj == null) return false;
            if (!_objectsInRange.Contains(obj)) return false;
            if (obj.IsAttached) return false;
            if (_localCollider != null && !IsObjectInsideLocalCollider(obj)) return false;
            return true;
        }

        // 可视化检测范围（在Scene视图中）
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                
                if (col is BoxCollider box)
                {
                    Gizmos.DrawCube(box.center, box.size);
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(box.center, box.size);
                }
                else if (col is SphereCollider sphere)
                {
                    Gizmos.DrawSphere(sphere.center, sphere.radius);
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(sphere.center, sphere.radius);
                }
            }
        }
    }
}