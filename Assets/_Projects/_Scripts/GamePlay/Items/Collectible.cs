using UnityEngine;

namespace _Projects._Scripts.GamePlay.Items
{
    /// <summary>
    /// 收集物类型
    /// </summary>
    public enum CollectibleType
    {
        AttackBoost,  // 攻击提升 - 叹号
        HealthBoost   // 血量提升 - 问号
    }

    /// <summary>
    /// 可收集物品 - 悬浮转圈，自动收集
    /// </summary>
    public class Collectible : MonoBehaviour
    {
        [Header("收集物类型")]
        [SerializeField] private CollectibleType collectibleType = CollectibleType.AttackBoost;

        [Header("视觉效果")]
        [Tooltip("旋转速度（度/秒）")]
        [SerializeField] private float rotationSpeed = 90f;

        [Tooltip("悬浮速度")]
        [SerializeField] private float floatSpeed = 1f;

        [Tooltip("悬浮幅度")]
        [SerializeField] private float floatAmplitude = 0.3f;

        [Header("收集设置")]
        [Tooltip("自动收集范围")]
        [SerializeField] private float collectRange = 2f;

        [Tooltip("收集音效")]
        [SerializeField] private AudioClip collectSound;

        [Tooltip("收集音效音量")]
        [SerializeField] [Range(0, 1)] private float collectVolume = 0.8f;

        [Header("特效")]
        [Tooltip("收集时的粒子特效")]
        [SerializeField] private GameObject collectEffectPrefab;

        private Vector3 _startPosition;
        private bool _isCollected;
        private Transform _playerTransform;

        public CollectibleType Type => collectibleType;

        private void Start()
        {
            _startPosition = transform.position;

            // 查找玩家
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }
        }

        private void Update()
        {
            if (_isCollected) return;

            // 旋转效果
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

            // 悬浮效果
            float newY = _startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            // 检测玩家距离
            if (_playerTransform != null)
            {
                float distance = Vector3.Distance(transform.position, _playerTransform.position);
                if (distance <= collectRange)
                {
                    Collect();
                }
            }
        }

        /// <summary>
        /// 收集物品
        /// </summary>
        private void Collect()
        {
            if (_isCollected) return;
            _isCollected = true;

            // 播放音效
            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, transform.position, collectVolume);
            }

            // 生成特效
            if (collectEffectPrefab != null)
            {
                Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
            }

            // 通知收集管理器
            if (CollectibleManager.Instance != null)
            {
                CollectibleManager.Instance.OnCollectibleCollected(this);
            }

            // 销毁物品
            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            // 在编辑器中显示收集范围
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, collectRange);
        }
    }
}

