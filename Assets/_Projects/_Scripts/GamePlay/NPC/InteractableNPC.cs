using UnityEngine;


namespace _Projects.GamePlay
{
    /// <summary>
    /// 可互动NPC - 可以与玩家进行交互的NPC
    /// </summary>
    public class InteractableNPC : MonoBehaviour
    {
        [Header("交互设置")]
        [Tooltip("交互UI预制体或图标")]
        public GameObject interactUI;
        
        [Tooltip("UI偏移位置")]
        public Vector3 uiOffset = new Vector3(0, 2, 0);
        
        private GameObject _uiInstance;
        private bool _isUIShowing = false;

        private void Awake()
        {
            // 如果有UI预制体，在初始化时创建但隐藏
            if (interactUI != null)
            {
                _uiInstance = Instantiate(interactUI, transform);
                _uiInstance.transform.localPosition = uiOffset;
                _uiInstance.SetActive(false);
            }
        }

        /// <summary>
        /// 显示可互动UI
        /// </summary>
        public void ShowInteractUI()
        {
            if (_uiInstance != null && !_isUIShowing)
            {
                _uiInstance.SetActive(true);
                _isUIShowing = true;
            }
        }

        /// <summary>
        /// 隐藏可互动UI
        /// </summary>
        public void HideInteractUI()
        {
            if (_uiInstance != null && _isUIShowing)
            {
                _uiInstance.SetActive(false);
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

        private void OnDestroy()
        {
            // 清理UI实例
            if (_uiInstance != null)
            {
                Destroy(_uiInstance);
            }
        }
    }
}

