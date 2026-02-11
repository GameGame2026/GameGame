using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace _Projects.GamePlay
{
    /// <summary>
    /// 材质闪红效果组件
    /// 用于在受伤时叠加红色 tint
    /// </summary>
    public class MaterialFlashEffect : MonoBehaviour
    {
        [Header("闪红设置")]
        [Tooltip("闪红颜色")]
        [SerializeField] private Color flashColor = new Color(1f, 0f, 0f, 0.5f);
        
        [Tooltip("闪红持续时间")]
        [SerializeField] private float flashDuration = 0.2f;
        
        [Tooltip("闪红曲线（控制颜色强度随时间的变化）")]
        [SerializeField] private AnimationCurve flashCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        
        [Tooltip("是否使用材质实例（避免影响其他使用相同材质的对象）")]
        [SerializeField] private bool useMaterialInstance = true;

        // 渲染器缓存
        private Renderer[] _renderers;
        
        // 材质缓存
        private Dictionary<Renderer, Material[]> _originalMaterials = new Dictionary<Renderer, Material[]>();
        private Dictionary<Renderer, Material[]> _instanceMaterials = new Dictionary<Renderer, Material[]>();

        // 闪红状态
        private Coroutine _flashCoroutine;

        // 常用的 Shader 属性
        private static readonly int ColorProperty = Shader.PropertyToID("_Color");
        private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionColorProperty = Shader.PropertyToID("_EmissionColor");

        private void Awake()
        {
            InitializeMaterials();
        }

        /// <summary>
        /// 初始化材质
        /// </summary>
        private void InitializeMaterials()
        {
            // 获取所有渲染器（包括子对象）
            _renderers = GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in _renderers)
            {
                if (renderer == null) continue;

                // 保存原始材质引用
                _originalMaterials[renderer] = renderer.sharedMaterials;

                if (useMaterialInstance)
                {
                    // 创建材质实例
                    Material[] instanceMats = new Material[renderer.sharedMaterials.Length];
                    for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                    {
                        if (renderer.sharedMaterials[i] != null)
                        {
                            instanceMats[i] = new Material(renderer.sharedMaterials[i]);
                        }
                    }
                    _instanceMaterials[renderer] = instanceMats;
                    renderer.materials = instanceMats;
                }
                else
                {
                    _instanceMaterials[renderer] = renderer.materials;
                }
            }
        }

        /// <summary>
        /// 触发闪红效果
        /// </summary>
        public void Flash()
        {
            Flash(flashColor, flashDuration);
        }

        /// <summary>
        /// 触发闪红效果（自定义参数）
        /// </summary>
        public void Flash(Color color, float duration)
        {
            // 停止之前的闪红效果
            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
            }

            _flashCoroutine = StartCoroutine(FlashCoroutine(color, duration));
        }

        /// <summary>
        /// 闪红协程
        /// </summary>
        private IEnumerator FlashCoroutine(Color color, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // 使用曲线计算当前强度
                float intensity = flashCurve.Evaluate(t);

                // 应用颜色到所有材质
                ApplyColorToMaterials(color * intensity);

                yield return null;
            }

            // 确保完全恢复原始颜色
            ResetMaterials();

            _flashCoroutine = null;
        }

        /// <summary>
        /// 应用颜色到材质
        /// </summary>
        private void ApplyColorToMaterials(Color tintColor)
        {
            foreach (var kvp in _instanceMaterials)
            {
                Renderer renderer = kvp.Key;
                Material[] materials = kvp.Value;

                if (renderer == null || materials == null) continue;

                foreach (Material mat in materials)
                {
                    if (mat == null) continue;

                    // 尝试不同的 Shader 属性
                    // 标准着色器和 URP Lit 使用 _BaseColor
                    if (mat.HasProperty(BaseColorProperty))
                    {
                        Color originalColor = _originalMaterials[renderer][System.Array.IndexOf(materials, mat)].GetColor(BaseColorProperty);
                        mat.SetColor(BaseColorProperty, Color.Lerp(originalColor, tintColor, tintColor.a));
                    }
                    // 旧版标准着色器使用 _Color
                    else if (mat.HasProperty(ColorProperty))
                    {
                        Color originalColor = _originalMaterials[renderer][System.Array.IndexOf(materials, mat)].GetColor(ColorProperty);
                        mat.SetColor(ColorProperty, Color.Lerp(originalColor, tintColor, tintColor.a));
                    }

                    // 也可以叠加发光效果（可选）
                    if (mat.HasProperty(EmissionColorProperty))
                    {
                        mat.SetColor(EmissionColorProperty, tintColor * tintColor.a);
                        mat.EnableKeyword("_EMISSION");
                    }
                }
            }
        }

        /// <summary>
        /// 重置材质到原始状态
        /// </summary>
        private void ResetMaterials()
        {
            foreach (var kvp in _instanceMaterials)
            {
                Renderer renderer = kvp.Key;
                Material[] materials = kvp.Value;
                Material[] originalMats = _originalMaterials[renderer];

                if (renderer == null || materials == null || originalMats == null) continue;

                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] == null || originalMats[i] == null) continue;

                    // 恢复原始颜色
                    if (materials[i].HasProperty(BaseColorProperty) && originalMats[i].HasProperty(BaseColorProperty))
                    {
                        materials[i].SetColor(BaseColorProperty, originalMats[i].GetColor(BaseColorProperty));
                    }
                    else if (materials[i].HasProperty(ColorProperty) && originalMats[i].HasProperty(ColorProperty))
                    {
                        materials[i].SetColor(ColorProperty, originalMats[i].GetColor(ColorProperty));
                    }

                    // 关闭发光
                    if (materials[i].HasProperty(EmissionColorProperty))
                    {
                        materials[i].SetColor(EmissionColorProperty, Color.black);
                        materials[i].DisableKeyword("_EMISSION");
                    }
                }
            }
        }

        /// <summary>
        /// 停止闪红效果
        /// </summary>
        public void StopFlash()
        {
            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
                _flashCoroutine = null;
            }
            
            ResetMaterials();
        }

        private void OnDestroy()
        {
            // 清理材质实例，避免内存泄漏
            if (useMaterialInstance)
            {
                foreach (var kvp in _instanceMaterials)
                {
                    if (kvp.Value != null)
                    {
                        foreach (Material mat in kvp.Value)
                        {
                            if (mat != null)
                            {
                                Destroy(mat);
                            }
                        }
                    }
                }
            }
            
            _originalMaterials.Clear();
            _instanceMaterials.Clear();
        }

        /// <summary>
        /// 设置闪红颜色
        /// </summary>
        public void SetFlashColor(Color color)
        {
            flashColor = color;
        }

        /// <summary>
        /// 设置闪红持续时间
        /// </summary>
        public void SetFlashDuration(float duration)
        {
            flashDuration = Mathf.Max(0.01f, duration);
        }
    }
}


