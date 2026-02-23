using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using _Projects._Scripts.SceneManagement;

namespace _Projects._Scripts.GamePlay.UI
{
    /// <summary>
    /// 玩家状态栏UI - 显示血条、点数和头像
    /// </summary>
    public class StatsBar : MonoBehaviour
    {
        [Header("玩家引用")]
        [Tooltip("玩家的PlayerStats组件")]
        [SerializeField] private PlayerStats playerStats;
        
        [Header("UI组件 - 血条")]
        [Tooltip("血条填充图像（使用Image的Fill Amount）")]
        [SerializeField] private Image healthBarFill;
        
        [Tooltip("血条背景图像")]
        [SerializeField] private Image healthBarBackground;
        
        [Tooltip("当前血量文本")]
        [SerializeField] private TextMeshProUGUI healthText;
        
        [Header("UI组件 - 点数")]
        [Tooltip("点数文本")]
        [SerializeField] private TextMeshProUGUI pointsText;
        
        [Tooltip("点数图标父对象")]
        [SerializeField] private Transform pointIconsContainer;
        
        [Tooltip("点数图标预制体（可选）")]
        [SerializeField] private GameObject pointIconPrefab;
        
        [Header("UI组件 - 头像")]
        [Tooltip("玩家头像图像")]
        [SerializeField] private Image avatarImage;
        
        [Tooltip("头像边框图像")]
        [SerializeField] private Image avatarBorder;
        
        [Header("外观设置")]
        [Tooltip("血条颜色 - 健康状态")]
        [SerializeField] private Color healthyColor = new Color(0.2f, 1f, 0.2f); // 绿色
        
        [Tooltip("血条颜色 - 中等状态")]
        [SerializeField] private Color warningColor = new Color(1f, 0.8f, 0.2f); // 黄色
        
        [Tooltip("血条颜色 - 危险状态")]
        [SerializeField] private Color dangerColor = new Color(1f, 0.2f, 0.2f); // 红色
        
        [Tooltip("中等血量阈值（百分比）")]
        [SerializeField] [Range(0, 1)] private float warningThreshold = 0.5f;
        
        [Tooltip("危险血量阈值（百分比）")]
        [SerializeField] [Range(0, 1)] private float dangerThreshold = 0.25f;
        
        [Header("动画设置")]
        [Tooltip("血条变化平滑速度")]
        [SerializeField] private float healthBarSmoothSpeed = 5f;
        
        [Tooltip("是否启用血量变化动画")]
        [SerializeField] private bool enableHealthAnimation = true;
        
        [Tooltip("低血量时是否闪烁")]
        [SerializeField] private bool enableLowHealthFlash = true;
        
        [Tooltip("低血量闪烁速度")]
        [SerializeField] private float flashSpeed = 2f;
        
        // 内部变量
        private float _targetHealthFill;
        private float _currentHealthFill;
        private Image[] _pointIcons;
        
        private void Awake()
        {
            // 如果没有指定playerStats，尝试在场景中查找
            if (playerStats == null)
            {
                playerStats = FindObjectOfType<PlayerStats>();
                if (playerStats == null)
                {
                    Debug.LogError("[StatsBar] 未找到PlayerStats组件！");
                }
            }
            
            // 自动配置血条填充为 Filled 类型
            ConfigureHealthBarFill();
        }
        
        /// <summary>
        /// 自动配置血条填充图像为 Filled 类型
        /// </summary>
        private void ConfigureHealthBarFill()
        {
            if (healthBarFill == null) return;
            
            // 确保 Image 类型设置为 Filled
            if (healthBarFill.type != Image.Type.Filled)
            {
                healthBarFill.type = Image.Type.Filled;
                Debug.Log("[StatsBar] 已自动将 HealthBarFill 的 Image Type 设置为 Filled");
            }
            
            // 设置填充方式为横向
            if (healthBarFill.fillMethod != Image.FillMethod.Horizontal)
            {
                healthBarFill.fillMethod = Image.FillMethod.Horizontal;
                Debug.Log("[StatsBar] 已设置 Fill Method 为 Horizontal");
            }
            
            // 设置填充起点为左侧
            if (healthBarFill.fillOrigin != (int)Image.OriginHorizontal.Left)
            {
                healthBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
                Debug.Log("[StatsBar] 已设置 Fill Origin 为 Left");
            }
            
            // 设置初始填充量为满
            healthBarFill.fillAmount = 1f;
        }
        
        private void OnEnable()
        {
            // 订阅玩家事件
            if (playerStats != null)
            {   
                playerStats.OnHealthChanged.AddListener(UpdateHealth);
                playerStats.OnPointsChanged.AddListener(UpdatePoints);
            }

            // 订阅场景加载事件
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        private void OnDisable()
        {
            // 取消订阅场景加载
            SceneManager.sceneLoaded -= OnSceneLoaded;

            // 取消订阅事件
            if (playerStats != null)
            {
                playerStats.OnHealthChanged.RemoveListener(UpdateHealth);
                playerStats.OnPointsChanged.RemoveListener(UpdatePoints);
            }
        }
        
        private void Start()
        {
            // 初始化UI
            InitializeUI();
        }
        
        private void Update()
        {
            // 平滑更新血条
            if (enableHealthAnimation && healthBarFill != null)
            {
                _currentHealthFill = Mathf.Lerp(_currentHealthFill, _targetHealthFill, Time.deltaTime * healthBarSmoothSpeed);
                healthBarFill.fillAmount = _currentHealthFill;
            }
            
            // 低血量闪烁效果
            if (enableLowHealthFlash && healthBarFill != null && playerStats != null)
            {
                if (playerStats.HealthPercent <= dangerThreshold)
                {
                    float alpha = Mathf.Lerp(0.5f, 1f, (Mathf.Sin(Time.time * flashSpeed) + 1f) / 2f);
                    Color flashColor = healthBarFill.color;
                    flashColor.a = alpha;
                    healthBarFill.color = flashColor;
                }
                else
                {
                    Color normalColor = healthBarFill.color;
                    normalColor.a = 1f;
                    healthBarFill.color = normalColor;
                }
            }
        }

        // private PlayerStats FindPersistentPlayerStats()
        // {
        //     var players = FindObjectsOfType<PlayerStats>();
        //     foreach (var p in players)
        //     {
        //         if (p.GetComponent<DontDestroyOnLoadManager>() != null)
        //             return p;
        //     }
        //     return null; // 如果没有持久化玩家，则返回第一个找到的
        // }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Scene_1")
            {
                playerStats = FindObjectOfType<PlayerStats>(); // 降级
                if (playerStats != null)
                {
                    // 先取消可能存在的旧订阅（安全起见）
                    playerStats.OnHealthChanged.RemoveListener(UpdateHealth);
                    playerStats.OnPointsChanged.RemoveListener(UpdatePoints);
                    
                    // 重新订阅
                    playerStats.OnHealthChanged.AddListener(UpdateHealth);
                    playerStats.OnPointsChanged.AddListener(UpdatePoints);
                    
                    // 立即同步当前血量与点数
                    UpdateHealth(playerStats.Health);
                    UpdatePoints(playerStats.Points);
                }
                else
                {
                    Debug.LogWarning("[StatsBar] 场景加载后未找到 PlayerStats");
                }
            }
        }
        
        /// <summary>
        /// 初始化UI
        /// </summary>
        private void InitializeUI()
        {
            if (playerStats == null) return;
            
            // 初始化血条
            UpdateHealth(playerStats.Health);
            Debug.Log("StatsBar: 已更新玩家血条"); // 好像从主菜单进第一关的时候不会初始化……
            
            // 初始化点数
            UpdatePoints(playerStats.Points);
            
            // 初始化点数图标（如果有预制体）
            if (pointIconPrefab != null && pointIconsContainer != null)
            {
                InitializePointIcons();
            }
        }
        
        /// <summary>
        /// 初始化点数图标
        /// </summary>
        private void InitializePointIcons()
        {
            // 清除现有图标
            foreach (Transform child in pointIconsContainer)
            {
                Destroy(child.gameObject);
            }
            
            // 创建新图标
            _pointIcons = new Image[playerStats.MaxPoints];
            for (int i = 0; i < playerStats.MaxPoints; i++)
            {
                GameObject iconObj = Instantiate(pointIconPrefab, pointIconsContainer);
                _pointIcons[i] = iconObj.GetComponent<Image>();
                
                if (_pointIcons[i] == null)
                {
                    _pointIcons[i] = iconObj.GetComponentInChildren<Image>();
                }
            }
            
            UpdatePointIcons(playerStats.Points);
        }
        
        /// <summary>
        /// 更新血量显示
        /// </summary>
        /// <param name="currentHealth">当前血量</param>
        private void UpdateHealth(float currentHealth)
        {
            if (playerStats == null) 
            {
                Debug.Log("[StatsBar]playerStats不存在，无法赋值");
                return;
            }
            // 更新血条填充量
            _targetHealthFill = playerStats.HealthPercent;
            
            if (!enableHealthAnimation && healthBarFill != null)
            {
                _currentHealthFill = _targetHealthFill;
                healthBarFill.fillAmount = _currentHealthFill;
            }
            
            // 更新血条颜色
            UpdateHealthBarColor(playerStats.HealthPercent);
            
            // 更新血量文本
            if (healthText != null)
            {
                healthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(playerStats.MaxHealth)}";
            }
        }
        
        /// <summary>
        /// 更新血条颜色
        /// </summary>
        /// <param name="healthPercent">血量百分比</param>
        private void UpdateHealthBarColor(float healthPercent)
        {
            if (healthBarFill == null) return;
            
            Color targetColor;
            
            if (healthPercent <= dangerThreshold)
            {
                targetColor = dangerColor;
            }
            else if (healthPercent <= warningThreshold)
            {
                // 在警告色和危险色之间插值
                float t = (healthPercent - dangerThreshold) / (warningThreshold - dangerThreshold);
                targetColor = Color.Lerp(dangerColor, warningColor, t);
            }
            else
            {
                // 在健康色和警告色之间插值
                float t = (healthPercent - warningThreshold) / (1f - warningThreshold);
                targetColor = Color.Lerp(warningColor, healthyColor, t);
            }
            
            healthBarFill.color = targetColor;
        }
        
        /// <summary>
        /// 更新点数显示
        /// </summary>
        /// <param name="currentPoints">当前点数</param>
        private void UpdatePoints(int currentPoints)
        {
            if (playerStats == null) return;
            
            // 更新点数文本
            if (pointsText != null)
            {
                pointsText.text = $"{currentPoints} / {playerStats.MaxPoints}";
            }
            
            // 更新点数图标
            if (_pointIcons != null && _pointIcons.Length > 0)
            {
                UpdatePointIcons(currentPoints);
            }
        }
        
        /// <summary>
        /// 更新点数图标显示
        /// </summary>
        /// <param name="currentPoints">当前点数</param>
        private void UpdatePointIcons(int currentPoints)
        {
            if (_pointIcons == null) return;
            
            for (int i = 0; i < _pointIcons.Length; i++)
            {
                if (_pointIcons[i] != null)
                {
                    // 激活或禁用图标（也可以改变颜色或透明度）
                    _pointIcons[i].enabled = i < currentPoints;
                    
                    // 或者使用透明度
                    // Color color = _pointIcons[i].color;
                    // color.a = i < currentPoints ? 1f : 0.3f;
                    // _pointIcons[i].color = color;
                }
            }
        }
        
        /// <summary>
        /// 设置头像图片
        /// </summary>
        /// <param name="sprite">头像精灵图</param>
        public void SetAvatar(Sprite sprite)
        {
            if (avatarImage != null)
            {
                avatarImage.sprite = sprite;
            }
        }
        
        /// <summary>
        /// 设置头像边框颜色
        /// </summary>
        /// <param name="color">边框颜色</param>
        public void SetAvatarBorderColor(Color color)
        {
            if (avatarBorder != null)
            {
                avatarBorder.color = color;
            }
        }
        
        /// <summary>
        /// 显示/隐藏状态栏
        /// </summary>
        /// <param name="show">是否显示</param>
        public void Show(bool show)
        {
            gameObject.SetActive(show);
        }
    }
}

