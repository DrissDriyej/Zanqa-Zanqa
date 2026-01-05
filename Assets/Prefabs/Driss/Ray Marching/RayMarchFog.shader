Shader "Custom/RayMarchFogURP"
{
    Properties
    {
        _FogColor ("Fog Color", Color) = (1.0, 0.8, 0.6, 1.0)
        _Density ("Density", Range(0, 0.2)) = 0.01
        _StepSize ("Step Size", Range(0.1, 5)) = 1.5
        _MaxDistance ("Max Distance", Range(10, 200)) = 80
        _NoiseScale ("Noise Scale", Range(0.01, 0.5)) = 0.05
        _FogHeight ("Fog Height", Range(-50, 100)) = 10
        _FogFalloff ("Fog Falloff", Range(0.01, 0.5)) = 0.1
        
        [Header(Labyrinth Zone No Fog)]
        _LabyMinX ("Labyrinth Min X", Float) = -55
        _LabyMaxX ("Labyrinth Max X", Float) = 25
        _LabyMinZ ("Labyrinth Min Z", Float) = -130
        _LabyMaxZ ("Labyrinth Max Z", Float) = -5
        _LabyFade ("Fade Distance", Range(1, 50)) = 5
        
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
                
                float _LabyMinX;
                float _LabyMaxX;
                float _LabyMinZ;
                float _LabyMaxZ;
                float _LabyFade;
                
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
                float n11 = lerp(n011, n111, f.y);
                
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
            // Returns 0 = inside labyrinth (NO FOG), 1 = outside (full fog)
            float getLabyrinthMask(float3 pos)
            {
                // Strict check: Are we inside the defined box?
                bool insideX = (pos.x >= _LabyMinX) && (pos.x <= _LabyMaxX);
                bool insideZ = (pos.z >= _LabyMinZ) && (pos.z <= _LabyMaxZ);

                if (insideX && insideZ)
                {
                    // WE ARE INSIDE: ABSOLUTELY NO FOG
                    return 0.0;
                }

                // If we are here, we are outside.
                // Calculate distance to the box for external fade
                float dx = max(0, max(_LabyMinX - pos.x, pos.x - _LabyMaxX));
                float dz = max(0, max(_LabyMinZ - pos.z, pos.z - _LabyMaxZ));
                float distToBox = length(float2(dx, dz));

                // Fade in fog as we move away from the box
                return saturate(distToBox / _LabyFade);
            };

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
                
                // Dithering
                float dither = frac(sin(dot(uv.xy, float2(12.9898, 78.233))) * 43758.5453);
                
                // --- WEATHER SYSTEM (FOG WAVES) ---
                // Modulate global density over time
                // Cycle duration approx 20-30 seconds
                // Shift phase by -1.6 (approx -PI/2) to start at -1 (Minimum/Clear)
                float weatherCycle = sin(_Time.y * 0.2 - 1.6); 
                weatherCycle = weatherCycle * 0.5 + 0.5; // 0 to 1
                
                // Make it sparse: Fog only appears 50% of the time
                // Remap: 0..1 -> -0.5 .. 1.0 clamped to 0..1?
                // Let's keep it simple: Smooth oscillation
                
                float globalDensityMod = smoothstep(0.2, 0.8, weatherCycle); 
                // Result: Clear -> Fade In -> Thick -> Fade Out -> Clear

                // --- OPTIMIZATION & LOGIC ---
                // Check if density is too low to bother rendering
                if (globalDensityMod < 0.01)
                {
                    return float4(sceneColor.rgb, 1.0);
                }
                // -----------------------------

                // Ray march
                float t = 1.0 + dither * _StepSize;
                float transmittance = 1.0;
                float3 fogAccum = float3(0, 0, 0);
                
                // Animation speed for heat effect (scrolling noise)
                float3 animOffset = float3(0, -_Time.y * 2.0, 0); 
                
                [loop]
                for (int step = 0; step < 32; step++)
                {
                    if (t > _MaxDistance || transmittance < 0.05)
                        break;
                    
                    float3 pos = rayOrigin + rayDir * t;
                    
                    // Height-based density
                    float heightFactor = exp(-max(0, pos.y - _FogHeight) * _FogFalloff);
                        
                    // Animated Noise
                    float n = fbm((pos + animOffset) * _NoiseScale);
                        
                    float noiseFactor = 0.4 + n * 0.6;
                    
                    // Final density: Base * Height * Noise * TIME(Weather)
                    float density = _Density * heightFactor * noiseFactor * globalDensityMod;
                        
                    if (density > 0.0001)
                    {
                        float stepAtten = exp(-density * _StepSize);
                        fogAccum += _FogColor.rgb * (1.0 - stepAtten) * transmittance;
                        transmittance *= stepAtten;
                    }
                    
                    t += _StepSize;
                }
                
                return float4(lerp(sceneColor.rgb, fogAccum + sceneColor.rgb * transmittance, 1.0), 1.0);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
