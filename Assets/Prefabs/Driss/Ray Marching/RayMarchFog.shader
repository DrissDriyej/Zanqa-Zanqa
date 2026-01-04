Shader "Custom/RayMarchFogURP"
{
    Properties
    {
        _FogColor ("Fog Color", Color) = (0.78, 0.7, 0.58, 1.0)
        _Density ("Density", Range(0, 0.2)) = 0.03
        _StepSize ("Step Size", Range(0.5, 5)) = 2.0
        _MaxDistance ("Max Distance", Range(10, 200)) = 60
        _NoiseScale ("Noise Scale", Range(0.01, 0.5)) = 0.08
        _FogHeight ("Fog Height", Range(-50, 100)) = 20
        _FogFalloff ("Fog Falloff", Range(0.01, 0.5)) = 0.05
        
        [Header(Labyrinth Zone No Fog)]
        _ZoneMinX ("Zone Min X", Float) = -55
        _ZoneMaxX ("Zone Max X", Float) = 25
        _ZoneMinZ ("Zone Min Z", Float) = -130
        _ZoneMaxZ ("Zone Max Z", Float) = -5
        _ZoneFadeDistance ("Fade Distance", Range(1, 50)) = 10
        
        [Header(Debug)]
        [Toggle] _DebugMode ("Debug Mode (show mask)", Float) = 0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "RayMarchFog"
            
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _FogColor;
                float _Density;
                float _StepSize;
                float _MaxDistance;
                float _NoiseScale;
                float _FogHeight;
                float _FogFalloff;
                
                float _ZoneMinX;
                float _ZoneMaxX;
                float _ZoneMinZ;
                float _ZoneMaxZ;
                float _ZoneFadeDistance;
                
                float _DebugMode;
            CBUFFER_END

            // Simple hash
            float hash(float3 p)
            {
                p = frac(p * float3(443.897, 441.423, 437.195));
                p += dot(p, p.yxz + 19.19);
                return frac((p.x + p.y) * p.z);
            }

            // 3D noise
            float noise(float3 p)
            {
                float3 i = floor(p);
                float3 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                
                float n000 = hash(i);
                float n100 = hash(i + float3(1, 0, 0));
                float n010 = hash(i + float3(0, 1, 0));
                float n110 = hash(i + float3(1, 1, 0));
                float n001 = hash(i + float3(0, 0, 1));
                float n101 = hash(i + float3(1, 0, 1));
                float n011 = hash(i + float3(0, 1, 1));
                float n111 = hash(i + float3(1, 1, 1));
                
                float n00 = lerp(n000, n100, f.x);
                float n01 = lerp(n001, n101, f.x);
                float n10 = lerp(n010, n110, f.x);
                float n11 = lerp(n011, n111, f.x);
                
                return lerp(lerp(n00, n10, f.y), lerp(n01, n11, f.y), f.z);
            }

            // FBM
            float fbm(float3 p)
            {
                float v = 0.0;
                float a = 0.5;
                for (int i = 0; i < 3; i++)
                {
                    v += a * noise(p);
                    p *= 2.0;
                    a *= 0.5;
                }
                return v;
            }
            
            // Check if position is inside labyrinth zone
            // Returns 0 = inside labyrinth (no fog), 1 = outside (full fog)
            // Check if position is inside labyrinth zone
            // Returns 0 = inside labyrinth (no fog), 1 = outside (full fog)
            float getLabyrinthMask(float3 pos)
            {
                // Check bounds with fade padding
                float minX = _ZoneMinX + _ZoneFadeDistance;
                float maxX = _ZoneMaxX - _ZoneFadeDistance;
                float minZ = _ZoneMinZ + _ZoneFadeDistance;
                float maxZ = _ZoneMaxZ - _ZoneFadeDistance;

                // Test if fully inside the inner box (secure 0 fog area)
                if (pos.x > minX && pos.x < maxX && pos.z > minZ && pos.z < maxZ)
                {
                    return 0.0;
                }

                // If not deeply inside, check if we are in the fade margin
                float distToEdgeX = min(abs(pos.x - _ZoneMinX), abs(pos.x - _ZoneMaxX));
                float distToEdgeZ = min(abs(pos.z - _ZoneMinZ), abs(pos.z - _ZoneMaxZ));
                
                // Are we inside the main bounds at all?
                bool insideX = (pos.x >= _ZoneMinX) && (pos.x <= _ZoneMaxX);
                bool insideZ = (pos.z >= _ZoneMinZ) && (pos.z <= _ZoneMaxZ);

                if (insideX && insideZ)
                {
                    // Inside the transition zone
                    float distToEdge = min(distToEdgeX, distToEdgeZ);
                    return saturate(1.0 - distToEdge / _ZoneFadeDistance);
                }

                // Outside bounds -> Full fog
                return 1.0;
            }

            struct appdata
            {
                uint vertexID : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                o.positionCS = GetFullScreenTriangleVertexPosition(v.vertexID);
                o.uv = GetFullScreenTriangleTexCoord(v.vertexID);
                
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                
                float2 uv = i.uv;
                
                // Sample scene
                float4 sceneColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);
                
                // Reconstruct ray
                float2 ndc = uv * 2.0 - 1.0;
                float3 rayOrigin = _WorldSpaceCameraPos;
                
                // Simple ray direction from NDC
                float4 clipPos = float4(ndc, 1.0, 1.0);
                float4 viewPos = mul(UNITY_MATRIX_I_P, clipPos);
                viewPos.xyz /= viewPos.w;
                float3 rayDir = normalize(mul((float3x3)UNITY_MATRIX_I_V, viewPos.xyz));
                
                // DEBUG MODE: Show coordinates
                if (_DebugMode > 0.5)
                {
                    float3 testPos = rayOrigin + rayDir * 10.0; // Test at 10 units from camera
                    float mask = getLabyrinthMask(testPos);
                    
                    // Show X coordinate as Red intensity (divide by 100 to normalize)
                    // Show Z coordinate as Blue intensity
                    float xNorm = (testPos.x + 50.0) / 150.0; // -50 to 100 -> 0 to 1
                    float zNorm = (testPos.z) / 150.0;        // 0 to 150 -> 0 to 1
                    
                    // If inside zone, show cyan. If outside, show the X/Z visualization
                    if (mask < 0.5)
                    {
                        // Inside labyrinth - CYAN
                        return float4(0, 1, 1, 1);
                    }
                    else
                    {
                        // Outside - show X as Red, Z as Blue
                        return float4(saturate(xNorm), 0, saturate(zNorm), 1);
                    }
                }
                
                // Ray march
                float t = 1.0;
                float transmittance = 1.0;
                float3 fogAccum = float3(0, 0, 0);
                
                [loop]
                for (int step = 0; step < 24; step++)
                {
                    if (t > _MaxDistance || transmittance < 0.02)
                        break;
                    
                    float3 pos = rayOrigin + rayDir * t;
                    
                    // Check if we're in the labyrinth zone (no fog there)
                    float labyrinthMask = getLabyrinthMask(pos);
                    
                    // Only add fog if mask > 0
                    if (labyrinthMask > 0.01)
                    {
                        // Height-based density
                        float heightFactor = exp(-max(0, pos.y - _FogHeight) * _FogFalloff);
                        
                        // Noise
                        float n = fbm(pos * _NoiseScale);
                        
                        // Final density (multiplied by labyrinth mask)
                        float density = _Density * heightFactor * (0.3 + n * 0.7) * labyrinthMask;
                        
                        if (density > 0.0001)
                        {
                            fogAccum += _FogColor.rgb * density * transmittance * _StepSize;
                            transmittance *= exp(-density * _StepSize);
                        }
                    }
                    
                    t += _StepSize;
                }
                
                // Final blend
                float3 finalColor = sceneColor.rgb * transmittance + fogAccum;
                
                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
