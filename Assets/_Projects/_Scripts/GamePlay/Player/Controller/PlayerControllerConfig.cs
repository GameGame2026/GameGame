using UnityEngine;

namespace GamePlay.Controller
{
    /// <summary>
    /// 玩家控制器配置 - ScriptableObject 数据配置文件
    /// 用于存储角色移动、跳跃、相机等所有可配置参数
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerControllerConfig", menuName = "GamePlay/Player Controller Config", order = 1)]
    public class PlayerControllerConfig : ScriptableObject
    {
        [Header("移动设置")]
        [Tooltip("移动速度")]
        public float moveSpeed = 2.0f;

        [Tooltip("冲刺速度")]
        public float sprintSpeed = 5.335f;

        [Tooltip("转向移动方向的速度")]
        [Range(0.0f, 0.3f)]
        public float rotationSmoothTime = 0.12f;

        [Tooltip("加速和减速")]
        public float speedChangeRate = 10.0f;

        [Header("跳跃设置")]
        [Tooltip("跳跃的高度")]
        public float jumpHeight = 1.2f;

        [Tooltip("使用自定义重力值. 默认值为 -9.81f")]
        public float gravity = -15.0f;

        [Tooltip("在能够再次跳跃之前需要经过的时间. 设置为 0f 可立即再次跳跃")]
        public float jumpTimeout = 0.50f;

        [Tooltip("进入下落状态之前需要经过的时间. 对于走下楼梯很有用")]
        public float fallTimeout = 0.15f;

        [Header("地面检测")]
        [Tooltip("对粗糙地面很有用")]
        public float groundedOffset = -0.14f;

        [Tooltip("接地检测的半径. 应与 CharacterController 的半径匹配")]
        public float groundedRadius = 0.28f;

        [Tooltip("角色作为地面使用的层")]
        public LayerMask groundLayers;

        [Header("相机设置")]
        [Tooltip("摄像机向上移动的最大角度")]
        public float topClamp = 70.0f;

        [Tooltip("摄像机向下移动的最大角度")]
        public float bottomClamp = -30.0f;

        [Tooltip("覆盖摄像机的额外角度. 当锁定时用于微调摄像机位置")]
        public float cameraAngleOverride = 0.0f;

        [Tooltip("锁定摄像机在所有轴上的位置")]
        public bool lockCameraPosition = false;

        [Header("音效设置")]
        public AudioClip landingAudioClip;
        public AudioClip[] footstepAudioClips;
        
        [Range(0, 1)] 
        public float footstepAudioVolume = 0.5f;

        [Header("交互设置")]
        [Tooltip("交互检测距离")]
        public float interactRange = 3.0f;
        
        [Tooltip("交互检测角度（前方锥形范围）")]
        public float interactAngle = 60.0f;
        
        [Tooltip("处置检测距离")]
        public float disposeRange = 3.0f;
        
        [Tooltip("处置检测角度")]
        public float disposeAngle = 60.0f;

        [Header("攻击设置")]
        [Tooltip("攻击冷却时间（秒）")]
        public float attackCooldown = 0.5f;
    }
}
