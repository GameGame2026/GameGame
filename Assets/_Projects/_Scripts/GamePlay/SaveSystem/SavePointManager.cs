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
        private bool _isLoadingSave; // 标记是否正在从存档加载

        /// <summary>
        /// 是否有存档数据
        /// </summary>
        public bool HasSaveData => _hasSaveData;
        
        /// <summary>
        /// 是否正在从存档加载（用于防止其他系统干扰玩家位置）
        /// </summary>
        public bool IsLoadingSave => _isLoadingSave;

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

            _isLoadingSave = true; // 设置加载标记
            
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
                StartCoroutine(TeleportPlayerWithDelay());
            }
        }

        /// <summary>
        /// 场景加载完成后的回调
        /// </summary>
        private void OnSaveSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnSaveSceneLoaded;
            StartCoroutine(TeleportPlayerWithDelay());
        }

        /// <summary>
        /// 延迟传送玩家（确保在所有其他系统之后执行）
        /// </summary>
        private System.Collections.IEnumerator TeleportPlayerWithDelay()
        {
            // 等待一帧，确保场景完全加载
            yield return null;
            
            // 再等待一小段时间，确保其他系统（如SceneTransitionManager）完成初始化
            yield return new WaitForSeconds(0.1f);
            
            TeleportPlayerToSavePoint();
            
            // 清除加载标记
            _isLoadingSave = false;
        }
        
        /// <summary>
        /// 将玩家传送到存档点
        /// </summary>
        private void TeleportPlayerToSavePoint()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // 禁用CharacterController以便直接设置位置
                CharacterController characterController = player.GetComponent<CharacterController>();
                if (characterController != null)
                {
                    characterController.enabled = false;
                }
                
                player.transform.position = _lastSavePosition;
                
                // 重新启用CharacterController
                if (characterController != null)
                {
                    characterController.enabled = true;
                }
                
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

