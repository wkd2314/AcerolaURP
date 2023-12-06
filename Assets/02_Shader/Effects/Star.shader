Shader "Unlit/Star"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR] _Emission ("Emission", Color) = (0, 0, 0)
        _EmissionDistanceModifier ("Emission Distance Modifier", Range(0.0, 1.0)) = 0.0
        _MinEmissionMod ("Minimum Emission Modifier", Range(0.0, 1.0)) = 0.0
        _MaxEmissionMod ("Maximum Emission Modifier", Range(0.0, 1.0)) = 1.0
        _MinSize("Min size", Range(0.0, 1.0)) = 0.0
        _MaxSize("Max size", Range(1.0, 2.0)) = 1.0
        _MaxOffset ("Maximum Positional Offset", Range(0.0, 1.0)) = 0.0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque" 
            "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True"
        }
        LOD 100
            
        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
            }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "StarShared.hlsl"
            
            ENDHLSL
        }

        // shadow caster for depth texture
        Pass
        {
            Tags{"LightMode"="ShadowCaster"}
            ZWrite On
            ZTest LEqual
            ColorMask 0
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment fragShadow
            
            #pragma  multi_compile_shadowcaster
            #include "StarShared.hlsl"
            
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
            #pragma fragment fragDepth
            #include "StarShared.hlsl"

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
            #pragma fragment fragDepth
            #include "StarShared.hlsl"

            ENDHLSL
        }
    }
}
