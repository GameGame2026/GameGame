using UnityEngine;
using Cinemachine;

namespace _Projects.Camera
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("Cinemachine/Extensions/Axis Lock")]
    public class VCam2_5DLockExtension : CinemachineExtension
    {
        [Header("X 轴锁定/跟随")]
        [Tooltip("是否锁定 X 轴")]
        public bool lockX = false;

        [Tooltip("是否根据 Follow 的世界 X 调整相机位置")]
        public bool enableFollowX = false;

        [Tooltip("当锁定 X 且未跟随时，相机所处的世界 X 值")]
        public float fixedWorldX = 0f;

        [Tooltip("当跟随 X 时，相机 X = Follow.X + 本偏移")]
        public float offsetX = 0f;

        [Tooltip("X 方向的平滑时间（秒），0 为无平滑")]
        [Min(0f)]
        public float dampingX = 0.2f;

        [Header("Y 轴锁定/跟随")]
        [Tooltip("是否锁定 Y 轴")]
        public bool lockY = false;

        [Tooltip("是否根据 Follow 的世界 Y 调整相机位置")]
        public bool enableFollowY = false;

        [Tooltip("当锁定 Y 且未跟随时，相机所处的世界 Y 值")]
        public float fixedWorldY = 0f;

        [Tooltip("当跟随 Y 时，相机 Y = Follow.Y + 本偏移")]
        public float offsetY = 0f;

        [Tooltip("Y 方向的平滑时间（秒），0 为无平滑")]
        [Min(0f)]
        public float dampingY = 0.2f;

        [Header("Z 轴锁定/跟随")]
        [Tooltip("是否锁定 Z 轴")]
        public bool lockZ = false;

        [Tooltip("是否根据 Follow 的世界 Z 调整相机深度")]
        public bool enableFollowZ = false;

        [Tooltip("当锁定 Z 且未跟随时，相机所处的世界 Z 值")]
        public float fixedWorldZ = -10f;

        [Tooltip("当跟随 Z 时，相机 Z = Follow.Z + 本偏移")]
        public float offsetZ = 0f;

        [Tooltip("Z 方向的平滑时间（秒），0 为无平滑")]
        [Min(0f)]
        public float dampingZ = 0.2f;

        private float _currentX;
        private float _currentY;
        private float _currentZ;
        private float _xVel;
        private float _yVel;
        private float _zVel;

        protected override void OnEnable()
        {
            base.OnEnable();
            _xVel = 0f;
            _yVel = 0f;
            _zVel = 0f;
            
            // 尝试用当前相机位置初始化，避免切换抖动
            var vcam = VirtualCamera;
            if (vcam != null)
            {
                var state = vcam.State;
                _currentX = state.RawPosition.x;
                _currentY = state.RawPosition.y;
                _currentZ = state.RawPosition.z;
            }
            else
            {
                _currentX = fixedWorldX;
                _currentY = fixedWorldY;
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

            var pos = state.RawPosition;
            
            bool validDeltaTime = deltaTime >= 0f;

            // 处理 X 轴
            if (lockX)
            {
                float desiredX = fixedWorldX;
                if (enableFollowX && vcam.Follow != null)
                    desiredX = vcam.Follow.position.x + offsetX;

                bool canDampX = dampingX > 0f && validDeltaTime;
                if (canDampX)
                    _currentX = Mathf.SmoothDamp(_currentX, desiredX, ref _xVel, dampingX, Mathf.Infinity, deltaTime);
                else
                    _currentX = desiredX;

                pos.x = _currentX;
            }

            // 处理 Y 轴
            if (lockY)
            {
                float desiredY = fixedWorldY;
                if (enableFollowY && vcam.Follow != null)
                    desiredY = vcam.Follow.position.y + offsetY;

                bool canDampY = dampingY > 0f && validDeltaTime;
                if (canDampY)
                    _currentY = Mathf.SmoothDamp(_currentY, desiredY, ref _yVel, dampingY, Mathf.Infinity, deltaTime);
                else
                    _currentY = desiredY;

                pos.y = _currentY;
            }

            // 处理 Z 轴
            if (lockZ)
            {
                float desiredZ = fixedWorldZ;
                if (enableFollowZ && vcam.Follow != null)
                    desiredZ = vcam.Follow.position.z + offsetZ;

                bool canDampZ = dampingZ > 0f && validDeltaTime;
                if (canDampZ)
                    _currentZ = Mathf.SmoothDamp(_currentZ, desiredZ, ref _zVel, dampingZ, Mathf.Infinity, deltaTime);
                else
                    _currentZ = desiredZ;

                pos.z = _currentZ;
            }

            state.RawPosition = pos;
        }
    }
}