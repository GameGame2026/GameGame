using System.Collections.Generic;
using UnityEngine;

namespace _Projects.GamePlay.Player.Controller
{
    public class DisposeCollider : MonoBehaviour
    {
        // 范围内的所有可贴附物体
        private HashSet<DisposableObject> _objectsInRange = new HashSet<DisposableObject>();
        
        public DisposableObject GetDisposable()
        {
            foreach (var obj in _objectsInRange)
            {
                if (obj != null && !obj.IsAttached)
                {
                    return obj;
                }
            }
            return null;
        }

        /// <summary>
        /// 触发范围内所有未贴附物体的贴附操作（如果需要的话）
        /// </summary>
        public void TriggerAllDisposables()
        {
            foreach (var obj in _objectsInRange)
            {
                if (obj != null && !obj.IsAttached)
                {
                    obj.ChangeState();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Disposeable"))
            {
                var disposable = other.GetComponent<DisposableObject>();
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
                var disposable = other.GetComponent<DisposableObject>();
                if (disposable != null)
                {
                    _objectsInRange.Remove(disposable);
                    Debug.Log($"[DisposeCollider] 可贴附物体离开范围: {disposable.name}");
                }
            }
            
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