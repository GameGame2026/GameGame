using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace _Projects._Scripts.SceneManagement
{
    public class SceneTransitionManager : Singleton<SceneTransitionManager>
    {

        [Header("场景转换设置")]
        [Tooltip("是否启用淡入淡出效果")]
        public bool useFadeEffect = true;
        
        [Tooltip("淡入淡出持续时间（秒）")]
        public float fadeDuration = 1f;

        [Header("Player设置")]
        [Tooltip("Player在新场景中的生成点标签")]
        public string spawnPointTag = "PlayerSpawnPoint";

        private bool _isTransitioning;

        /// <summary>
        /// 当前是否���在转换场景
        /// </summary>
        public bool IsTransitioning => _isTransitioning;
        

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

            // 确保时间缩放正常
            Time.timeScale = 1f;

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

            // 确保时间缩放正常
            Time.timeScale = 1f;

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
            if (asyncLoad != null)
            {
                while (!asyncLoad.isDone)
                {
                    yield return null;
                }
            }

            // 场景加载完成后，处理Player位置
            yield return new WaitForEndOfFrame();
            SetupPlayerInNewScene();

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
            if (asyncLoad != null)
            {
                while (!asyncLoad.isDone)
                {
                    yield return null;
                }
            }

            // 场景加载完成后，处理Player位置
            yield return new WaitForEndOfFrame();
            SetupPlayerInNewScene();

            // 淡入
            if (SceneTransitionUI.Instance != null)
            {
                yield return StartCoroutine(SceneTransitionUI.Instance.FadeIn(fadeDuration));
            }

            _isTransitioning = false;
        }

        /// <summary>
        /// 在新场景中设置Player位置
        /// </summary>
        private void SetupPlayerInNewScene()
        {
            // 查找DontDestroyOnLoad的Player
            GameObject persistentPlayer = FindPersistentPlayer();
            
            if (persistentPlayer != null)
            {
                // 查找场景中的生成点
                GameObject spawnPoint = GameObject.FindGameObjectWithTag(spawnPointTag);
                
                if (spawnPoint != null)
                {
                    // 将Player移动到生成点
                    persistentPlayer.transform.position = spawnPoint.transform.position;
                    persistentPlayer.transform.rotation = spawnPoint.transform.rotation;
                    Debug.Log($"Player已移动到生成点: {spawnPoint.name}");
                }
                else
                {
                    Debug.LogWarning($"未找到标签为 '{spawnPointTag}' 的生成点，Player保持原位置");
                }

                // 清理场景中的重复Player
                CleanupScenePlayers(persistentPlayer);
            }
            else
            {
                Debug.LogWarning("未找到持久化的Player对象");
            }
        }

        /// <summary>
        /// 查找DontDestroyOnLoad的Player
        /// </summary>
        private GameObject FindPersistentPlayer()
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            
            foreach (GameObject player in players)
            {
                if (player.GetComponent<DontDestroyOnLoadManager>() != null)
                {
                    return player;
                }
            }
            
            return null;
        }

        /// <summary>
        /// 清理场景中的重复Player（保留持久化的）
        /// </summary>
        private void CleanupScenePlayers(GameObject persistentPlayer)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            
            foreach (GameObject player in players)
            {
                // 如果不是持久化Player，就销毁
                if (player != persistentPlayer)
                {
                    Debug.Log($"清理场景中的重复Player: {player.name}");
                    Destroy(player);
                }
            }
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

