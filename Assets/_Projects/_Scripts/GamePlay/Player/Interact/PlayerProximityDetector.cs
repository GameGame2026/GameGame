using _Projects.GamePlay;
using _Projects.GamePlay.Player;
using _Projects.GamePlay.Player.Controller;
using UnityEngine;

namespace GamePlay.Controller
{
    public class PlayerProximityDetector : MonoBehaviour
    {
        [Tooltip("Interact检测器")]
        public InteractCollider interactCollider;
        
        [Tooltip("Dispose检测器")]
        public DisposeCollider disposeCollider;

        /// <summary>
        /// 获取范围内的可互动NPC
        /// </summary>
        public InteractableNPC GetInteractable()
        {
            if (interactCollider == null)
            {
                Debug.LogWarning("[PlayerProximityDetector] interactCollider 未设置", this);
                return null;
            }
            return interactCollider.GetInteractable();
        }

        /// <summary>
        /// 获取范围内的可贴附物体
        /// </summary>
        public DisposableObject GetDisposable()
        {
            if (disposeCollider == null)
            {
                Debug.LogWarning("[PlayerProximityDetector] disposeCollider 未设置", this);
                return null;
            }
            return disposeCollider.GetDisposable();
        }
    }
}
