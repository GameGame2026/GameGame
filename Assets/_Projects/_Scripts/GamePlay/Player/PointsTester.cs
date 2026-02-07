using UnityEngine;

/// <summary>
/// 点数测试辅助脚本 - 用于快速测试点数变化和动画切换
/// 在Player上添加此组件，按数字键1-4可以快速切换点数
/// </summary>
public class PointsTester : MonoBehaviour
{
    private PlayerStats _playerStats;
    
    [Header("测试设置")]
    [Tooltip("是否启用键盘测试")]
    public bool enableKeyboardTest = true;
    
    [Tooltip("是否在Inspector中显示实时信息")]
    public bool showDebugInfo = true;
    
    [Header("当前状态（只读）")]
    [SerializeField] private int currentPoints = 0;
    
    private void Start()
    {
        _playerStats = GetComponent<PlayerStats>();
        
        if (_playerStats == null)
        {
            Debug.LogError("[PointsTester] 未找到 PlayerStats 组件！", this);
            enabled = false;
            return;
        }
        
        // 订阅点数变化事件来更新显示
        _playerStats.OnPointsChanged.AddListener(OnPointsChanged);
        currentPoints = _playerStats.Points;
        
        Debug.Log("[PointsTester] 测试脚本已启动！按键说明：");
        Debug.Log("  - 按 0: 设置点数为 0");
        Debug.Log("  - 按 1: 设置点数为 1");
        Debug.Log("  - 按 2: 设置点数为 2");
        Debug.Log("  - 按 3: 设置点数为 3");
        Debug.Log("  - 按 +: 增加 1 点");
        Debug.Log("  - 按 -: 减少 1 点");
    }
    
    private void Update()
    {
        if (!enableKeyboardTest || _playerStats == null) return;
        
        // 数字键直接设置点数
        if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
        {
            _playerStats.SetPoints(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            _playerStats.SetPoints(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            _playerStats.SetPoints(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            _playerStats.SetPoints(3);
        }
        
        // + 号增加点数
        else if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            _playerStats.AddPoints(1);
        }
        
        // - 号减少点数
        else if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            _playerStats.RemovePoints(1);
        }
    }
    
    private void OnPointsChanged(int newPoints)
    {
        currentPoints = newPoints;
        
        if (showDebugInfo)
        {
            Debug.Log($"[PointsTester] ===== 点数已更新: {newPoints} =====");
        }
    }
    
    private void OnDestroy()
    {
        if (_playerStats != null)
        {
            _playerStats.OnPointsChanged.RemoveListener(OnPointsChanged);
        }
    }
    
    // 在Inspector中显示帮助信息
    private void OnValidate()
    {
        if (Application.isPlaying && _playerStats != null)
        {
            currentPoints = _playerStats.Points;
        }
    }
}

