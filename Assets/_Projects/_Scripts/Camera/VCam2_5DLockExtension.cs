using UnityEngine;
using Cinemachine;

namespace _Projects.Camera
{
    /// <summary>
    /// 在 3D 场景中模拟 2D 侧视相机：
    /// \- 锁定世界 Z 平面
    /// \- 可选跟随目标 Z 调整相机深度（带阻尼）
    /// 将本组件挂到需要的 CinemachineVirtualCamera 上
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("Cinemachine/Extensions/2.5D Z Lock")]
    public class VCam2_5DLockExtension : CinemachineExtension
    {
        [Header("Z 锁定/跟随")]
        [Tooltip("是否根据 Follow 的世界 Z 调整相机深度")]
        public bool enableDepthFollowZ = false;

        [Tooltip("当未跟随 Z 时，相机所处的世界 Z 值")]
        public float fixedWorldZ = -10f;

        [Tooltip("当跟随 Z 时，相机 Z = Follow.Z + 本偏移")]
        public float depthOffset = 0f;

        [Tooltip("Z 方向的平滑时间（秒），0 为无平滑")]
        [Min(0f)]
        public float depthDamping = 0.2f;

        private float _currentZ;
        private float _zVel;

        protected override void OnEnable()
        {
            base.OnEnable();
            _zVel = 0f;
            // 尝试用当前相机位置初始化，避免切换抖动
            var vcam = VirtualCamera;
            if (vcam != null)
            {
                var state = vcam.State;
                _currentZ = state.RawPosition.z;
            }
            else
            {
                _currentZ = fixedWorldZ;
            }
        }

        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage,
            ref CameraState state,
            float deltaTime)
        {
            if (stage != CinemachineCore.Stage.Body)
                return;

            // 计算目标 Z
            float desiredZ = fixedWorldZ;
            if (enableDepthFollowZ && vcam.Follow != null)
                desiredZ = vcam.Follow.position.z + depthOffset;

            // 处理首次帧或暂停编辑器时可能给出的负 deltaTime
            bool canDamp = depthDamping > 0f && deltaTime >= 0f;

            if (canDamp)
                _currentZ = Mathf.SmoothDamp(_currentZ, desiredZ, ref _zVel, depthDamping, Mathf.Infinity, deltaTime);
            else
                _currentZ = desiredZ;

            // 只改世界 Z，保持 X/Y 与 Cinemachine 计算一致
            var pos = state.RawPosition;
            pos.z = _currentZ;
            state.RawPosition = pos;
        }
    }
}