Shader "Custom/UI/ProceduralGaugeRing_V2"
{
    Properties
    {
        [Header(Geometry Settings)]
        _Radius ("Radius (0 to 0.5)", Range(0.0, 0.5)) = 0.45
        _Thickness ("Thickness", Range(0.0, 0.5)) = 0.1
        _TotalAngle ("Total Arc Angle (Degrees)", Range(0, 360)) = 180
        
        [Header(Color Zones)]
        [Toggle] _UseSolidColors ("Use Solid Blocks", Float) = 1
        // 阈值定义：0=中心, 1=边缘。
        // GreenLimit: 绿色区域的结束位置 (例如 0.3 代表中心向外 30% 是绿色)
        _GreenLimit ("Green Area Limit (0-1)", Range(0, 1)) = 0.33
        // YellowLimit: 黄色区域的结束位置 (必须大于绿色)
        _YellowLimit ("Yellow Area Limit (0-1)", Range(0, 1)) = 0.66
        
        [Header(Colors)]
        _ColorCenter ("Center Color (Green)", Color) = (0,1,0,1)
        _ColorMid ("Mid Color (Yellow)", Color) = (1,1,0,1)
        _ColorEdge ("Edge Color (Red)", Color) = (1,0,0,1)
        
        [Header(Segments)]
        _SegmentCount ("Segment Count", Float) = 10
        _GapWidth ("Gap Width", Range(0, 0.2)) = 0.02
        
        [Header(Rendering)]
        _Softness ("Edge Softness", Range(0.001, 0.05)) = 0.005
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            // 属性声明
            float _Radius;
            float _Thickness;
            float _TotalAngle;
            
            float _UseSolidColors;
            float _GreenLimit;
            float _YellowLimit;
            
            float4 _ColorCenter;
            float4 _ColorMid;
            float4 _ColorEdge;
            
            float _SegmentCount;
            float _GapWidth;
            float _Softness;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // --- 坐标计算 ---
                float2 centeredUV = i.uv - float2(0.5, 0.5);
                float dist = length(centeredUV);
                float angleRad = atan2(centeredUV.x, centeredUV.y);
                float angleDeg = degrees(angleRad);

                // --- 1. 形状遮罩 (Shape) ---
                float outerCircle = 1.0 - smoothstep(_Radius - _Softness, _Radius, dist);
                float innerCircle = smoothstep(_Radius - _Thickness, (_Radius - _Thickness) + _Softness, dist);
                float ringMask = outerCircle * innerCircle;

                // --- 2. 角度裁切 (Arc) ---
                float halfAngle = _TotalAngle / 2.0;
                float absAngle = abs(angleDeg); // 获取绝对值以实现左右对称
                float angleMask = 1.0 - smoothstep(halfAngle - 0.5, halfAngle, absAngle);

                // --- 3. 颜色计算 (核心修改) ---
                
                // t 是归一化进度：0 = 顶部中心, 1 = 边缘
                float t = absAngle / max(halfAngle, 0.001); // 防止除零
                float4 finalColor;

                // 强制阈值逻辑合理化 (Yellow不能小于Green)
                float limit1 = _GreenLimit;
                float limit2 = max(_YellowLimit, _GreenLimit + 0.01);

                if (_UseSolidColors > 0.5)
                {
                    // === 模式 A: 分块显示 (Solid Blocks) ===
                    if (t < limit1) 
                    {
                        finalColor = _ColorCenter; // 绿色区域
                    }
                    else if (t < limit2)
                    {
                        finalColor = _ColorMid;    // 黄色区域
                    }
                    else 
                    {
                        finalColor = _ColorEdge;   // 红色区域
                    }
                }
                else
                {
                    // === 模式 B: 自定义范围渐变 (Custom Gradient) ===
                    // 允许你调整渐变发生的“位置”
                    if (t < limit1)
                    {
                        // 0 ~ limit1 之间从 Center 渐变到 Mid
                        float localT = t / limit1;
                        finalColor = lerp(_ColorCenter, _ColorMid, localT);
                    }
                    else if (t < limit2)
                    {
                        // limit1 ~ limit2 之间从 Mid 渐变到 Edge
                        float localT = (t - limit1) / (limit2 - limit1);
                        finalColor = lerp(_ColorMid, _ColorEdge, localT);
                    }
                    else
                    {
                        // 超过 limit2 保持 Edge 颜色
                        finalColor = _ColorEdge;
                    }
                }

                // --- 4. 分割线 (Segments) ---
                // 计算当前处于哪个分割块
                float arcProgress = (angleDeg + halfAngle) / _TotalAngle;
                float segmentVal = arcProgress * _SegmentCount;
                
                // 简单的分割线逻辑
                float segmentPattern = abs(2.0 * frac(segmentVal) - 1.0);
                float segmentMask = smoothstep(_GapWidth, _GapWidth + 0.02, segmentPattern);

                // --- 5. 合成输出 ---
                float alpha = ringMask * angleMask * segmentMask;
                finalColor *= i.color; // 支持 UI 这里的 Color 属性调节透明度

                return float4(finalColor.rgb, alpha * finalColor.a);
            }
            ENDCG
        }
    }
}