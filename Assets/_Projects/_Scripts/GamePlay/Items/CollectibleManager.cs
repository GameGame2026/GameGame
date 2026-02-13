using UnityEngine;

namespace _Projects._Scripts.GamePlay.Items
{
    /// <summary>
    /// 收集数据 - 用于记录本关收集情况
    /// </summary>
    [System.Serializable]
    public class CollectionData
    {
        public int attackBoostCount;
        public int healthBoostCount;
        public float totalAttackIncrease;
        public float totalHealthIncrease;

        public CollectionData()
        {
            attackBoostCount = 0;
            healthBoostCount = 0;
            totalAttackIncrease = 0f;
            totalHealthIncrease = 0f;
        }
    }

    /// <summary>
    /// 收集物管理器 - 管理所有收集物品和统计
    /// </summary>
    public class CollectibleManager : MonoBehaviour
    {
        public static CollectibleManager Instance { get; private set; }

        [Header("提升倍率")]
        [Tooltip("每个收集物的提升倍率（1.25%）")]
        [SerializeField] private float boostMultiplier = 0.0125f; // 1.25%

        [Header("UI引用")]
        [SerializeField] private CollectibleUI collectibleUI;

        // 当前关卡收集数据
        private CollectionData _currentLevelData = new CollectionData();

        // 玩家属性引用
        private PlayerStats _playerStats;

        // 事件
        public delegate void CollectibleCollectedHandler(CollectibleType type, int count);
        public event CollectibleCollectedHandler OnCollectibleCollectedEvent;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // 查找玩家
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerStats = player.GetComponent<PlayerStats>();
            }

            // 查找UI
            if (collectibleUI == null)
            {
                collectibleUI = FindObjectOfType<CollectibleUI>();
            }

            // 初始化当前关卡数据
            ResetLevelData();
        }

        /// <summary>
        /// 重置关卡数据（新关卡开始时调用）
        /// </summary>
        public void ResetLevelData()
        {
            _currentLevelData = new CollectionData();
        }

        /// <summary>
        /// 收集物品被收集时调用
        /// </summary>
        public void OnCollectibleCollected(Collectible collectible)
        {
            if (collectible == null) return;

            // 更新计数
            switch (collectible.Type)
            {
                case CollectibleType.AttackBoost:
                    _currentLevelData.attackBoostCount++;
                    break;
                case CollectibleType.HealthBoost:
                    _currentLevelData.healthBoostCount++;
                    break;
            }

            // 更新UI显示
            if (collectibleUI != null)
            {
                collectibleUI.UpdateCollectionCount(
                    _currentLevelData.attackBoostCount,
                    _currentLevelData.healthBoostCount
                );
            }

            // 触发事件
            OnCollectibleCollectedEvent?.Invoke(collectible.Type, GetCollectionCount(collectible.Type));

            Debug.Log($"[CollectibleManager] 收集 {collectible.Type}，当前数量: 攻击={_currentLevelData.attackBoostCount}, 血量={_currentLevelData.healthBoostCount}");
        }

        /// <summary>
        /// 获取指定类型的收集数量
        /// </summary>
        public int GetCollectionCount(CollectibleType type)
        {
            return type switch
            {
                CollectibleType.AttackBoost => _currentLevelData.attackBoostCount,
                CollectibleType.HealthBoost => _currentLevelData.healthBoostCount,
                _ => 0
            };
        }

        /// <summary>
        /// 关卡结束时应用收集效果
        /// </summary>
        public void ApplyCollectionBoosts()
        {
            if (_playerStats == null)
            {
                Debug.LogWarning("[CollectibleManager] 未找到玩家属性，无法应用提升！");
                return;
            }

            // 计算攻击力提升
            if (_currentLevelData.attackBoostCount > 0)
            {
                float attackIncrease = _playerStats.AttackDamage * boostMultiplier * _currentLevelData.attackBoostCount;
                _currentLevelData.totalAttackIncrease = attackIncrease;
                float newAttack = _playerStats.AttackDamage + attackIncrease;
                _playerStats.SetAttackDamage(newAttack);

                Debug.Log($"[CollectibleManager] 攻击力提升: +{attackIncrease:F2} -> {newAttack:F2}");
            }

            // 计算生命值提升
            if (_currentLevelData.healthBoostCount > 0)
            {
                float healthIncrease = _playerStats.MaxHealth * boostMultiplier * _currentLevelData.healthBoostCount;
                _currentLevelData.totalHealthIncrease = healthIncrease;
                float newMaxHealth = _playerStats.MaxHealth + healthIncrease;
                _playerStats.SetMaxHealth(newMaxHealth);
                _playerStats.Heal(healthIncrease); // 回复增加的血量

                Debug.Log($"[CollectibleManager] 生命值提升: +{healthIncrease:F2} -> {newMaxHealth:F2}");
            }
        }

        /// <summary>
        /// 获取当前关卡收集数据
        /// </summary>
        public CollectionData GetCurrentLevelData()
        {
            return _currentLevelData;
        }
    }
}

