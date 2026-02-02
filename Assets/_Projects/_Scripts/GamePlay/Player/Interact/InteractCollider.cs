using System.Collections.Generic;
using UnityEngine;

namespace _Projects.GamePlay.Player
{
    public class InteractCollider : MonoBehaviour
    {
        // 范围内的所有可互动NPC
        private HashSet<InteractableNPC> _npcsInRange = new HashSet<InteractableNPC>();
        
        public InteractableNPC GetInteractable()
        {
            foreach (var npc in _npcsInRange)
            {
                if (npc != null)
                {
                    return npc;
                }
            }
            return null;
        }

        /// <summary>
        /// 触发范围内所有NPC的互动（如果需要的话）
        /// </summary>
        public void TriggerAllInteractables()
        {
            foreach (var npc in _npcsInRange)
            {
                if (npc != null)
                {
                    npc.TriggerAction();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("interactable"))
            {
                var npc = other.GetComponent<InteractableNPC>();
                if (npc != null)
                {
                    _npcsInRange.Add(npc);
                    Debug.Log($"[InteractCollider] NPC 进入范围: {npc.name}");
                }
            }
           
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("interactable"))
            {
                var npc = other.GetComponent<InteractableNPC>();
                if (npc != null)
                {
                    _npcsInRange.Remove(npc);
                    Debug.Log($"[InteractCollider] NPC 离开范围: {npc.name}");
                }
            }
            
        }

        // 可视化检测范围（在Scene视图中）
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                
                if (col is BoxCollider box)
                {
                    Gizmos.DrawCube(box.center, box.size);
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(box.center, box.size);
                }
                else if (col is SphereCollider sphere)
                {
                    Gizmos.DrawSphere(sphere.center, sphere.radius);
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(sphere.center, sphere.radius);
                }
            }
        }
    }
}