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

        // 2.12 静影:尝试加入对话提示（问号）
        [Header("UI 提示")]
        [SerializeField] private GameObject questMark;   // 拖入问号子物体
        [SerializeField] private bool hideAfterDialogue = true;  // 对话后是否永久隐藏
        private bool hasInteracted = false;

        private void Awake()
        {
            // 初始化时隐藏Point
            if (InteractUI != null)
            {
                InteractUI.SetActive(false);
            }

            // 初始化时隐藏问号
            if (questMark != null) questMark.SetActive(false);
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
            
            // 标记已开始对话
            hasInteracted = true;
            if (hideAfterDialogue) HideQuestMark();
        }

        /// <summary>
        /// 显示问号、隐藏问号（可由玩家触发器调用）
        /// </summary>
        public void ShowQuestMark()
        {
            if (questMark != null && !hasInteracted)  // 如果尚未对话过，才显示
            {
                questMark.SetActive(true);
            }
        }

        public void HideQuestMark()
        {
            if (questMark != null)
            {
                questMark.SetActive(false);
            }
        }
        
    }
}

