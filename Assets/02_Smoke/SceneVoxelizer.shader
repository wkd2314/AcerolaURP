Shader "Hidden/SceneVoxelizer"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        HLSLINCLUDE
        #pragma vertex vert
        #pragma fragment frag

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "VoxelizeShared.hlsl"

        struct Attributes 
        {
            float4 positionOS	: POSITION;
            float3 normalOS : NORMAL;
        };

        struct Varyings 
        {
            float4 positionCS 	: SV_POSITION;
            // UNITY_VERTEX_OUTPUT_STEREO
            uint instanceID : SV_InstanceID;
        };
        
        RWStructuredBuffer<int> a ;
        Varyings vert (Attributes input, uint instanceID : SV_InstanceID)
        {
            Varyings output;
            output.instanceID = instanceID;
            
            float3 posisionOS = MultiplyByVoxelScale(input.positionOS.xyz, _VoxelSize);
            
            float3 positionWS = TransformObjectToWorld(posisionOS);
            
            output.positionCS = TransformWorldToHClip(positionWS);
            return output;
        }

        float4 frag (Varyings input) : SV_Target
        {
            return 0.5;
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

        Pass
        {
            Tags{"LightMode"="ShadowCaster"}
            ZWrite On
            ZTest LEqual
            ColorMask 0
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma  multi_compile_shadowcaster
            
            ENDHLSL
        }

        Pass
        {
            Tags 
            {
                "LightMode"="DepthOnly"
            }
            
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            ENDHLSL
        }
         
        Pass
        {
            Tags 
            {
                "LightMode"="DepthNormalsOnly"
            }
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            ENDHLSL
        }
    }
}
