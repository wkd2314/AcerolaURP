Shader "Hidden/Voxel"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        HLSLINCLUDE

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        struct Attributes 
        {
            float4 positionOS	: POSITION;
            // float3 normalOS : NORMAL;
            // UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct Varyings 
        {
            float4 positionCS 	: SV_POSITION;
            float4 color : TEXCOORD0;
            // UNITY_VERTEX_INPUT_INSTANCE_ID
            // UNITY_VERTEX_OUTPUT_STEREO
        };

        struct VoxelData
        {
            float3 position;
            float4 color;
        };

        CBUFFER_START(UnityPerMaterial)
        float _VoxelSize;
        float3 _SmokeOrigin;
        CBUFFER_END

        StructuredBuffer<VoxelData> _VoxelsBuffer;

        Varyings vert (Attributes input, uint instanceID : SV_INSTANCEID)
        {
            Varyings output;
            
            // UNITY_SETUP_INSTANCE_ID(input);
			// UNITY_TRANSFER_INSTANCE_ID(input, output);
			// UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
            
            float4x4 scaleMat = {_VoxelSize, 0, 0, 0,
                                 0, _VoxelSize, 0, 0,
                                 0, 0, _VoxelSize, 0,
                                 0, 0, 0, 1};
            float4 positionOS = mul(scaleMat, input.positionOS);
            positionOS += float4(_SmokeOrigin.xyz, 0);

            VertexPositionInputs positionInputs = GetVertexPositionInputs(positionOS.xyz);

            float3 voxelPosition = _VoxelsBuffer[instanceID].position;
            
            float3 positionWS = positionInputs.positionWS + voxelPosition;

            output.positionCS = TransformWorldToHClip(positionWS);
            output.color = _VoxelsBuffer[instanceID].color;
            return output;
        }

        float4 frag (Varyings input) : SV_Target
        {
            // UNITY_SETUP_INSTANCE_ID(input);
			// UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            clip(input.color.a - 0.5);
            
            return input.color;
        }

        ENDHLSL

        Pass
        {
            Tags{"LightMode"="SRPDefaultUnlit"}
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            
            ENDHLSL
        }

//        Pass
//        {
//            Tags{"LightMode"="ShadowCaster"}
//            ZWrite On
//            ZTest LEqual
//            ColorMask 0
//            
//            HLSLPROGRAM
//            #pragma vertex vert
//            #pragma fragment frag
//            
//            #pragma  multi_compile_shadowcaster
//            
//            ENDHLSL
//        }
//
//        Pass
//        {
//            Tags 
//            {
//                "LightMode"="DepthOnly"
//            }
//            
//            HLSLPROGRAM
//
//            #pragma vertex vert
//            #pragma fragment frag
//
//            ENDHLSL
//        }
//         
//        Pass
//        {
//            Tags 
//            {
//                "LightMode"="DepthNormalsOnly"
//            }
//            HLSLPROGRAM
//
//            #pragma vertex vert
//            #pragma fragment frag
//
//            ENDHLSL
//        }
    }
}
