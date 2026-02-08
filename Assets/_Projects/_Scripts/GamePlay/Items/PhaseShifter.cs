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
            base.ChangeState();
            
            if (IsAttached)
            {
                SetPhase(true);
            }
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
                
                if (makeIntangible)
                {
                    mat.SetFloat("_Surface", 1); 
                    
                    mat.SetFloat("_Blend", 0);
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    
                    mat.SetInt("_ZWrite", 0);
                    
                    mat.renderQueue = TRANSPARENT_QUEUE;
                    
                    mat.SetFloat("_AlphaClip", 0); 
                }
                else
                {
                    mat.SetFloat("_Surface", 0);
                    mat.SetInt("_ZWrite", 1);
                    mat.renderQueue = OPAQUE_QUEUE;
                    
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                }
                
                Color color = mat.color;
                color.a = isIntangible ? intangibleAlpha : tangibleAlpha;
                mat.color = color;
                
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