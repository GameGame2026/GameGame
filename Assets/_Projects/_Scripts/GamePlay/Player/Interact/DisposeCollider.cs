using System.Collections.Generic;
using UnityEngine;

namespace _Projects.GamePlay.Player.Controller
{
    public class DisposeCollider : MonoBehaviour
    {
        // 范围内的所有可贴附物体及其对应的碰撞器
        private Dictionary<DisposableObject, Collider> _objectsInRange = new Dictionary<DisposableObject, Collider>();
        
        public DisposableObject GetDisposable()
        {
            // 清理无效引用并返回第一个仍然在范围内且未贴附的对象
            List<DisposableObject> toRemove = new List<DisposableObject>();
            
            foreach (var kvp in _objectsInRange)
            {
                var obj = kvp.Key;
                var objCollider = kvp.Value;
                
                if (obj == null || objCollider == null) 
                {
                    toRemove.Add(kvp.Key);
                    continue;
                }

                // 如果已经被贴附，移除
                if (obj.IsAttached)
                {
                    toRemove.Add(kvp.Key);
                    continue;
                }

                // 有效且可贴附，立即返回
                // 注意：不再使用 IsObjectInTrigger 检查，因为 OnTriggerEnter/Exit 已经可靠地管理了范围
                Debug.Log($"[DisposeCollider] 找到有效可贴附物体: {obj.name}");
                return obj;
            }

            // 执行移除
            foreach (var r in toRemove)
            {
                _objectsInRange.Remove(r);
            }

            return null;
        }

        // // 2.23 静影：加入一个手动移除
        // public void RemoveFromRange(DisposableObject obj)
        // {
        //     if (_objectsInRange.ContainsKey(obj))
        //     {
        //         _objectsInRange.Remove(obj);
        //         Debug.Log($"[DisposeCollider] 主动移除 {obj.name} 从范围字典");
        //     }
        // }
        
        // /// <summary>
        // /// 触发范围内所有未贴附物体的贴附操作（如果需要的话）
        // /// </summary>
        // public void TriggerAllDisposables()
        // {
        //     // 遍历时收集需要移除的项以避免在枚举时修改集合
        //     List<DisposableObject> toRemove = new List<DisposableObject>();
            
        //     foreach (var kvp in _objectsInRange)
        //     {
        //         var obj = kvp.Key;
        //         var objCollider = kvp.Value;
                
        //         if (obj == null || objCollider == null)
        //         {
        //             toRemove.Add(kvp.Key);
        //             continue;
        //         }

        //         if (obj.IsAttached)
        //         {
        //             toRemove.Add(kvp.Key);
        //             continue;
        //         }

        //         obj.ChangeState();
        //     }

        //     foreach (var r in toRemove)
        //     {
        //         _objectsInRange.Remove(r);
        //     }
        // }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Disposeable"))
            {
                // 使用 GetComponentInParent 以提高在模型层级中寻找组件的可靠性
                var disposable = other.GetComponentInParent<DisposableObject>();
                if (disposable != null)
                {
                    // 存储 DisposableObject 和触发的 Collider
                    _objectsInRange[disposable] = other;
                    Debug.Log($"[DisposeCollider] 可贴附物体进入范围: {disposable.name}, Collider位置: {other.bounds.center}");
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
        

        /// <summary>
        /// 对外公开的判断接口：判断指定 DisposableObject 是否仍在检测范围内（并且未贴附且未被销毁）
        /// </summary>
        public bool IsInRange(DisposableObject obj)
        {
            if (obj == null) return false;
            if (!_objectsInRange.ContainsKey(obj)) return false;
            if (obj.IsAttached) return false;
            

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