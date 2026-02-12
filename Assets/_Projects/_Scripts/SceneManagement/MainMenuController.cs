using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Projects._Scripts.SceneManagement
{
    /// <summary>
    /// 主菜单控制器 - 管理开始菜单的所有交互逻辑
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("场景设置")]
        [Tooltip("第一关的场景名称")]
        [SerializeField] private string firstLevelSceneName = "Scene1";

        [Header("按钮引用")]
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitGameButton;

        [Header("面板引用")]
        [SerializeField] private CreditsPanel creditsPanel;
        [SerializeField] private QuitConfirmationDialog quitDialog;

        [Header("音效设置")]
        [Tooltip("按钮点击音效")]
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioSource audioSource;

        private void Awake()
        {
            // 如果没有音频源，自动添加一个
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        private void Start()
        {
            // 绑定按钮事件
            if (startGameButton != null)
            {
                startGameButton.onClick.AddListener(OnStartGameClicked);
            }
            else
            {
                Debug.LogError("StartGameButton 未设置！");
            }

            if (creditsButton != null)
            {
                creditsButton.onClick.AddListener(OnCreditsClicked);
            }
            else
            {
                Debug.LogError("CreditsButton 未设置！");
            }

            if (quitGameButton != null)
            {
                quitGameButton.onClick.AddListener(OnQuitGameClicked);
            }
            else
            {
                Debug.LogError("QuitGameButton 未设置！");
            }

            // 确保面板初始状态是隐藏的
            if (creditsPanel != null)
            {
                creditsPanel.Hide();
            }

            if (quitDialog != null)
            {
                quitDialog.Hide();
            }
        }

        private void OnDestroy()
        {
            // 取消绑定按钮事件
            if (startGameButton != null)
            {
                startGameButton.onClick.RemoveListener(OnStartGameClicked);
            }

            if (creditsButton != null)
            {
                creditsButton.onClick.RemoveListener(OnCreditsClicked);
            }

            if (quitGameButton != null)
            {
                quitGameButton.onClick.RemoveListener(OnQuitGameClicked);
            }
        }

        /// <summary>
        /// 点击"开始游戏"按钮
        /// </summary>
        private void OnStartGameClicked()
        {
            PlayButtonSound();
            Debug.Log("开始游戏，加载场景: " + firstLevelSceneName);
            
            // 使用场景转换管理器加载场景（如果存在）
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadScene(firstLevelSceneName);
            }
            else
            {
                // 直接加载场景
                SceneManager.LoadScene(firstLevelSceneName);
            }
        }

        /// <summary>
        /// 点击"制作人员"按钮
        /// </summary>
        private void OnCreditsClicked()
        {
            PlayButtonSound();
            Debug.Log("打开制作人员面板");
            
            if (creditsPanel != null)
            {
                creditsPanel.Show();
            }
            else
            {
                Debug.LogError("CreditsPanel 未设置！");
            }
        }

        /// <summary>
        /// 点击"退出游戏"按钮
        /// </summary>
        private void OnQuitGameClicked()
        {
            PlayButtonSound();
            Debug.Log("显示退出确认对话框");
            
            if (quitDialog != null)
            {
                quitDialog.Show();
            }
            else
            {
                Debug.LogError("QuitConfirmationDialog 未设置！");
            }
        }

        /// <summary>
        /// 播放按钮音效
        /// </summary>
        private void PlayButtonSound()
        {
            if (audioSource != null && buttonClickSound != null)
            {
                audioSource.PlayOneShot(buttonClickSound);
            }
        }

        /// <summary>
        /// 公开方法：确认退出游戏
        /// </summary>
        public void ConfirmQuitGame()
        {
            Debug.Log("确认退出游戏");
            
#if UNITY_EDITOR
            // 在编辑器中停止播放
            UnityEditor.EditorApplication.isPlaying = false;
#else
            // 在构建版本中退出应用程序
            Application.Quit();
#endif
        }
    }
}

