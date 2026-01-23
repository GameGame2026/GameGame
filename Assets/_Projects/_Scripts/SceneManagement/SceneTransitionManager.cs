using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace _Projects.SceneManagement
{
    public class SceneTransitionManager : MonoBehaviour
    {
        private static SceneTransitionManager _instance;
        public static SceneTransitionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("SceneTransitionManager");
                    _instance = go.AddComponent<SceneTransitionManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("场景转换设置")]
        [Tooltip("是否启用淡入淡出效果")]
        public bool useFadeEffect = true;
        
        [Tooltip("淡入淡出持续时间（秒）")]
        public float fadeDuration = 1f;

        private bool _isTransitioning = false;

        /// <summary>
        /// 当前是否���在转换场景
        /// </summary>
        public bool IsTransitioning => _isTransitioning;

        private void Awake()
        {
            // 确保只有一个实例
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 加载场景（通过场景名称）
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        public void LoadScene(string sceneName)
        {
            if (_isTransitioning)
            {
                Debug.LogWarning("场景转换正在进行中，请稍候...");
                return;
            }

            if (useFadeEffect)
            {
                StartCoroutine(LoadSceneWithFade(sceneName));
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }
        }

        /// <summary>
        /// 加载场景（通过场景索引）
        /// </summary>
        /// <param name="sceneIndex">场景索引</param>
        public void LoadScene(int sceneIndex)
        {
            if (_isTransitioning)
            {
                Debug.LogWarning("场景转换正在进行中，请稍候...");
                return;
            }

            if (useFadeEffect)
            {
                StartCoroutine(LoadSceneWithFade(sceneIndex));
            }
            else
            {
                SceneManager.LoadScene(sceneIndex);
            }
        }

        /// <summary>
        /// 重新加载当前场景
        /// </summary>
        public void ReloadCurrentScene()
        {
            LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>
        /// 加载下一个场景（按照 Build Settings 中的顺序）
        /// </summary>
        public void LoadNextScene()
        {
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                LoadScene(nextSceneIndex);
            }
            else
            {
                Debug.LogWarning("已经是最后一个场景了！");
            }
        }

        /// <summary>
        /// 异步加载场景（带淡入淡出效果）
        /// </summary>
        private IEnumerator LoadSceneWithFade(string sceneName)
        {
            _isTransitioning = true;

            // 淡出
            if (SceneTransitionUI.Instance != null)
            {
                yield return StartCoroutine(SceneTransitionUI.Instance.FadeOut(fadeDuration));
            }

            // 加载场景
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            
            // 等待场景加载完成
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // 淡入
            if (SceneTransitionUI.Instance != null)
            {
                yield return StartCoroutine(SceneTransitionUI.Instance.FadeIn(fadeDuration));
            }

            _isTransitioning = false;
        }

        /// <summary>
        /// 异步加载场景（带淡入淡出效果）
        /// </summary>
        private IEnumerator LoadSceneWithFade(int sceneIndex)
        {
            _isTransitioning = true;

            // 淡出
            if (SceneTransitionUI.Instance != null)
            {
                yield return StartCoroutine(SceneTransitionUI.Instance.FadeOut(fadeDuration));
            }

            // 加载场景
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
            
            // 等待场景加载完成
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // 淡入
            if (SceneTransitionUI.Instance != null)
            {
                yield return StartCoroutine(SceneTransitionUI.Instance.FadeIn(fadeDuration));
            }

            _isTransitioning = false;
        }

        /// <summary>
        /// 退出游戏
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("退出游戏");
            Application.Quit();
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}

