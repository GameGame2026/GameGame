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
    /// 暂停UI面板
    /// </summary>
    public class PauseUI : Singleton<PauseUI>
    {
        [Header("主面板")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private TextMeshProUGUI pauseTitleText; // 可选标题

        [Header("确认对话框")]
        [SerializeField] private GameObject confirmPanel;
        [SerializeField] private Button confirmYesButton;
        [SerializeField] private Button confirmNoButton;
        [SerializeField] private TextMeshProUGUI confirmMessageText;

        [Header("设置")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private string confirmMessage = "确定要返回主界面吗？您会丢失已有的进度";

        private CanvasGroup _pauseCanvasGroup;
        private CanvasGroup _confirmCanvasGroup;
        private bool _isPaused = false;
        private bool _isConfirming = false; // 是否在确认对话框中

        // 按钮数组（主面板）
        private Button[] _mainButtons;
        // 按钮数组（确认面板）
        private Button[] _confirmButtons;

        private int _currentMainIndex = 0;      // 0=继续, 1=主菜单
        private int _currentConfirmIndex = 0;   // 0=确认, 1=取消

        protected override void Awake()
        {
            base.Awake();
            
            // 初始化主面板 CanvasGroup
            if (pausePanel != null)
            {
                _pauseCanvasGroup = pausePanel.GetComponent<CanvasGroup>();
                if (_pauseCanvasGroup == null)
                    _pauseCanvasGroup = pausePanel.AddComponent<CanvasGroup>();
            }
            // 初始化确认面板 CanvasGroup
            if (confirmPanel != null)
            {
                _confirmCanvasGroup = confirmPanel.GetComponent<CanvasGroup>();
                if (_confirmCanvasGroup == null)
                    _confirmCanvasGroup = confirmPanel.AddComponent<CanvasGroup>();
            }
            // 设置按钮数组
            _mainButtons = new[] { continueButton, mainMenuButton };
            _confirmButtons = new[] { confirmYesButton, confirmNoButton };

            // 设置主面板按钮事件
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueClicked);
                AddPointerEnterEvent(continueButton, 0, true);
            }
            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
                AddPointerEnterEvent(mainMenuButton, 1, true);
            }
            // 设置确认面板按钮事件
            if (confirmYesButton != null)
            {
                confirmYesButton.onClick.AddListener(OnConfirmYesClicked);
                AddPointerEnterEvent(confirmYesButton, 0, false);
            }
            if (confirmNoButton != null)
            {
                confirmNoButton.onClick.AddListener(OnConfirmNoClicked);
                AddPointerEnterEvent(confirmNoButton, 1, false);
            }
            // 设置确认消息文本
            if (confirmMessageText != null)
                confirmMessageText.text = confirmMessage;
            
            // 初始隐藏
            HidePausePanel();
        }
        

        private void Update()
        {
            // 检测 Esc 键（在游戏运行且没有暂停时才能暂停；如果已经在暂停中，按 Esc 继续或取消二级对话框）
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!_isPaused) Pause(); // 未暂停 → 暂停
                else
                {
                    if (_isConfirming)
                    {
                        // 如果在二级确认中，按 Esc 相当于取消（回到主暂停菜单）
                        HideConfirmPanel();
                    }
                    else
                    {
                        // 如果在主暂停菜单，按 Esc 继续游戏
                        Resume();
                    }
                }
            }

            // 只有在暂停状态且不在二级确认时才处理主面板的键盘选择
            if (_isPaused && !_isConfirming && pausePanel != null && pausePanel.activeSelf)
            {
                HandleMainPanelInput();
            }

            // 如果在二级确认状态，处理确认面板的键盘选择
            if (_isPaused && _isConfirming && confirmPanel != null && confirmPanel.activeSelf)
            {
                HandleConfirmPanelInput();
            }
        }

        /// <summary>
        /// 处理主面板键盘输入（W/S, 上下箭头, 空格/回车）
        /// </summary>
        private void HandleMainPanelInput()
        {
            // 上
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                _currentMainIndex--;
                if (_currentMainIndex < 0) _currentMainIndex = _mainButtons.Length - 1;
                UpdateMainButtonSelection();
            }
            // 下
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                _currentMainIndex++;
                if (_currentMainIndex >= _mainButtons.Length) _currentMainIndex = 0;
                UpdateMainButtonSelection();
            }
            // 确认
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (_currentMainIndex == 0)
                    OnContinueClicked();
                else
                    OnMainMenuClicked();
            }
        }

        /// <summary>
        /// 处理确认面板键盘输入
        /// </summary>
        private void HandleConfirmPanelInput()
        {
            // 上
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                _currentConfirmIndex--;
                if (_currentConfirmIndex < 0) _currentConfirmIndex = _confirmButtons.Length - 1;
                UpdateConfirmButtonSelection();
            }
            // 下
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                _currentConfirmIndex++;
                if (_currentConfirmIndex >= _confirmButtons.Length) _currentConfirmIndex = 0;
                UpdateConfirmButtonSelection();
            }
            // 同时保留空格/回车确认
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (_currentConfirmIndex == 0)
                    OnConfirmYesClicked();
                else
                    OnConfirmNoClicked();
            }
        }

        /// <summary>
        /// 为主面板按钮添加鼠标悬停事件
        /// </summary>
        private void AddPointerEnterEvent(Button button, int index, bool isMain)
        {
            EventTrigger trigger = button.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = button.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            // 使用闭包捕获 index 和 isMain
            entry.callback.AddListener((data) =>
            {
                if (isMain)
                    OnMainButtonHover(index);
                else
                    OnConfirmButtonHover(index);
            });
            trigger.triggers.Add(entry);
        }

        private void OnMainButtonHover(int index)
        {
            if (_isPaused && !_isConfirming)
            {
                _currentMainIndex = index;
                UpdateMainButtonSelection();
                Debug.Log($"Main index:{index}");
            }
        }

        private void OnConfirmButtonHover(int index)
        {
            if (_isPaused && _isConfirming)
            {
                _currentConfirmIndex = index;
                UpdateConfirmButtonSelection();
                Debug.Log($"Confirm index:{index}");
            }
        }

        /// <summary>
        /// 更新主面板按钮选中状态（高亮）
        /// </summary>
        private void UpdateMainButtonSelection()
        {
            if (EventSystem.current == null) return;

            for (int i = 0; i < _mainButtons.Length; i++)
            {
                if (_mainButtons[i] == null) continue;
                if (i == _currentMainIndex)
                {
                    EventSystem.current.SetSelectedGameObject(_mainButtons[i].gameObject);
                }
            }
        }

        /// <summary>
        /// 更新确认面板按钮选中状态
        /// </summary>
        private void UpdateConfirmButtonSelection()
        {
            if (EventSystem.current == null) return;

            for (int i = 0; i < _confirmButtons.Length; i++)
            {
                if (_confirmButtons[i] == null) continue;
                if (i == _currentConfirmIndex)
                {
                    EventSystem.current.SetSelectedGameObject(_confirmButtons[i].gameObject);
                }
            }
        }

        /// <summary>
        /// 暂停游戏
        /// </summary>
        public void Pause()
        {
            if (_isPaused) return;
            _isPaused = true;

            // 显示光标
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // 暂停时间
            Time.timeScale = 0f;

            // 显示主暂停面板
            ShowPausePanel();

            Debug.Log("[PauseUI] 游戏已暂停");
        }

        /// <summary>
        /// 继续游戏
        /// </summary>
        public void Resume()
        {
            if (!_isPaused) return;

            // 隐藏暂停面板
            HidePausePanel();

            // 恢复时间
            Time.timeScale = 1f;

            // 隐藏光标
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            _isPaused = false;
            _isConfirming = false;

            Debug.Log("[PauseUI] 游戏已继续");
        }

        /// <summary>
        /// 显示主暂停面板
        /// </summary>
        private void ShowPausePanel()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
                if (_pauseCanvasGroup != null)
                {
                    _pauseCanvasGroup.alpha = 1f;
                    _pauseCanvasGroup.interactable = true;
                    _pauseCanvasGroup.blocksRaycasts = true;
                }
            }
            // 确保确认面板隐藏
            if (confirmPanel != null) confirmPanel.SetActive(false);

            // 重置选择
            _currentMainIndex = 0;
            UpdateMainButtonSelection();
        }

        /// <summary>
        /// 显示确认对话框
        /// </summary>
        private void ShowConfirmPanel()
        {
            _isConfirming = true;

            // 隐藏主面板交互（但可保留显示，或者完全隐藏主面板？通常做法是让主面板变暗但交互禁用）
            if (_pauseCanvasGroup != null)
            {
                _pauseCanvasGroup.interactable = false;
                _pauseCanvasGroup.blocksRaycasts = false;
            }

            // 显示确认面板
            if (confirmPanel != null)
            {
                confirmPanel.SetActive(true);
                if (_confirmCanvasGroup != null)
                {
                    _confirmCanvasGroup.alpha = 1f;
                    _confirmCanvasGroup.interactable = true;
                    _confirmCanvasGroup.blocksRaycasts = true;
                }
            }

            // 重置确认面板选择
            _currentConfirmIndex = 0;
            UpdateConfirmButtonSelection();

            Debug.Log("[PauseUI] 显示确认对话框");
        }

        /// <summary>
        /// 隐藏确认对话框，返回主暂停菜单
        /// </summary>
        private void HideConfirmPanel()
        {
            _isConfirming = false;

            // 恢复主面板交互
            if (_pauseCanvasGroup != null)
            {
                _pauseCanvasGroup.interactable = true;
                _pauseCanvasGroup.blocksRaycasts = true;
            }

            // 隐藏确认面板
            if (confirmPanel != null)
            {
                confirmPanel.SetActive(false);
                if (_confirmCanvasGroup != null)
                {
                    _confirmCanvasGroup.interactable = false;
                    _confirmCanvasGroup.blocksRaycasts = false;
                }
            }

            // 重新高亮主面板当前按钮
            UpdateMainButtonSelection();

            Debug.Log("[PauseUI] 关闭确认对话框");
        }

        /// <summary>
        /// 隐藏暂停面板（两个一起隐藏）
        /// </summary>
        public void HidePausePanel()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
                if (_pauseCanvasGroup != null)
                {
                    _pauseCanvasGroup.alpha = 0f;
                    _pauseCanvasGroup.interactable = false;
                    _pauseCanvasGroup.blocksRaycasts = false;
                }
            }
            if (confirmPanel != null)
            {
                confirmPanel.SetActive(false);
                if (_confirmCanvasGroup != null)
                {
                    _confirmCanvasGroup.alpha = 0f;
                    _confirmCanvasGroup.interactable = false;
                    _confirmCanvasGroup.blocksRaycasts = false;
                }
            }
        }

        // /// <summary>
        // /// 显示死亡面板
        // /// </summary>
        // public void ShowDeathPanel()
        // {
        //     if (_isShowing) return;
            
        //     _isShowing = true;
            
        //     // 播放死亡音效
        //     if (deathSound != null)
        //     {
        //         var mainCamera = UnityEngine.Camera.main;
        //         if (mainCamera != null)
        //         {
        //             AudioSource.PlayClipAtPoint(deathSound, mainCamera.transform.position, soundVolume);
        //         }
        //     }
            
        //     // 使用协程代替Invoke，因为协程不受Time.timeScale影响
        //     StartCoroutine(ShowPanelWithDelay());
            
        //     Debug.Log("[DeathUI] 显示死亡面板");
        // }
        
        // /// <summary>
        // /// 延迟显示面板的协程
        // /// </summary>
        // private System.Collections.IEnumerator ShowPanelWithDelay()
        // {
        //     // 使用真实时间等待，不受Time.timeScale影响
        //     yield return new WaitForSecondsRealtime(showDelay);
        //     ShowPanelInternal();
        // }

       // ---------- 按钮点击事件 ----------
        private void OnContinueClicked()
        {
            if (_isConfirming) return;
            Resume();
        }

        private void OnMainMenuClicked()
        {
            if (_isConfirming) return;
            // 显示确认对话框
            ShowConfirmPanel();
        }

        private void OnConfirmYesClicked()
        {
            // 用户确认返回主菜单
            Debug.Log("[PauseUI] 确认返回主菜单");
            // 关闭面板
            HidePausePanel();

            // 恢复时间（以防万一）
            Time.timeScale = 1f;

            // 销毁持久化的Player对象
            DestroyPersistentPlayer();

            // 加载主菜单场景
            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.LoadScene(mainMenuSceneName);
            else
                SceneManager.LoadScene(mainMenuSceneName);
        }

        private void OnConfirmNoClicked()
        {
            // 用户取消，返回暂停主菜单
            HideConfirmPanel();
        }

        /// <summary>
        /// 销毁持久化的Player对象（与DeathUI中的逻辑一致）
        /// </summary>
        private void DestroyPersistentPlayer()
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                var dontDestroyManager = player.GetComponent<DontDestroyOnLoadManager>();
                if (dontDestroyManager != null)
                {
                    Debug.Log($"[PauseUI] 销毁持久化的Player: {player.name}");
                    Destroy(player);
                }
            }
        }
    }
}
