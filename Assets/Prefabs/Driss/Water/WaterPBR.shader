Shader "Custom/WaterPBR"
{
    Properties
    {
        [Header(Surface)]
        _BaseColor ("Base Color", Color) = (0.4, 0.8, 0.9, 0.2)
        _ShallowColor ("Shallow Color", Color) = (0.4, 0.9, 1.0, 0.3)
        _DeepColor ("Deep Color", Color) = (0.1, 0.4, 0.7, 0.8)
        _Smoothness ("Smoothness", Range(0, 1)) = 0.95
        _Metallic ("Metallic", Range(0, 1)) = 0.0

        [Header(Normal Maps)]
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalScale ("Normal Scale", Range(0, 2)) = 1.0
        _WaveSpeed ("Wave Speed", Vector) = (0.1, 0.1, -0.1, 0.1)

        [Header(Depth and Refraction)]
        _DepthDistance ("Depth Distance", Range(0.1, 10)) = 2.0
        _RefractionStrength ("Refraction Strength", Range(0, 1)) = 0.5

        [Header(Foam)]
        _FoamColor ("Foam Color", Color) = (1, 1, 1, 1)
        _FoamDistance ("Foam Distance", Range(0, 2)) = 0.5
        _FoamCutoff ("Foam Cutoff", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline" 
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            
            // URP Keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GlobalIllumination.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD3;
                float4 screenPos : TEXCOORD4;
                float2 uv : TEXCOORD5;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _ShallowColor;
                float4 _DeepColor;
                float _Smoothness;
                float _Metallic;
                float4 _NormalMap_ST; // Tiling/Offset
                float _NormalScale;
                float4 _WaveSpeed;
                float _DepthDistance;
                float _RefractionStrength;
                float4 _FoamColor;
                float _FoamDistance;
                float _FoamCutoff;
            CBUFFER_END

            TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);

            // --- NOISE FUNCTIONS ---
            float hash(float3 p) {
                p = frac(p * float3(443.897, 441.423, 437.195));
                p += dot(p, p.yxz + 19.19);
                return frac((p.x + p.y) * p.z);
            }
            float noise(float3 p) {
                float3 i = floor(p); float3 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                
                float n000 = hash(i); float n100 = hash(i + float3(1, 0, 0));
                float n010 = hash(i + float3(0, 1, 0)); float n110 = hash(i + float3(1, 1, 0));
                float n001 = hash(i + float3(0, 0, 1)); float n101 = hash(i + float3(1, 0, 1));
                float n011 = hash(i + float3(0, 1, 1)); float n111 = hash(i + float3(1, 1, 1));
                
                float n00 = lerp(n000, n100, f.x); float n01 = lerp(n001, n101, f.x);
                float n10 = lerp(n010, n110, f.x); float n11 = lerp(n011, n111, f.x);
                return lerp(lerp(n00, n10, f.y), lerp(n01, n11, f.y), f.z);
            }
            float fbm(float3 p) {
                float v = 0.0; float a = 0.5;
                for (int i = 0; i < 3; i++) { v += a * noise(p); p *= 2.0; a *= 0.5; }
                return v;
            }
            // -----------------------

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(output.positionWS);
                output.screenPos = ComputeScreenPos(output.positionCS);
                output.uv = input.uv;
                return output;
            }

            // Blended Normal Mapping
            float3 SampleNormals(float2 uv, float3 positionWS)
            {
                // Animation logic
                float2 uv1 = uv + _Time.y * _WaveSpeed.xy;
                float2 uv2 = uv + _Time.y * _WaveSpeed.zw;

                // Sample normal map twice
                float3 n1 = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uv1), _NormalScale);
                float3 n2 = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uv2), _NormalScale);

                // Blend
                return normalize(float3(n1.xy + n2.xy, n1.z * n2.z)); // Whiteout blend approx
            }

            half4 Frag(Varyings input) : SV_Target
            {
                // 1. Data Prep
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                float3 viewDirWS = normalize(input.viewDirWS);
                
                // 2. Normals
                // Sample 2 normal maps moving in opposite directions
                float3 normalTangent = SampleNormals(input.uv * _NormalMap_ST.xy, input.positionWS);
                
                // Calculate TBN for perturbations
                float3 baseNormal = normalize(input.normalWS);
                float3 tangent = normalize(cross(baseNormal, float3(0,0,1))); // Arbitrary tangent for plane
                float3 bitangent = cross(baseNormal, tangent);
                float3x3 TBN = float3x3(tangent, bitangent, baseNormal);
                
                float3 normalWS = normalize(mul(normalTangent, TBN));

                // 3. Depth & Transparency
                float rawDepth = SampleSceneDepth(screenUV);
                float sceneDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float surfaceDepth = LinearEyeDepth(input.screenPos.z / input.screenPos.w, _ZBufferParams);
                float waterDepth = sceneDepth - surfaceDepth;
                
                // If we are rendering "above" the water line due to distortion, clamp it
                if (waterDepth < 0) waterDepth = 0;

                // 4. Refraction (Background Distortion)
                // Distort UVs based on normal and depth (less distortion at edges)
                float2 refractUV = screenUV + (normalWS.xz * _RefractionStrength * saturate(waterDepth * 0.5));
                float3 background = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, refractUV).rgb;

                // 5. Water Color (Absorption)
                // Deeper = Darker/Blue
                float absorption = saturate(waterDepth / _DepthDistance);
                float4 albedo = lerp(_ShallowColor, _DeepColor, absorption);

                // 6. Fresnel Effect (Reflection vs Refraction strength)
                // F0 for water is usually around 0.02, but we use _Smoothness to control the look slightly or fix it
                float3 F0 = float3(0.02, 0.02, 0.02); 
                float NdotV = saturate(dot(normalWS, viewDirWS));
                float fresnel = pow(1.0 - NdotV, 5.0); // Simple Schlick approximation power
                fresnel = clamp(fresnel + 0.02, 0.0, 1.0); // Ensure minimal reflection

                // 7. Environment Reflection
                // Reflect view dir around normal
                float3 reflectDir = reflect(-viewDirWS, normalWS);
                
                // Sample Unity Reflection Probe mechanics
                float3 reflection = GlossyEnvironmentReflection(reflectDir, 1.0 - _Smoothness, 1.0);
                
                // 8. Main Light Specular
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                float3 halfDir = normalize(mainLight.direction + viewDirWS);
                float NdotH = saturate(dot(normalWS, halfDir));
                float specular = pow(NdotH, _Smoothness * 256.0) * _Smoothness;
                
                // 9. Combine
                // Mix background (Refraction) and WaterColor based on alpha/depth
                float3 refraction = lerp(background, albedo.rgb, albedo.a * absorption + 0.1);
                
                // Mix Refraction and Reflection based on Fresnel
                // Ideally: Color = lerp(Refraction, Reflection, Fresnel) + Specular
                float3 finalColor = lerp(refraction, reflection, fresnel * _RefractionStrength * 2.0); // Boost reflection a bit
                finalColor += specular * mainLight.color;
                
                // 10. Foam
                // Foam at intersection edges
                float foamFactor = 1.0 - saturate(waterDepth / _FoamDistance);
                float foamNoise = fbm(input.positionWS * 2.0); // Optional noise for foam
                if (foamFactor > _FoamCutoff)
                {
                   finalColor = lerp(finalColor, _FoamColor.rgb, _FoamColor.a);
                }

                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}
