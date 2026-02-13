using UnityEngine;

namespace _Projects._Scripts.GamePlay.Items
{
    /// <summary>
    /// 炸弹物体 - 实例化后3秒爆炸，对范围内的玩家造成伤害
    /// </summary>
    public class BombObject : MonoBehaviour
    {
        [Header("爆炸设置")]
        [Tooltip("爆炸延迟时间（秒）")]
        [SerializeField] private float explodeDelay = 3f;
        
        [Tooltip("爆炸伤害")]
        [SerializeField] private float explosionDamage = 10f;
        
        [Tooltip("爆炸范围半径")]
        [SerializeField] private float explosionRadius = 5f;
        
        [Header("特效设置")]
        [Tooltip("爆炸粒子特效")]
        [SerializeField] private ParticleSystem explosionParticle;
        
        [Tooltip("爆炸音效")]
        [SerializeField] private AudioClip explosionSound;
        
        [Tooltip("音效音量")]
        [SerializeField] [Range(0, 1)] private float soundVolume = 0.8f;
        
        [Header("可选设置")]
        [Tooltip("爆炸后销毁延迟时间（给粒子特效播放时间）")]
        [SerializeField] private float destroyDelay = 2f;
        
        [Tooltip("炸弹模型（爆炸时隐藏）")]
        [SerializeField] private GameObject bombModel;
        
        private float _timer;
        private bool _hasExploded;

        private void Start()
        {
            // 如果没有手动指定粒子特效，尝试自动查找
            if (explosionParticle == null)
            {
                explosionParticle = GetComponentInChildren<ParticleSystem>();
            }
            
            // 确保粒子特效初始时不播放
            if (explosionParticle != null)
            {
                explosionParticle.Stop();
            }
            
            _timer = explodeDelay;
        }

        private void Update()
        {
            if (_hasExploded) return;
            
            _timer -= Time.deltaTime;
            
            if (_timer <= 0)
            {
                Explode();
            }
        }

        /// <summary>
        /// 执行爆炸
        /// </summary>
        private void Explode()
        {
            if (_hasExploded) return;
            _hasExploded = true;
            
            Debug.Log($"[BombObject] 炸弹爆炸！位置: {transform.position}");
            
            // 播放爆炸粒子特效
            if (explosionParticle != null)
            {
                explosionParticle.Play();
            }
            
            // 播放爆炸音效
            if (explosionSound != null)
            {
                AudioSource.PlayClipAtPoint(explosionSound, transform.position, soundVolume);
            }
            
            // 隐藏炸弹模型
            if (bombModel != null)
            {
                bombModel.SetActive(false);
            }
            
            // 对范围内的玩家造成伤害
            DamagePlayersInRange();
            
            // 延迟销毁炸弹对象
            Destroy(gameObject, destroyDelay);
        }

        /// <summary>
        /// 对范围内的所有玩家造成伤害
        /// </summary>
        private void DamagePlayersInRange()
        {
            // 检测范围内的所有碰撞体
            Collider[] colliders = new Collider[10];
            int hitCount = Physics.OverlapSphereNonAlloc(transform.position, explosionRadius, colliders);
            
            for (int i = 0; i < hitCount; i++)
            {
                Collider col = colliders[i];
                
                // 检查是否是玩家
                PlayerStats player = col.GetComponent<PlayerStats>();
                if (player != null)
                {
                    // 计算距离衰减（可选）
                    float distance = Vector3.Distance(transform.position, col.transform.position);
                    float damageMultiplier = 1f - (distance / explosionRadius);
                    damageMultiplier = Mathf.Clamp01(damageMultiplier);
                    
                    // 造成伤害（可以选择使用衰减或固定伤害）
                    float finalDamage = explosionDamage * damageMultiplier;
                    player.TakeDamage(finalDamage);
                    
                    Debug.Log($"[BombObject] 对玩家造成 {finalDamage} 点伤害（距离: {distance:F2}m）");
                }
            }
        }

        /// <summary>
        /// 在编辑器中绘制爆炸范围
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}

