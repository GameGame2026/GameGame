using UnityEngine;
using _Projects.GamePlay;

public class Billboard : MonoBehaviour
{
    public Camera targetCamera;
    public bool lockYAxis = false; // 可选：锁定Y轴旋转
    public bool lockXAxis = false; // 可选：锁定X轴旋转

    [Header("Flip 模式（像 Player 的翻转）")]
    [Tooltip("启用后使用 localScale.x 翻转而不是改变旋转")]
    public bool useFlip = true;

    [Tooltip("指定敌人组件，若为空会在父对象中自动查找")]
    public EnemyBase enemyBase;

    private Vector3 _originalLocalScale;

    void Awake()
    {
        _originalLocalScale = transform.localScale;
        if (enemyBase == null)
            enemyBase = GetComponentInParent<EnemyBase>();
    }

    void Update()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null) return;

        Vector3 direction = targetCamera.transform.position - transform.position;

        if (lockYAxis)
            direction.y = 0; // 保持Y轴与世界一致（如地面上的标志）
        if (lockXAxis)
            direction.x = 0;

        // 先保持面向相机（保持 billboard 效果）
        transform.rotation = Quaternion.LookRotation(-direction);

        if (useFlip)
        {
            // 如果有敌人组件并且正在攻击，则跳过翻转逻辑（保持当前 localScale）
            if (enemyBase != null && enemyBase.IsAttacking)
            {
                // 不改变 localScale，保持攻击动画中的朝向
                return;
            }

            // 通过相机的右向量判断相机是在物体的左侧还是右侧，从而决定翻转方向
            // 使用 direction（物体->相机）与相机的 right 做点积
            float side = Vector3.Dot(targetCamera.transform.right, direction);
            float sign = side >= 0f ? 1f : -1f;

            Vector3 s = _originalLocalScale;
            s.x = Mathf.Abs(_originalLocalScale.x) * sign;
            transform.localScale = s;
        }
        else
        {
            // 非 Flip 模式，保持原始缩放（避免被上一次翻转影响）
            transform.localScale = _originalLocalScale;
        }
    }
}