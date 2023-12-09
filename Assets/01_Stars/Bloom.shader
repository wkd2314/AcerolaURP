Shader "Hidden/Bloom"
{
     HLSLINCLUDE
        #pragma editor_sync_compilation
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    ENDHLSL
    
    SubShader
    {
        //Tags { "RenderType"="Opaque" }
        //LOD 100

        // Filter pixels (Bright ones)
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert 
            #pragma fragment PrefilterAndDownSample
            
            #include "BloomMain.hlsl"
            
            ENDHLSL
        }

        // DownSample
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert 
            #pragma fragment DownSample
            
            #include "BloomMain.hlsl"
            
            ENDHLSL
        }

        // UpSample
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert 
            #pragma fragment UpSample
            
            #include "BloomMain.hlsl"
            
            ENDHLSL
        }

        // BlendBloom
        Pass
        {
            Name "Additive Blend"
            
            HLSLPROGRAM
            #pragma vertex Vert 
            #pragma fragment BlendBloom
            
            #include "BloomMain.hlsl"
            
            ENDHLSL
        }
    }
}
