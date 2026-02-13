using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using _Projects._Scripts.SceneManagement;
using _Projects._Scripts.GamePlay.SaveSystem;
using TMPro;

namespace _Projects._Scripts.GamePlay.UI
{
    /// <summary>
    /// 死亡UI面板
    /// </summary>
    public class DeathUI : Singleton<DeathUI>
    {
        [Header("UI组件")]
        [Tooltip("死亡面板")]
        [SerializeField] private GameObject deathPanel;
        
        [Tooltip("继续游戏按钮")]
        [SerializeField] private Button continueButton;
        
        [Tooltip("返回主菜单按钮")]
        [SerializeField] private Button mainMenuButton;
        
        [Tooltip("死亡提示文本")]
        [SerializeField] private TextMeshProUGUI deathMessageText;
        
        [Header("设置")]
        [Tooltip("主菜单场景名称")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        
        [Tooltip("死亡提示文本")]
        [SerializeField] private string deathMessage = "你死了";
        
        [Tooltip("显示延迟（秒）")]
        [SerializeField] private float showDelay = 1f;
        
        [Header("音效")]
        [Tooltip("死亡音效")]
        [SerializeField] private AudioClip deathSound;
        
        [Tooltip("音效音量")]
        [SerializeField] [Range(0, 1)] private float soundVolume = 0.7f;
        
        private CanvasGroup _canvasGroup;
        private bool _isShowing;
        private int _currentSelectedIndex = 0; // 0=继续游戏, 1=返回主菜单
        private Button[] _buttons;

        protected override void Awake()
        {
            base.Awake();
            
            // 获取或添加 CanvasGroup
            _canvasGroup = deathPanel?.GetComponent<CanvasGroup>();
            if (_canvasGroup == null && deathPanel != null)
            {
                _canvasGroup = deathPanel.AddComponent<CanvasGroup>();
            }
            
            // 初始化按钮数组
            _buttons = new[] { continueButton, mainMenuButton };
            
            // 设置按钮事件
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueButtonClicked);
                AddPointerEnterEvent(continueButton, 0);
            }
            
            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
                AddPointerEnterEvent(mainMenuButton, 1);
            }
            
            // 初始隐藏
            HideDeathPanel();
        }
        
        /// <summary>
        /// 添加鼠标悬停事件
        /// </summary>
        private void AddPointerEnterEvent(Button button, int index)
        {
            EventTrigger trigger = button.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = button.gameObject.AddComponent<EventTrigger>();
            }
            
            // 添加鼠标进入事件
            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            entry.callback.AddListener((data) => { OnButtonHover(index); });
            trigger.triggers.Add(entry);
        }
        
        /// <summary>
        /// 按钮悬停回调
        /// </summary>
        private void OnButtonHover(int index)
        {
            if (_isShowing)
            {
                _currentSelectedIndex = index;
                UpdateButtonSelection();
                Debug.Log($"[DeathUI] 鼠标悬停选择: {(_currentSelectedIndex == 0 ? "继续游戏" : "返回主菜单")}");
            }
        }

        private void Start()
        {
            // 设置死亡提示文本
            if (deathMessageText != null)
            {
                deathMessageText.text = deathMessage;
            }
        }

        private void Update()
        {
            // 只有在死亡面板显示时才处理输入
            if (!_isShowing || deathPanel == null || !deathPanel.activeSelf) return;
            
            // W键或上箭头 - 向上选择
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                _currentSelectedIndex--;
                if (_currentSelectedIndex < 0) _currentSelectedIndex = _buttons.Length - 1;
                UpdateButtonSelection();
                Debug.Log($"[DeathUI] 选择: {(_currentSelectedIndex == 0 ? "继续游戏" : "返回主菜单")}");
            }
            
            // S键或下箭头 - 向下选择
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                _currentSelectedIndex++;
                if (_currentSelectedIndex >= _buttons.Length) _currentSelectedIndex = 0;
                UpdateButtonSelection();
                Debug.Log($"[DeathUI] 选择: {(_currentSelectedIndex == 0 ? "继续游戏" : "返回主菜单")}");
            }
            
            // 空格键或回车键 - 确认选择
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                ConfirmSelection();
            }
        }

        /// <summary>
        /// 显示死亡面板
        /// </summary>
        public void ShowDeathPanel()
        {
            if (_isShowing) return;
            
            _isShowing = true;
            
            // 播放死亡音效
            if (deathSound != null)
            {
                var mainCamera = UnityEngine.Camera.main;
                if (mainCamera != null)
                {
                    AudioSource.PlayClipAtPoint(deathSound, mainCamera.transform.position, soundVolume);
                }
            }
            
            // 使用协程代替Invoke，因为协程不受Time.timeScale影响
            StartCoroutine(ShowPanelWithDelay());
            
            Debug.Log("[DeathUI] 显示死亡面板");
        }
        
        /// <summary>
        /// 延迟显示面板的协程
        /// </summary>
        private System.Collections.IEnumerator ShowPanelWithDelay()
        {
            // 使用真实时间等待，不受Time.timeScale影响
            yield return new WaitForSecondsRealtime(showDelay);
            ShowPanelInternal();
        }

        /// <summary>
        /// 内部显示面板逻辑
        /// </summary>
        private void ShowPanelInternal()
        {
            // 先显示和解锁鼠标光标，在暂停游戏之前
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            
            if (deathPanel != null)
            {
                deathPanel.SetActive(true);
                
                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = 1f;
                    _canvasGroup.interactable = true;
                    _canvasGroup.blocksRaycasts = true;
                }
            }
            
            // 最后暂停游戏
            Time.timeScale = 0f;
            
            // 重置选择到第一个按钮
            _currentSelectedIndex = 0;
            UpdateButtonSelection();
            
            Debug.Log("[DeathUI] 死亡面板已显示，鼠标已解锁，游戏已暂停");
            Debug.Log("[DeathUI] 使用 W/S 键选择，空格/回车确认");
        }
        
        /// <summary>
        /// 更新按钮选择状态
        /// </summary>
        private void UpdateButtonSelection()
        {
            // 确保 EventSystem 存在
            if (EventSystem.current == null)
            {
                Debug.LogWarning("[DeathUI] EventSystem 不存在！");
                return;
            }
            
            for (int i = 0; i < _buttons.Length; i++)
            {
                if (_buttons[i] == null) continue;
                
                if (i == _currentSelectedIndex)
                {
                    // 选中的按钮 - 使用 EventSystem 选择
                    EventSystem.current.SetSelectedGameObject(_buttons[i].gameObject);
                    Debug.Log($"[DeathUI] 设置选中按钮: {_buttons[i].name}");
                }
            }
        }
        
        /// <summary>
        /// 确认当前选择
        /// </summary>
        private void ConfirmSelection()
        {
            Debug.Log($"[DeathUI] 确认选择: {_currentSelectedIndex}");
            
            if (_currentSelectedIndex == 0)
            {
                OnContinueButtonClicked();
            }
            else if (_currentSelectedIndex == 1)
            {
                OnMainMenuButtonClicked();
            }
        }

        /// <summary>
        /// 隐藏死亡面板
        /// </summary>
        public void HideDeathPanel()
        {
            _isShowing = false;
            
            if (deathPanel != null)
            {
                deathPanel.SetActive(false);
                
                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = 0f;
                    _canvasGroup.interactable = false;
                    _canvasGroup.blocksRaycasts = false;
                }
            }
            
            // 恢复游戏
            Time.timeScale = 1f;
            
            // 隐藏鼠标光标
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        /// <summary>
        /// 继续游戏按钮点击
        /// </summary>
        private void OnContinueButtonClicked()
        {
            Debug.Log("[DeathUI] 点击继续游戏");
            
            // 先恢复时间，确保场景加载正常
            Time.timeScale = 1f;
            
            // 隐藏死亡面板
            HideDeathPanel();
            
            // 从最后的存档点加载
            if (SavePointManager.Instance != null && SavePointManager.Instance.HasSaveData)
            {
                SavePointManager.Instance.LoadSave();
            }
            else
            {
                // 如果没有存档，重新加载当前场景
                Debug.LogWarning("[DeathUI] 没有存档数据，重新加载当前场景");
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        /// <summary>
        /// 返回主菜单按钮点击
        /// </summary>
        private void OnMainMenuButtonClicked()
        {
            Debug.Log("[DeathUI] 点击返回主菜单");
            
            // 恢复时间
            Time.timeScale = 1f;
            Debug.Log("[DeathUI] 时间已恢复");
            
            // 确保鼠标可见且未锁定（主菜单需要）
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Debug.Log($"[DeathUI] 光标状态设置 - Visible: {Cursor.visible}, LockState: {Cursor.lockState}");
            
            // 销毁持久化的Player对象（如果存在）
            DestroyPersistentPlayer();
            
            // 隐藏死亡面板（但不锁定光标）
            _isShowing = false;
            if (deathPanel != null)
            {
                deathPanel.SetActive(false);
                
                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = 0f;
                    _canvasGroup.interactable = false;
                    _canvasGroup.blocksRaycasts = false;
                }
            }
            Debug.Log("[DeathUI] 死亡面板已隐藏");
            
            // 加载主菜单场景
            Debug.Log($"[DeathUI] 准备加载主菜单场景: {mainMenuSceneName}");
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadScene(mainMenuSceneName);
            }
            else
            {
                SceneManager.LoadScene(mainMenuSceneName);
            }
        }
        
        /// <summary>
        /// 销毁持久化的Player对象
        /// </summary>
        private void DestroyPersistentPlayer()
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            
            foreach (GameObject player in players)
            {
                var dontDestroyManager = player.GetComponent<DontDestroyOnLoadManager>();
                if (dontDestroyManager != null)
                {
                    Debug.Log($"[DeathUI] 销毁持久化的Player: {player.name}");
                    Destroy(player);
                }
            }
        }

        /// <summary>
        /// 设置死亡提示文本
        /// </summary>
        public void SetDeathMessage(string message)
        {
            if (deathMessageText != null)
            {
                deathMessageText.text = message;
            }
        }
    }
}

