using _Projects.GamePlay;
using UnityEngine;

namespace _Projects.GamePlay
{
    public class PhaseShifter : DisposableObject
    {
        [Header("透明度设置")]
        [Range(0, 1)] public float intangibleAlpha = 0.3f;
        [Range(0, 1)] public float tangibleAlpha = 1.0f;

        [Header("当前状态")]
        public bool isIntangible = false;

        // 定义渲染队列常量
        private const int OPAQUE_QUEUE = 2000;
        private const int TRANSPARENT_QUEUE = 3000;
        
        public override void ChangeState()
        {
            SetPhase(true);
            base.ChangeState();
            
            
        }
        
        public override void Recycle()
        {
            base.Recycle();
            
            SetPhase(false);
        }
        
        private void SetPhase(bool makeIntangible)
        {
            Debug.Log("Changing phase state to " + (makeIntangible ? "Intangible" : "Tangible"));
            isIntangible = makeIntangible;

            // 获取渲染器
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = renderer.material;

                // 核心修复：完整设置透明状态
                if (makeIntangible)
                {
                    // 1. 设置表面类型为透明
                    mat.SetFloat("_Surface", 1); // 1 = Transparent

                    // 2. 设置混合模式（重要！）- 这是显示半透明的关键
                    mat.SetFloat("_Blend", 0); // 0 = Alpha Blend
                    // 设置源和目标混合因子
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

                    // 3. 关闭深度写入，允许物体被后面的物体穿透
                    mat.SetInt("_ZWrite", 0);

                    // 4. 设置正确的渲染队列
                    mat.renderQueue = TRANSPARENT_QUEUE;

                    // 5. 启用Alpha裁剪（可选，根据需求）
                    mat.SetFloat("_AlphaClip", 0); // 0 = Disabled
                }
                else
                {
                    // 恢复为不透明状态
                    mat.SetFloat("_Surface", 0); // 0 = Opaque
                    mat.SetInt("_ZWrite", 1);
                    mat.renderQueue = OPAQUE_QUEUE;

                    // 重置混合模式（不透明物体不需要）
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                }

                // 6. 设置Alpha值
                Color color = mat.color;
                color.a = isIntangible ? intangibleAlpha : tangibleAlpha;
                mat.color = color;

                // 7. 刷新材质（重要！）
                // 强制Unity更新材质的内部状态
                mat.shaderKeywords = null;
            }

            BoxCollider collider = GetComponent<BoxCollider>();
            if (collider != null)
            {
                collider.isTrigger = isIntangible;
            }
        }
        
        public void TogglePhase()
        {
            SetPhase(!isIntangible);
        }
        
        public void SetIntangible()
        {
            SetPhase(true);
        }
        
        public void SetTangible()
        {
            SetPhase(false);
        }
        
        [ContextMenu("切换相位状态")]
        private void ToggleInEditor()
        {
            TogglePhase();
        }
    }
}