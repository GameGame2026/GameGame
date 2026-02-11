using UnityEngine;

namespace _Projects.GamePlay
{
    /// <summary>
    /// tangibleObject: 实体形态（如有碰撞/可见）
    /// intangibleObject: 虚体形态（如无碰撞/半透明/不可见）
    /// </summary>
    public class PhaseShifter : DisposableObject
    {
        [Header("形态对象")]
        [Tooltip("实体形态 GameObject（如有碰撞/可见）")]
        public GameObject tangibleObject;
        [Tooltip("虚体形态 GameObject（如无碰撞/半透明/不可见）")]
        public GameObject intangibleObject;

        [Header("当前状态")]
        public bool isIntangible = false;

        /// <summary>
        /// 切换到虚体
        /// </summary>
        public void SetIntangible()
        {
            isIntangible = true;
            UpdateActiveState();
        }

        /// <summary>
        /// 切换到实体
        /// </summary>
        public void SetTangible()
        {
            isIntangible = false;
            UpdateActiveState();
        }

        /// <summary>
        /// 切换形态
        /// </summary>
        public void TogglePhase()
        {
            isIntangible = !isIntangible;
            UpdateActiveState();
        }

        /// <summary>
        /// 贴点系统接口：贴点时切换形态
        /// </summary>
        public override void ChangeState()
        {
            base.ChangeState();
            SetIntangible();
        }

        /// <summary>
        /// 贴点系统接口：回收时切换回实体
        /// </summary>
        public override void Recycle()
        {
            base.Recycle();
            SetTangible();
        }

        /// <summary>
        /// 根据 isIntangible 切换 GameObject 激活状态
        /// </summary>
        private void UpdateActiveState()
        {
            if (tangibleObject != null)
                tangibleObject.SetActive(!isIntangible);
            if (intangibleObject != null)
                intangibleObject.SetActive(isIntangible);
        }

        // 编辑器辅助
        [ContextMenu("切换形态（TogglePhase）")]
        private void ToggleInEditor()
        {
            TogglePhase();
        }
        [ContextMenu("设为虚体（SetIntangible）")]
        private void SetIntangibleInEditor()
        {
            SetIntangible();
        }
        [ContextMenu("设为实体（SetTangible）")]
        private void SetTangibleInEditor()
        {
            SetTangible();
        }
    }
}