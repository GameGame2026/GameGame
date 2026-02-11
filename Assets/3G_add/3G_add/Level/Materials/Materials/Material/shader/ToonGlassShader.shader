Shader "Custom/SimpleToonGlass"
{
    Properties
    {
        // 基础颜色 - 设置为醒目的红色测试
        _BaseColor ("Base Color", Color) = (1, 0, 0, 0.8)
        
        // 边缘颜色
        _EdgeColor ("Edge Color", Color) = (1, 1, 1, 1)
        _EdgeWidth ("Edge Width", Range(0, 1)) = 0.2
        
        // 透明度
        _Opacity ("Opacity", Range(0, 1)) = 0.7
    }
    
    SubShader
    {
        Tags {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }
        
        // 必须启用Alpha混合
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normal : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
                UNITY_FOG_COORDS(2)
            };
            
            float4 _BaseColor;
            float4 _EdgeColor;
            float _EdgeWidth;
            float _Opacity;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                
                // 转换法线和视角方向到世界空间
                o.normal = UnityObjectToWorldNormal(v.normal);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // 计算边缘强度
                float edge = 1.0 - saturate(dot(normalize(i.normal), normalize(i.viewDir)));
                
                // 创建卡通风格的硬边缘
                float toonEdge = step(1.0 - _EdgeWidth, edge);
                
                // 混合颜色
                float4 color = _BaseColor;
                color.rgb = lerp(color.rgb, _EdgeColor.rgb, toonEdge);
                
                // 边缘处增加透明度
                color.a = _Opacity * (1.0 - toonEdge * 0.3);
                
                // 应用雾效
                UNITY_APPLY_FOG(i.fogCoord, color);
                
                return color;
            }
            ENDCG
        }
    }
}