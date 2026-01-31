using UnityEngine;

namespace _Projects.SceneManagement
{
    /// <summary>
    /// 该类用于在场景切换时保存和恢复玩家数据
    /// </summary>
    public class PlayerDataManager : Singleton<PlayerDataManager>
    {
        [Header("玩家基础数据")]
        public int currentHealth = 100;
        public int maxHealth = 100;
        public int score = 0;
        public int lives = 3;

        [Header("玩家位置数据")]
        public Vector3 lastPosition = Vector3.zero;
        public Quaternion lastRotation = Quaternion.identity;
        public string lastSceneName = "";

        [Header("游戏进度数据")]
        public int currentLevel = 1;
        public float playTime = 0f;
        

        /// <summary>
        /// 保存玩家当前状态
        /// </summary>
        public void SavePlayerState(GameObject player)
        {
            if (player == null)
            {
                Debug.LogWarning("尝试保存空的玩家对象");
                return;
            }

            // 保存位置和旋转
            lastPosition = player.transform.position;
            lastRotation = player.transform.rotation;
            lastSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            // 尝试获取玩家统计数据
            PlayerStats stats = player.GetComponent<PlayerStats>();
            if (stats != null)
            {
                // TODO:这里可以扩展保存更多数据
                Debug.Log("玩家统计数据已保存");
            }

            Debug.Log($"玩家状态已保存 - 位置: {lastPosition}, 场景: {lastSceneName}");
        }

        /// <summary>
        /// 恢复玩家状态到指定对象
        /// </summary>
        public void RestorePlayerState(GameObject player)
        {
            if (player == null)
            {
                Debug.LogWarning("尝试恢复到空的玩家对象");
                return;
            }

            // 恢复位置和旋转
            if (lastPosition != Vector3.zero)
            {
                player.transform.position = lastPosition;
                player.transform.rotation = lastRotation;
            }
            
            PlayerStats stats = player.GetComponent<PlayerStats>();
            if (stats != null)
            {
                // TODO:这里可以扩展保存更多数据
                Debug.Log("玩家统计数据已恢复");
            }

            Debug.Log($"玩家状态已恢复 - 位置: {lastPosition}");
        }

        /// <summary>
        /// 重置所有玩家数据（用于新游戏）
        /// </summary>
        public void ResetPlayerData()
        {
            currentHealth = maxHealth;
            score = 0;
            lives = 3;
            lastPosition = Vector3.zero;
            lastRotation = Quaternion.identity;
            lastSceneName = "";
            currentLevel = 1;
            playTime = 0f;

            Debug.Log("玩家数据已重置");
        }

        /// <summary>
        /// 增加分数
        /// </summary>
        public void AddScore(int amount)
        {
            score += amount;
            Debug.Log($"分数增加: +{amount}, 总分: {score}");
        }

        /// <summary>
        /// 受到伤害
        /// </summary>
        public void TakeDamage(int damage)
        {
            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);
            Debug.Log($"受到伤害: -{damage}, 当前生命值: {currentHealth}");

            if (currentHealth <= 0)
            {
                OnPlayerDeath();
            }
        }

        /// <summary>
        /// 治疗
        /// </summary>
        public void Heal(int amount)
        {
            currentHealth += amount;
            currentHealth = Mathf.Min(maxHealth, currentHealth);
            Debug.Log($"治疗: +{amount}, 当前生命值: {currentHealth}");
        }

        /// <summary>
        /// 玩家死亡处理
        /// </summary>
        private void OnPlayerDeath()
        {
            lives--;
            Debug.Log($"玩家死亡！剩余生命: {lives}");

            if (lives <= 0)
            {
                Debug.Log("游戏结束！");
                // 这里可以触发游戏结束逻辑
            }
            else
            {
                // 重置生命值准备重生
                currentHealth = maxHealth;
            }
        }

        /// <summary>
        /// 更新游戏时间
        /// </summary>
        private void Update()
        {
            playTime += Time.deltaTime;
        }

        /// <summary>
        /// 获取格式化的游戏时间
        /// </summary>
        public string GetFormattedPlayTime()
        {
            int hours = Mathf.FloorToInt(playTime / 3600);
            int minutes = Mathf.FloorToInt((playTime % 3600) / 60);
            int seconds = Mathf.FloorToInt(playTime % 60);

            return $"{hours:00}:{minutes:00}:{seconds:00}";
        }
    }
}

