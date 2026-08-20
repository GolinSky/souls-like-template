Shader "SoulsLike/GroundItemAdditive"
{
    Properties
    {
        [HDR] _Tint("Tint", Color) = (1.2, 0.65, 0.12, 1)
        _Intensity("Emission Intensity", Range(0, 10)) = 5
        _PulseSpeed("Pulse Speed", Range(0, 12)) = 6.28
        _Wobble("Wobble", Range(0, 0.1)) = 0.02
        _Radial("Radial Mask", Range(0, 1)) = 0
        _Dissolve("Dissolve", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="HDRenderPipeline" }
        Blend SrcAlpha One
        ZWrite Off
        Cull Off

        Pass
        {
            Name "ForwardOnly"
            Tags { "LightMode"="ForwardOnly" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 4.5
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float noise : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Tint;
                float _Intensity;
                float _PulseSpeed;
                float _Wobble;
                float _Radial;
                float _Dissolve;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 positionOS = input.positionOS.xyz;
                float wave = sin(_Time.y * 1.3 + positionOS.y * 11 + positionOS.x * 17);
                positionOS.xz += wave * _Wobble;
                output.positionCS = TransformObjectToHClip(positionOS);
                output.uv = input.uv;
                output.color = input.color;
                output.noise = wave * 0.5 + 0.5;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 centered = input.uv - 0.5;
                float stripMask = smoothstep(0.5, 0.18, abs(centered.y));
                float radialMask = smoothstep(0.52, 0.05, length(centered));
                float shape = lerp(stripMask, radialMask, _Radial);
                float dissolveEdge = _Dissolve + (input.noise - 0.5) * 0.12;
                clip(shape * input.color.a - dissolveEdge);
                float pulse = 0.9 + sin(_Time.y * _PulseSpeed) * 0.1;
                return half4(_Tint.rgb * _Intensity * pulse * input.color.rgb, shape * _Tint.a);
            }
            ENDHLSL
        }
    }
}
