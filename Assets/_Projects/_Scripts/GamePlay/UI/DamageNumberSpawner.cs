using UnityEngine;
using TMPro;

namespace _Projects.GamePlay.UI
{
    /// <summary>
    /// 伤害数字生成器 - 管理伤害数字的创建和显示
    /// 这是一个单例管理器，在场景中只需要一个实例
    /// </summary>
    public class DamageNumberSpawner : Singleton<DamageNumberSpawner>
    {
        
        [Header("预制体设置")]
        [Tooltip("伤害数字预制体（可为空，会自动创建）")]
        [SerializeField] private GameObject damageNumberPrefab;
        
        [Header("Canvas设置")]
        [Tooltip("用于显示伤害数字的Canvas（可为空，会自动查找）")]
        [SerializeField] private Canvas worldCanvas;
        
        [Header("颜色设置")]
        [SerializeField] private Color playerDamageColor = new Color(1f, 0.3f, 0.3f); // 玩家受伤颜色（红色）
        [SerializeField] private Color enemyDamageColor = new Color(1f, 1f, 0.3f); // 敌人受伤颜色（黄色）
        [SerializeField] private Color criticalColor = new Color(1f, 0.5f, 0f); // 暴击颜色（橙色）
        
        [Header("字体设置")]
        [SerializeField] private TMP_FontAsset font;
        [SerializeField] private float fontSize = 36f;
        
        private UnityEngine.Camera _mainCamera;
        
        private void Awake()
        {
            _mainCamera = UnityEngine.Camera.main;
            
            // 如果没有指定Canvas，尝试查找或创建
            if (worldCanvas == null)
            {
                worldCanvas = FindObjectOfType<Canvas>();
                
                if (worldCanvas == null)
                {
                    CreateWorldCanvas();
                }
            }
            
            // 如果没有预制体，创建一个默认的
            if (damageNumberPrefab == null)
            {
                CreateDefaultPrefab();
            }
        }
        
        /// <summary>
        /// 创建世界空间Canvas
        /// </summary>
        private void CreateWorldCanvas()
        {
            GameObject canvasObj = new GameObject("DamageNumberCanvas");
            worldCanvas = canvasObj.AddComponent<Canvas>();
            worldCanvas.renderMode = RenderMode.WorldSpace;
            
            var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10f;
            
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // 设置Canvas大小
            RectTransform rectTransform = canvasObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(1920, 1080);
            rectTransform.localScale = Vector3.one * 0.01f; // 缩小以适应世界空间
            
            Debug.Log("[DamageNumberSpawner] 已创建世界空间Canvas");
        }
        
        /// <summary>
        /// 创建默认的伤害数字预制体
        /// </summary>
        private void CreateDefaultPrefab()
        {
            GameObject prefab = new GameObject("DamageNumber");
            
            // 添加RectTransform
            RectTransform rectTransform = prefab.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 100);
            
            // 添加TextMeshProUGUI
            TextMeshProUGUI textMesh = prefab.AddComponent<TextMeshProUGUI>();
            textMesh.fontSize = fontSize;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.enableWordWrapping = false;
            textMesh.font = font; // 可能为null，使用默认字体
            textMesh.fontStyle = FontStyles.Bold;
            textMesh.outlineWidth = 0.2f;
            textMesh.outlineColor = Color.black;
            
            // 添加CanvasGroup
            prefab.AddComponent<CanvasGroup>();
            
            // 添加DamageNumber组件
            prefab.AddComponent<DamageNumber>();
            
            damageNumberPrefab = prefab;
            
            Debug.Log("[DamageNumberSpawner] 已创建默认伤害数字预制体");
        }
        
        /// <summary>
        /// 在世界位置生成伤害数字
        /// </summary>
        /// <param name="damage">伤害值</param>
        /// <param name="worldPosition">世界坐标位置</param>
        /// <param name="isPlayer">是否是玩家受到伤害</param>
        public void SpawnDamage(float damage, Vector3 worldPosition, bool isPlayer = false)
        {
            if (damageNumberPrefab == null || worldCanvas == null)
            {
                Debug.LogWarning("[DamageNumberSpawner] 缺少必要组件，无法生成伤害数字");
                return;
            }
            
            // 创建伤害数字实例
            GameObject instance = Instantiate(damageNumberPrefab, worldCanvas.transform);
            
            // 设置位置（世界空间转Canvas空间）
            RectTransform rectTransform = instance.GetComponent<RectTransform>();
            Vector3 canvasPosition = worldPosition;
            canvasPosition.y += 1.5f; // 在目标上方显示
            rectTransform.position = canvasPosition;
            
            // 初始化颜色
            Color damageColor = isPlayer ? playerDamageColor : enemyDamageColor;
            
            DamageNumber damageNumber = instance.GetComponent<DamageNumber>();
            if (damageNumber != null)
            {
                damageNumber.Initialize(damage, damageColor);
            }
        }
        
        /// <summary>
        /// 在屏幕空间生成伤害数字
        /// </summary>
        public void SpawnDamageScreenSpace(float damage, Vector2 screenPosition, bool isPlayer = false)
        {
            if (_mainCamera == null)
            {
                _mainCamera = UnityEngine.Camera.main;
            }
            
            if (_mainCamera != null)
            {
                Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10f));
                SpawnDamage(damage, worldPos, isPlayer);
            }
        }
    }
}

