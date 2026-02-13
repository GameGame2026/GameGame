using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Projects._Scripts.GamePlay.SaveSystem
{
    /// <summary>
    /// 存档点管理器 - 管理游戏存档数据
    /// </summary>
    public class SavePointManager : Singleton<SavePointManager>
    {
        [Header("存档数据")]
        private Vector3 _lastSavePosition;
        private Vector3 _lastSavePointPosition;
        private int _lastSaveSceneIndex;
        private bool _hasSaveData;

        /// <summary>
        /// 是否有存档数据
        /// </summary>
        public bool HasSaveData => _hasSaveData;

        /// <summary>
        /// 最后的存档位置
        /// </summary>
        public Vector3 LastSavePosition => _lastSavePosition;

        /// <summary>
        /// 保存游戏
        /// </summary>
        /// <param name="playerPosition">玩家当前位置</param>
        /// <param name="savePointPosition">存档点位置</param>
        public void SaveGame(Vector3 playerPosition, Vector3 savePointPosition)
        {
            _lastSavePosition = playerPosition;
            _lastSavePointPosition = savePointPosition;
            _lastSaveSceneIndex = SceneManager.GetActiveScene().buildIndex;
            _hasSaveData = true;

            // 可以在这里添加更多存档数据
            // 例如：玩家状态、物品、任务进度等
            
            Debug.Log($"[SavePointManager] 游戏已保存 - 位置: {playerPosition}, 场景: {_lastSaveSceneIndex}");
        }

        /// <summary>
        /// 加载存档（将玩家传送到最后的存档点）
        /// </summary>
        public void LoadSave()
        {
            if (!_hasSaveData)
            {
                Debug.LogWarning("[SavePointManager] 没有可用的存档数据");
                return;
            }

            // 如果在不同场景，先加载场景
            if (SceneManager.GetActiveScene().buildIndex != _lastSaveSceneIndex)
            {
                // 加载存档场景
                SceneManager.sceneLoaded += OnSaveSceneLoaded;
                SceneManager.LoadScene(_lastSaveSceneIndex);
            }
            else
            {
                // 在同一场景，直接传送玩家
                TeleportPlayerToSavePoint();
            }
        }

        /// <summary>
        /// 场景加载完成后的回调
        /// </summary>
        private void OnSaveSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnSaveSceneLoaded;
            TeleportPlayerToSavePoint();
        }

        /// <summary>
        /// 将玩家传送到存档点
        /// </summary>
        private void TeleportPlayerToSavePoint()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = _lastSavePosition;
                
                // 恢复玩家血量
                PlayerStats playerStats = player.GetComponent<PlayerStats>();
                if (playerStats != null)
                {
                    playerStats.Respawn();
                }
                
                Debug.Log($"[SavePointManager] 玩家已传送到存档点位置: {_lastSavePosition}");
            }
            else
            {
                Debug.LogWarning("[SavePointManager] 未找到玩家对象");
            }
        }

        /// <summary>
        /// 清除存档数据
        /// </summary>
        public void ClearSaveData()
        {
            _hasSaveData = false;
            _lastSavePosition = Vector3.zero;
            _lastSavePointPosition = Vector3.zero;
            _lastSaveSceneIndex = 0;
            Debug.Log("[SavePointManager] 存档数据已清除");
        }
    }
}

