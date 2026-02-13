using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace _Projects._Scripts.GamePlay.Items
{
    /// <summary>
    /// 收集系统场景设置助手 - 用于快速检查场景配置
    /// 添加到场景中的任何物体上，会在Inspector显示检查结果
    /// </summary>
    public class CollectibleSystemChecker : MonoBehaviour
    {
#if UNITY_EDITOR
        [Header("场景检查")]
        [SerializeField] private bool autoCheck = true;

        private void OnValidate()
        {
            if (autoCheck && Application.isPlaying == false)
            {
                PerformCheck();
            }
        }

        [ContextMenu("检查场景配置")]
        private void PerformCheck()
        {
            Debug.Log("=== 收集系统场景配置检查 ===");

            // 检查 Player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogError("❌ 未找到 Player！请确保场景中有带 'Player' 标签的物体");
            }
            else
            {
                Debug.Log("✅ 找到 Player: " + player.name);

                PlayerStats stats = player.GetComponent<PlayerStats>();
                if (stats == null)
                {
                    Debug.LogError("❌ Player 没有 PlayerStats 组件！");
                }
                else
                {
                    Debug.Log($"✅ PlayerStats 配置正常 - 攻击:{stats.AttackDamage}, 生命:{stats.MaxHealth}");
                }
            }

            // 检查 CollectibleManager
            CollectibleManager manager = FindObjectOfType<CollectibleManager>();
            if (manager == null)
            {
                Debug.LogError("❌ 场景中没有 CollectibleManager！");
            }
            else
            {
                Debug.Log("✅ 找到 CollectibleManager");
            }

            // 检查 CollectibleUI
            CollectibleUI ui = FindObjectOfType<CollectibleUI>();
            if (ui == null)
            {
                Debug.LogWarning("⚠️ 场景中没有 CollectibleUI");
            }
            else
            {
                Debug.Log("✅ 找到 CollectibleUI");
            }

            // 检查 LevelSummaryPanel
            LevelSummaryPanel panel = FindObjectOfType<LevelSummaryPanel>();
            if (panel == null)
            {
                Debug.LogWarning("⚠️ 场景中没有 LevelSummaryPanel");
            }
            else
            {
                Debug.Log("✅ 找到 LevelSummaryPanel");
            }

            // 检查 LevelEndTrigger
            LevelEndTrigger trigger = FindObjectOfType<LevelEndTrigger>();
            if (trigger == null)
            {
                Debug.LogWarning("⚠️ 场景中没有 LevelEndTrigger");
            }
            else
            {
                Debug.Log("✅ 找到 LevelEndTrigger");
            }

            // 检查收集物
            Collectible[] collectibles = FindObjectsOfType<Collectible>();
            if (collectibles.Length == 0)
            {
                Debug.LogWarning("⚠️ 场景中没有收集物");
            }
            else
            {
                int attackCount = 0;
                int healthCount = 0;
                foreach (var c in collectibles)
                {
                    if (c.Type == CollectibleType.AttackBoost) attackCount++;
                    else if (c.Type == CollectibleType.HealthBoost) healthCount++;
                }
                Debug.Log($"✅ 找到 {collectibles.Length} 个收集物 (攻击:{attackCount}, 生命:{healthCount})");
            }

            Debug.Log("=== 检查完成 ===");
        }
#endif
    }

#if UNITY_EDITOR
    /// <summary>
    /// 自定义编辑器 - 添加快捷按钮
    /// </summary>
    [CustomEditor(typeof(CollectibleSystemChecker))]
    public class CollectibleSystemCheckerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            
            CollectibleSystemChecker checker = (CollectibleSystemChecker)target;

            if (GUILayout.Button("🔍 检查场景配置", GUILayout.Height(30)))
            {
                checker.SendMessage("PerformCheck");
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(
                "此工具用于检查收集系统是否正确配置。\n" +
                "点击上方按钮或查看 Console 获取检查结果。\n\n" +
                "需要的组件：\n" +
                "• Player (带 PlayerStats)\n" +
                "• CollectibleManager\n" +
                "• CollectibleUI (推荐)\n" +
                "• LevelSummaryPanel (推荐)\n" +
                "• LevelEndTrigger (推荐)\n" +
                "• Collectible 物品", 
                MessageType.Info);
        }
    }
#endif
}

