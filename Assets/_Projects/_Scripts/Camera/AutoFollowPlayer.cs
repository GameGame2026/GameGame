using UnityEngine;
using Cinemachine;
using _Projects._Scripts.SceneManagement;

namespace _Projects.Camera
{
    /// <summary>
    /// 自动设置 Cinemachine 相机的 Follow 目标为 Player
    /// 如果 Follow 为空，会自动查找场景中的 Player 并设置
    /// 适用于场景切换时相机需要重新找到持久化的 Player
    /// </summary>
    [RequireComponent(typeof(CinemachineVirtualCamera))]
    public class AutoFollowPlayer : MonoBehaviour
    {
        [Header("设置")]
        [Tooltip("Player 的 Tag")]
        public string playerTag = "Player";
        
        [Tooltip("是否在每次 OnEnable 时都重新查找 Player（用于处理场景切换）")]
        public bool refreshOnEnable = true;
        
        [Tooltip("是否强制刷新（即使已有有效的 Follow 目标也重新设置）")]
        public bool forceRefresh = false;
        
        [Tooltip("是否查找 Player 的子对象作为跟随目标（例如 CinemachineCameraTarget）")]
        public bool useChildTarget = true;
        
        [Tooltip("子对象的名称（如果 useChildTarget 为 true）")]
        public string childTargetName = "PlayerCameraRoot";

        private CinemachineVirtualCamera _virtualCamera;

        private void Awake()
        {
            _virtualCamera = GetComponent<CinemachineVirtualCamera>();
            Debug.Log($"[AutoFollowPlayer] Awake - 找到虚拟相机组件");
        }

        private void Start()
        {
            Debug.Log($"[AutoFollowPlayer] Start 被调用");
            SetupFollowTarget();
        }

        private void OnEnable()
        {
            Debug.Log($"[AutoFollowPlayer] OnEnable 被调用，refreshOnEnable = {refreshOnEnable}");
            
            if (refreshOnEnable && _virtualCamera != null)
            {
                // 延迟执行，确保场景和 DontDestroyOnLoad 对象都已加载
                StartCoroutine(DelayedSetup());
            }
        }

        private System.Collections.IEnumerator DelayedSetup()
        {
            Debug.Log("[AutoFollowPlayer] 开始延迟设置...");
            
            // 等待多帧，确保 DontDestroyOnLoad 的对象已经准备好
            yield return null;
            yield return null;
            yield return new WaitForEndOfFrame();
            
            Debug.Log("[AutoFollowPlayer] 延迟等待完成，开始查找 Player");
            SetupFollowTarget();
        }

        /// <summary>
        /// 设置跟随目标
        /// </summary>
        private void SetupFollowTarget()
        {
            Debug.Log($"[AutoFollowPlayer] ===== SetupFollowTarget 开始 =====");
            
            if (_virtualCamera == null)
            {
                Debug.LogError("[AutoFollowPlayer] _virtualCamera 为 null！", this);
                return;
            }

            Debug.Log($"[AutoFollowPlayer] 虚拟相机: {_virtualCamera.name}");
            Debug.Log($"[AutoFollowPlayer] 当前 Follow: {(_virtualCamera.Follow == null ? "null" : _virtualCamera.Follow.name)}");
            Debug.Log($"[AutoFollowPlayer] forceRefresh: {forceRefresh}");

            // 如果强制刷新为 false，检查是否已有有效目标
            if (!forceRefresh && _virtualCamera.Follow != null)
            {
                try
                {
                    string name = _virtualCamera.Follow.name;
                    Debug.Log($"[AutoFollowPlayer] 已有有效 Follow 目标: {name}，跳过设置");
                    return;
                }
                catch
                {
                    Debug.Log("[AutoFollowPlayer] Follow 目标已销毁，将重新查找");
                }
            }

            // 查找所有带 Player Tag 的对象
            GameObject[] allPlayers = GameObject.FindGameObjectsWithTag(playerTag);
            Debug.Log($"[AutoFollowPlayer] 找到 {allPlayers.Length} 个带 '{playerTag}' 标签的对象");

            GameObject player = null;
            
            // 如果找到多个，尝试找到 DontDestroyOnLoad 的那个
            if (allPlayers.Length > 1)
            {
                Debug.Log("[AutoFollowPlayer] 找到多个 Player，尝试找到 DontDestroyOnLoad 的对象");
                foreach (var p in allPlayers)
                {
                    Debug.Log($"[AutoFollowPlayer] 检查 Player: {p.name}, 场景: {p.scene.name}");
                    if (p.scene.name == "DontDestroyOnLoad" || p.GetComponent<DontDestroyOnLoadManager>() != null)
                    {
                        player = p;
                        Debug.Log($"[AutoFollowPlayer] 找到 DontDestroyOnLoad 的 Player: {p.name}");
                        break;
                    }
                }
            }
            
            // 如果没找到特定的，就用第一个
            if (player == null && allPlayers.Length > 0)
            {
                player = allPlayers[0];
                Debug.Log($"[AutoFollowPlayer] 使用第一个找到的 Player: {player.name}");
            }
            
            if (player == null)
            {
                Debug.LogWarning($"[AutoFollowPlayer] 未找到标签为 '{playerTag}' 的 Player", this);
                return;
            }

            Transform targetTransform = null;

            // 如果需要查找子对象
            if (useChildTarget)
            {
                Debug.Log($"[AutoFollowPlayer] 开始查找子对象: {childTargetName}");
                Transform childTarget = FindChildRecursive(player.transform, childTargetName);
                
                if (childTarget != null)
                {
                    targetTransform = childTarget;
                    Debug.Log($"[AutoFollowPlayer] ✓ 找到子对象: {childTargetName}");
                }
                else
                {
                    Debug.LogWarning($"[AutoFollowPlayer] ✗ 未找到子对象 '{childTargetName}'，使用 Player 根对象");
                    targetTransform = player.transform;
                }
            }
            else
            {
                targetTransform = player.transform;
                Debug.Log($"[AutoFollowPlayer] 直接使用 Player 根对象");
            }

            // 设置 Follow 目标
            _virtualCamera.Follow = targetTransform;
            _virtualCamera.LookAt = targetTransform;

            Debug.Log($"[AutoFollowPlayer] ✓✓✓ 成功设置 Follow 为: {targetTransform.name} (来自 Player: {player.name})");
            Debug.Log($"[AutoFollowPlayer] ===== SetupFollowTarget 完成 =====");
        }

        /// <summary>
        /// 递归查找子对象
        /// </summary>
        private Transform FindChildRecursive(Transform parent, string childName)
        {
            // 先在直接子对象中查找
            Transform child = parent.Find(childName);
            if (child != null)
            {
                return child;
            }

            // 递归查找所有子对象
            foreach (Transform t in parent)
            {
                Transform result = FindChildRecursive(t, childName);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// 手动刷新跟随目标（可以从外部调用）
        /// </summary>
        public void RefreshFollowTarget()
        {
            if (_virtualCamera != null)
            {
                _virtualCamera.Follow = null;
                _virtualCamera.LookAt = null;
            }
            SetupFollowTarget();
        }
    }
}

