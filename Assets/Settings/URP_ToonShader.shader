Shader "Custom/URP/ToonShader"
{
    Properties
    {
        [Header(Base)]
        _MainTex ("Base Texture (RGB)", 2D) = "white" {}
        _Color ("Base Color", Color) = (1,1,1,1)

        [Header(Toon Shading)]
        _RampSmooth ("Shadow Edge Softness", Range(0.001, 1.0)) = 0.05
        _ShadowThreshold ("Shadow Threshold", Range(-1.0, 1.0)) = 0.0
        _ShadowColor ("Shadow Color (Multiply)", Color) = (0.6,0.65,0.8,1)
        _MidThreshold ("Mid-Tone Threshold", Range(-1.0, 1.0)) = 0.5
        _MidColor ("Mid-Tone Color (Multiply)", Color) = (0.85,0.85,0.9,1)

        [Header(Specular)]
        [Toggle(_SPECULAR_ON)] _UseSpecular ("Enable Specular", Float) = 1
        _SpecColor ("Specular Color", Color) = (1,1,1,1)
        _SpecSize ("Specular Size", Range(0.001, 1.0)) = 0.08
        _SpecSmooth ("Specular Edge Softness", Range(0.001, 1.0)) = 0.02
        _Glossiness ("Glossiness (Power)", Range(1, 256)) = 32

        [Header(Rim Light)]
        [Toggle(_RIM_ON)] _UseRim ("Enable Rim Light", Float) = 1
        _RimColor ("Rim Color", Color) = (1,1,1,1)
        _RimAmount ("Rim Amount", Range(0.0, 1.0)) = 0.7
        _RimSmooth ("Rim Edge Softness", Range(0.001, 1.0)) = 0.1
        _RimIntensity ("Rim Intensity", Range(0.0, 5.0)) = 1.0

        [Header(Outline)]
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0.0, 0.1)) = 0.005

        [Header(Extras)]
        _EmissionMap ("Emission Map", 2D) = "black" {}
        [HDR]_EmissionColor ("Emission Color", Color) = (0,0,0,1)
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(0,2)) = 1

        [Header(Rendering)]
        [Enum(UnityEngine.Rendering.CullMode)] _OutlineCull ("Outline Cull Mode (leave as Front)", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        LOD 200

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _Color;
            float4 _NormalMap_ST;
            float _NormalStrength;
            float4 _EmissionColor;

            float _RampSmooth;
            float _ShadowThreshold;
            float4 _ShadowColor;
            float _MidThreshold;
            float4 _MidColor;

            float4 _SpecColor;
            float _SpecSize;
            float _SpecSmooth;
            float _Glossiness;

            float4 _RimColor;
            float _RimAmount;
            float _RimSmooth;
            float _RimIntensity;

            float4 _OutlineColor;
            float _OutlineWidth;
        CBUFFER_END

        TEXTURE2D(_MainTex);       SAMPLER(sampler_MainTex);
        TEXTURE2D(_NormalMap);     SAMPLER(sampler_NormalMap);
        TEXTURE2D(_EmissionMap);   SAMPLER(sampler_EmissionMap);
        ENDHLSL

        // ----------------------------------------------------------------
        // OUTLINE PASS
        // ----------------------------------------------------------------
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Front
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                positionWS += normalWS * _OutlineWidth;
                OUT.positionHCS = TransformWorldToHClip(positionWS);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }

        // ----------------------------------------------------------------
        // MAIN FORWARD LIT PASS (toon lighting)
        // ----------------------------------------------------------------
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature_local _SPECULAR_ON
            #pragma shader_feature_local _RIM_ON

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 positionWS  : TEXCOORD1;
                float3 normalWS    : TEXCOORD2;
                float4 tangentWS   : TEXCOORD3; // w = sign
                float3 viewDirWS   : TEXCOORD4;
                float4 shadowCoord : TEXCOORD5;
                float  fogCoord    : TEXCOORD6;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normInputs = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);

                OUT.positionHCS = posInputs.positionCS;
                OUT.positionWS = posInputs.positionWS;
                OUT.normalWS = normInputs.normalWS;
                OUT.tangentWS = float4(normInputs.tangentWS, IN.tangentOS.w * GetOddNegativeScale());
                OUT.viewDirWS = GetWorldSpaceViewDir(posInputs.positionWS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.shadowCoord = GetShadowCoord(posInputs);
                OUT.fogCoord = ComputeFogFactor(posInputs.positionCS.z);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float3 normalWS = normalize(IN.normalWS);
                float3 tangentWS = normalize(IN.tangentWS.xyz);
                float3 bitangentWS = cross(normalWS, tangentWS) * IN.tangentWS.w;
                float3x3 TBN = float3x3(tangentWS, bitangentWS, normalWS);

                half3 normalTex = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, IN.uv), _NormalStrength);
                float3 finalNormalWS = normalize(mul(normalTex, TBN));

                float3 viewDirWS = normalize(IN.viewDirWS);

                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                half4 albedo = tex * _Color;

                // Main light (with shadow attenuation)
                Light mainLight = GetMainLight(IN.shadowCoord);
                float3 lightDirWS = normalize(mainLight.direction);

                float NdotL = dot(finalNormalWS, lightDirWS);
                float lightIntensity = NdotL * mainLight.shadowAttenuation * mainLight.distanceAttenuation;

                float shadowBand = smoothstep(_ShadowThreshold - _RampSmooth, _ShadowThreshold + _RampSmooth, lightIntensity);
                float midBand = smoothstep(_MidThreshold - _RampSmooth, _MidThreshold + _RampSmooth, lightIntensity);

                half3 toneColor = lerp(_ShadowColor.rgb, _MidColor.rgb, shadowBand);
                toneColor = lerp(toneColor, half3(1,1,1), midBand);

                half3 litColor = albedo.rgb * toneColor * mainLight.color;

                // Ambient / SH
                litColor += albedo.rgb * SampleSH(finalNormalWS) * 0.5;

                // Toon specular
                #if defined(_SPECULAR_ON)
                float3 halfDir = normalize(lightDirWS + viewDirWS);
                float NdotH = saturate(dot(finalNormalWS, halfDir));
                float specVal = pow(NdotH, _Glossiness);
                float specToon = smoothstep(1.0 - _SpecSize - _SpecSmooth, 1.0 - _SpecSize + _SpecSmooth, specVal);
                litColor += _SpecColor.rgb * specToon * mainLight.shadowAttenuation * step(0.001, lightIntensity);
                #endif

                // Rim light
                #if defined(_RIM_ON)
                float NdotV = saturate(dot(finalNormalWS, viewDirWS));
                float rim = 1.0 - NdotV;
                float rimToon = smoothstep(_RimAmount - _RimSmooth, _RimAmount + _RimSmooth, rim);
                litColor += _RimColor.rgb * rimToon * _RimIntensity * saturate(lightIntensity + 0.5);
                #endif

                // Additional lights (point/spot), toon-stepped per light
                #ifdef _ADDITIONAL_LIGHTS
                uint additionalLightsCount = GetAdditionalLightsCount();
                for (uint lightIndex = 0u; lightIndex < additionalLightsCount; lightIndex++)
                {
                    Light addLight = GetAdditionalLight(lightIndex, IN.positionWS);
                    float addNdotL = dot(finalNormalWS, addLight.direction);
                    float addAtten = addLight.shadowAttenuation * addLight.distanceAttenuation;
                    float addBand = smoothstep(_ShadowThreshold - _RampSmooth, _ShadowThreshold + _RampSmooth, addNdotL * addAtten);
                    litColor += albedo.rgb * addLight.color * addBand * addAtten;
                }
                #endif

                // Emission
                half3 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, IN.uv).rgb * _EmissionColor.rgb;
                litColor += emission;

                half4 finalColor = half4(litColor, albedo.a);
                finalColor.rgb = MixFog(finalColor.rgb, IN.fogCoord);
                return finalColor;
            }
            ENDHLSL
        }

        // ----------------------------------------------------------------
        // SHADOW CASTER PASS
        // ----------------------------------------------------------------
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            float4 GetShadowPositionHClip(Attributes IN)
            {
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);

                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                float3 lightDirectionWS = _LightDirection;
                #endif

                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

                #if UNITY_REVERSED_Z
                positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #else
                positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #endif

                return positionCS;
            }

            Varyings ShadowPassVertex (Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = GetShadowPositionHClip(IN);
                return OUT;
            }

            half4 ShadowPassFragment (Varyings IN) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        // ----------------------------------------------------------------
        // DEPTH ONLY PASS (needed for depth texture / post effects)
        // ----------------------------------------------------------------
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
    CustomEditor "ToonShaderGUI"
}
