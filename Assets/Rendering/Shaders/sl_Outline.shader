Shader "Selene/Outline" {
    
    Properties {
        [HideInInspector] _MainTex("Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _NormalOutlineWidth ("Normal Outline Width", Float) = 0.5
        _NotmalOutlineCutoff ("Normal Outline Cutoff", Range(0,1)) = 0.9999
        _DepthOutlineWidth ("Depth Outline Width", Float) = 0.5
        _DepthOutlineCutoff ("Depth Outline Cutoff", Range(0,1)) = 0.005
        _MaskOutlineWidth ("Mask Outline Width", Float) = 1
        _MaskOutlineCutoff ("Mask Outline Cutoff", Range(0,1)) = 0.1
        _NoiseScale ("Noise Scale", Range(0,1)) = 0
        _NoiseIntensity ("Noise Intensity", Range(0,1)) = 0
    }
    SubShader {

        Tags { "RenderType"="Opaque" }
        LOD 100

        HLSLINCLUDE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)

                sampler2D _MainTex;
                float4 _OutlineColor;
                float _NormalOutlineWidth;
                float _NotmalOutlineCutoff;
                float _DepthOutlineWidth;
                float _DepthOutlineCutoff;
                float _MaskOutlineWidth;
                float _MaskOutlineCutoff;
                float _NoiseScale;
                float _NoiseIntensity;

            CBUFFER_END
        


            struct VertexInput {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct VertexOutput{
                float4 positionCS : SV_POSITION;
                float3 normalVS : TEXCOORD1;
                float2 uv : TEXCOORD0;
            };

        ENDHLSL
        
        Pass {

            Name "RenderOutlines"

            HLSLPROGRAM
                    
                #pragma vertex ForwardPassVertex
                #pragma fragment ForwardPassFragment
                
                uniform sampler2D _ViewSpaceNormals;
                uniform sampler2D _OutlineMask;


                // 5-point Sobel filter

                static float2 samplePoints[5] = {
                                   float2(0, 1),
                    float2(-1, 0), float2(0, 0), float2(1, 1),
                                   float2(0, -1),
                };

                static float2 convolutionMatrix[5] = {
                                  float2(0, 2),
                    float2(2, 0), float2(0, 0), float2(-2, 0),
                                  float2(0, -2)
                };


                // 9-point Sobel filter

                // static float2 samplePoints[9] = {
                //     float2(-1, 1), float2(0, 1), float2(1, 1),
                //     float2(-1, 0), float2(0, 0), float2(1, 1),
                //     float2(-1, -1), float2(0, -1), float2(1, -1),
                // };

                // static float2 convolutionMatrix[9] = {
                //     float2(1, 1), float2(0, 2), float2(-1, 1),
                //     float2(2, 0), float2(0, 0), float2(-2, 0),
                //     float2(1, -1), float2(0, -2), float2(-1, -1)
                // };


                // Laplacian filter

                // static float2 samplePoints[9] = {
                //     float2(-1, 1), float2(0, 1), float2(1, 1),
                //     float2(-1, 0), float2(0, 0), float2(1, 1),
                //     float2(-1, -1), float2(0, -1), float2(1, -1),
                // };

                // static float2 convolutionMatrix[9] = {
                //     float2(-1, 0), float2(-1, 0), float2(-1, 0),
                //     float2(-1, 0), float2(8, 0), float2(-1, 0),
                //     float2(-1, 0), float2(-1, 0), float2(-1, 0)
                // };


                float4 SobelSampleNormal(float2 uv) {
                    
                    float2 sobelX = 0;
                    float2 sobelY = 0;
                    float2 sobelZ = 0;
                    
                    [unroll] for (int i = 0; i < 5; i++) {

                        float2 uvOffset = samplePoints[i];
                        uvOffset.x /= _ScreenParams.x;
                        uvOffset.y /= _ScreenParams.y;
                        
                        float3 normal = tex2D(_ViewSpaceNormals, uv + uvOffset * _NormalOutlineWidth);
                        
                        float2 kernel = convolutionMatrix[i];
                        // Accumulate samples for each coordinate
                        sobelX += normal.x * kernel;
                        sobelY += normal.y * kernel;
                        sobelZ += normal.z * kernel;
                    }
                    // Get the final sobel value
                    return max(length(sobelX), max(length(sobelY), length(sobelZ)));

                }

                float SobelSampleDepth(float2 uv) {

                    float2 sobel = 0;
                    
                    [unroll] for (int i = 0; i < 5; i++) {

                        float2 uvOffset = samplePoints[i];
                        uvOffset.x /= _ScreenParams.x;
                        uvOffset.y /= _ScreenParams.y;

                        float depth = SampleSceneDepth(uv + uvOffset * _DepthOutlineWidth);
                        sobel += depth * convolutionMatrix[i];
                    }
                    // Get the final sobel value
                    return length(sobel);
                }

                float SobelSampleMask(float2 uv) {

                    float2 sobel = 0;
                    
                    [unroll] for (int i = 0; i < 5; i++) {

                        float2 uvOffset = samplePoints[i];
                        uvOffset.x /= _ScreenParams.x;
                        uvOffset.y /= _ScreenParams.y;

                        float depth = tex2D(_OutlineMask, uv + uvOffset * 1).r;
                        sobel += depth * convolutionMatrix[i];
                    }
                    // Get the final sobel value
                    return length(sobel);
                }

                VertexOutput ForwardPassVertex(VertexInput input) {
                    
                    VertexOutput output = (VertexOutput)0;
                    output.positionCS = TransformWorldToHClip( TransformObjectToWorld(input.positionOS.xyz) );
                    output.normalVS = normalize(mul((float3x3)UNITY_MATRIX_MV, input.normalOS));
                    output.uv = input.uv;

                    return output;
                }

                float4 ForwardPassFragment(VertexOutput input) : SV_Target {

                    float3 sceneColor = tex2D(_MainTex, input.uv).rgb;
                    // return tex2D(_OutlineMask, input.uv).r;
                    float3 outlineColor = lerp(sceneColor, _OutlineColor.rgb, _OutlineColor.a);

                    float sobelNormalIntensity = 0;
                    if (_NormalOutlineWidth > 0 && _NotmalOutlineCutoff < 1) {
                        float3 sobelNormal = SobelSampleNormal(input.uv).rgb;
                        sobelNormalIntensity = saturate(sobelNormal.x + sobelNormal.y + sobelNormal.z);
                        sobelNormalIntensity = sobelNormalIntensity > _NotmalOutlineCutoff ? 1 : 0;
                    }

                    float sobelDepthIntensity = 0;
                    if (_DepthOutlineWidth > 0 && _DepthOutlineCutoff < 1) {
                        sobelDepthIntensity = SobelSampleDepth(input.uv);
                        sobelDepthIntensity = sobelDepthIntensity > _DepthOutlineCutoff ? 1 : 0;
                    }

                    float sobelMaskIntensity = 0;
                    if (_MaskOutlineWidth > 0 && _MaskOutlineCutoff < 1) {
                        sobelMaskIntensity = SobelSampleMask(input.uv);
                        sobelMaskIntensity = sobelMaskIntensity > _MaskOutlineCutoff ? 1 : 0;
                    }
                    
                    float sobelIntensity = saturate(max(sobelNormalIntensity, max(sobelDepthIntensity, sobelMaskIntensity)));
                    

                    float3 finalColor = lerp(sceneColor, outlineColor, sobelIntensity);
                    return float4(finalColor.r, finalColor.g, finalColor.b, 1);
                }

            ENDHLSL
        }
    }
}
