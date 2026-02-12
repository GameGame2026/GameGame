using UnityEngine;

namespace _Projects.GamePlay
{
    /// <summary>
    /// tangibleObject: 实体形态（如有碰撞/可见）
    /// intangibleObject: 虚体形态（如无碰撞/半透明/不可见）
    /// </summary>
    public class PhaseShifter2 : DisposableObject
    {
        [Header("形态对象")]
        [Tooltip("贴点前形态 GameObject")]
        public GameObject originalObject;
        [Tooltip("贴点后形态 GameObject")]
        public GameObject shiftedObject;

        [Header("当前状态")]
        public bool isShifted = false;

        /// <summary>
        /// 改变形态
        /// </summary>
        public void SetShifted()
        {
            isShifted = true;
            UpdateActiveState();
        }

        /// <summary>
        /// 还原形态
        /// </summary>
        public void SetOriginal()
        {
            isShifted = false;
            UpdateActiveState();
        }

        /// <summary>
        /// 切换形态
        /// </summary>
        public void TogglePhase()
        {
            isShifted = !isShifted;
            UpdateActiveState();
        }

        /// <summary>
        /// 贴点系统接口：贴点时切换形态
        /// </summary>
        public override void ChangeState()
        {
            base.ChangeState();
            SetShifted();
        }

        /// <summary>
        /// 贴点系统接口：回收时切换回实体
        /// </summary>
        public override void Recycle()
        {
            base.Recycle();
            SetOriginal();
        }

        /// <summary>
        /// 根据 isIntangible 切换 GameObject 激活状态
        /// </summary>
        private void UpdateActiveState()
        {
            if (originalObject != null)
                originalObject.SetActive(!isShifted);
            if (shiftedObject != null)
                shiftedObject.SetActive(isShifted);
        }

        // 编辑器辅助
        [ContextMenu("切换形态（TogglePhase）")]
        private void ToggleInEditor()
        {
            TogglePhase();
        }
        [ContextMenu("设为改变后状态（SetShifted）")]
        private void SetIntangibleInEditor()
        {
            SetShifted();
        }
        [ContextMenu("设为改变前状态（SetOriginal）")]
        private void SetTangibleInEditor()
        {
            SetOriginal();
        }
    }
}