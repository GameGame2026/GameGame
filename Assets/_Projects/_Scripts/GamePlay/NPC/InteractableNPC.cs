using UnityEngine;
using UnityEngine.Serialization;


namespace _Projects.GamePlay
{
    /// <summary>
    /// 可互动NPC - 可以与玩家进行交互的NPC
    /// </summary>
    public class InteractableNPC : MonoBehaviour
    {
        [Header("交互设置")]
        [Tooltip("交互UI GameObject")]
        public GameObject InteractUI;
        
        private bool _isUIShowing = false;

        private void Awake()
        {
            // 初始化时隐藏Point
            if (InteractUI != null)
            {
                InteractUI.SetActive(false);
            }
        }

        /// <summary>
        /// 显示可互动UI
        /// </summary>
        public void ShowInteractUI()
        {
            if (InteractUI != null && !_isUIShowing)
            {
                InteractUI.SetActive(true);
                _isUIShowing = true;
            }
        }

        /// <summary>
        /// 隐藏可互动UI
        /// </summary>
        public void HideInteractUI()
        {
            if (InteractUI != null && _isUIShowing)
            {
                InteractUI.SetActive(false);
                _isUIShowing = false;
            }
        }

        /// <summary>
        /// 触发交互动作（如对话）
        /// </summary>
        public void TriggerAction()
        {
            Debug.Log($"与 {gameObject.name} 开始互动");
            
            // TODO: 在这里调用对话系统
            // 例如: DialogueManager.Instance.StartDialogue(dialogueData);
            
            // 互动开始后可以隐藏UI
            HideInteractUI();
        }
    }
}

