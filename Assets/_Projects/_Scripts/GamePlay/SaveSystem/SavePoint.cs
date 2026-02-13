using UnityEngine;

namespace _Projects._Scripts.GamePlay.SaveSystem
{
    /// <summary>
    /// 存档点 - 玩家靠近时自动触发存档
    /// </summary>
    public class SavePoint : MonoBehaviour
    {
        [Header("存档点设置")]
        [Tooltip("触发存档的范围")]
        [SerializeField] private float detectionRadius = 3f;
        
        [Tooltip("玩家标签")]
        [SerializeField] private string playerTag = "Player";
        
        [Header("材质设置")]
        [Tooltip("存档前的材质")]
        [SerializeField] private Material inactiveMaterial;
        
        [Tooltip("存档后的材质")]
        [SerializeField] private Material activeMaterial;
        
        [Tooltip("应用材质的 MeshRenderer")]
        [SerializeField] private MeshRenderer meshRenderer;
        
        [Header("视觉效果")]
        [Tooltip("是否启用旋转动画")]
        [SerializeField] private bool enableRotation = true;
        
        [Tooltip("旋转速度")]
        [SerializeField] private float rotationSpeed = 30f;
        
        [Tooltip("是否启用浮动动画")]
        [SerializeField] private bool enableFloating = true;
        
        [Tooltip("浮动速度")]
        [SerializeField] private float floatSpeed = 1f;
        
        [Tooltip("浮动幅度")]
        [SerializeField] private float floatAmount = 0.3f;
        
        [Header("音效")]
        [Tooltip("存档成功音效")]
        [SerializeField] private AudioClip saveSound;
        
        [Tooltip("音效音量")]
        [SerializeField] [Range(0, 1)] private float soundVolume = 0.7f;
        
        private bool _hasBeenActivated = false;
        private Vector3 _startPosition;
        private Transform _playerTransform;
        private bool _playerInRange = false;

        private void Start()
        {
            _startPosition = transform.position;
            
            // 自动查找 MeshRenderer
            if (meshRenderer == null)
            {
                meshRenderer = GetComponentInChildren<MeshRenderer>();
            }
            
            // 设置初始材质
            if (meshRenderer != null && inactiveMaterial != null)
            {
                meshRenderer.material = inactiveMaterial;
            }
        }

        private void Update()
        {
            // 检测玩家距离
            CheckPlayerProximity();
            
            // 播放动画效果
            PlayVisualEffects();
        }

        /// <summary>
        /// 检测玩家是否在范围内
        /// </summary>
        private void CheckPlayerProximity()
        {
            if (_hasBeenActivated) return;
            
            // 查找玩家
            if (_playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag(playerTag);
                if (player != null)
                {
                    _playerTransform = player.transform;
                }
                else
                {
                    return;
                }
            }
            
            // 检测距离
            float distance = Vector3.Distance(transform.position, _playerTransform.position);
            
            if (distance <= detectionRadius && !_playerInRange)
            {
                _playerInRange = true;
                OnPlayerEnterRange();
            }
            else if (distance > detectionRadius && _playerInRange)
            {
                _playerInRange = false;
            }
        }

        /// <summary>
        /// 玩家进入范围时触发
        /// </summary>
        private void OnPlayerEnterRange()
        {
            if (_hasBeenActivated) return;
            
            // 执行存档
            SaveGame();
        }

        /// <summary>
        /// 执行存档逻辑
        /// </summary>
        private void SaveGame()
        {
            _hasBeenActivated = true;
            
            // 保存游戏数据
            if (SavePointManager.Instance != null)
            {
                SavePointManager.Instance.SaveGame(_playerTransform.position, transform.position);
            }
            
            // 更换材质
            if (meshRenderer != null && activeMaterial != null)
            {
                meshRenderer.material = activeMaterial;
            }
            
            // 播放音效
            if (saveSound != null)
            {
                AudioSource.PlayClipAtPoint(saveSound, transform.position, soundVolume);
            }
            
            // 显示存档成功提示
            if (SaveNotificationUI.Instance != null)
            {
                SaveNotificationUI.Instance.ShowSaveNotification();
            }
            
            Debug.Log($"[SavePoint] 游戏已在存档点 {gameObject.name} 保存");
        }

        /// <summary>
        /// 播放视觉效果
        /// </summary>
        private void PlayVisualEffects()
        {
            // 旋转动画
            if (enableRotation)
            {
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            }
            
            // 浮动动画
            if (enableFloating)
            {
                float newY = _startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmount;
                transform.position = new Vector3(_startPosition.x, newY, _startPosition.z);
            }
        }

        /// <summary>
        /// 在Scene视图中显示检测范围
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _hasBeenActivated ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }

        /// <summary>
        /// 重置存档点（用于测试）
        /// </summary>
        public void ResetSavePoint()
        {
            _hasBeenActivated = false;
            if (meshRenderer != null && inactiveMaterial != null)
            {
                meshRenderer.material = inactiveMaterial;
            }
        }
    }
}

