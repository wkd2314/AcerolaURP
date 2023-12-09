Shader "Custom/CheckerShader" {
	Properties {
		_BlockSize ("Block Size", Range(0.001, 1)) = 0.17
		_ColorBase ("Base Color", Range(0,1)) = 0.05
		_ColorDiff ("Color Contrast", Range(0, 1)) = 0.47
		_ShadowStrength ("Shadow Strength", Range(0,1)) = 0.5
	}
	SubShader {
		Tags {
			"RenderPipeline"="UniversalPipeline"
			"RenderType"="Opaque"
			"Queue"="Geometry"
			"UniversalMaterialType" = "Lit" "IgnoreProjector" = "True"
		}

		HLSLINCLUDE
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

		CBUFFER_START(UnityPerMaterial)
		float4 _BaseMap_ST;
		float4 _BaseColor;
		float _BlockSize;
		float _ColorDiff;
		float _ColorBase;
		float _ShadowStrength;
		CBUFFER_END


			
		// Structs
		struct Attributes {
			float4 positionOS	: POSITION;
			float3 normalOS : NORMAL;
		};

		struct Varyings {
			float4 positionCS 	: SV_POSITION;
			float4 positionOS   : TEXCOORD0;
			float3 positionWS : TEXCOORD1;
			float3 scale : TEXCOORD2;
		};

		// Vertex Shader
		Varyings LitPassVertex(Attributes input) {
			Varyings output;

			VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
			output.positionCS = positionInputs.positionCS;
			output.positionOS = input.positionOS;
			
			output.scale = float3 (
				length(unity_ObjectToWorld._m00_m10_m20),
				length(unity_ObjectToWorld._m01_m11_m21),
				length(unity_ObjectToWorld._m02_m12_m22)
			);
			output.positionWS = positionInputs.positionWS;
			
			return output;
		}

		// Fragment Shader
		half4 LitPassFragment(Varyings input) : SV_Target {
			float3 checker = floor(input.positionOS.xyz / _BlockSize * input.scale) * 0.5;
			float color = frac(checker.x + checker.y + checker.z) * _ColorDiff + _ColorBase;

			float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
			Light light = GetMainLight(shadowCoord);
			
			float shadowColor = lerp((1.0 - _ShadowStrength), 1.0, light.shadowAttenuation);
			
			return color * shadowColor;
		}

		half4 EmptyFragment(Varyings input) : SV_Target {
			return 0;
		}
		
		ENDHLSL 

		Pass {
			Name "Checker"
			Tags { "LightMode"="UniversalForward" }

			HLSLPROGRAM
			#pragma vertex LitPassVertex
			#pragma fragment LitPassFragment
			
			// Material Keywords
			#pragma shader_feature _ALPHATEST_ON

			// URP Keywords
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			// Note, v11 changes this to :
			// #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN

			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING // v10+ only, renamed from "_MIXED_LIGHTING_SUBTRACTIVE"
			#pragma multi_compile _ SHADOWS_SHADOWMASK // v10+ only

			
			
			ENDHLSL
		}

		// UsePass "Universal Render Pipeline/Lit/ShadowCaster"
		// UsePass "Universal Render Pipeline/Lit/DepthOnly"
		// Would be nice if we could just use the passes from existing shaders,
		// However this breaks SRP Batcher compatibility. Instead, we should define them :

		Pass {
			Name "ShadowCaster"
			Tags { "LightMode"="ShadowCaster" }

			ZWrite On
			ZTest LEqual

			HLSLPROGRAM
			#pragma vertex LitPassVertex
			#pragma fragment EmptyFragment

			ENDHLSL
		}
		

		// DepthOnly, used for Camera Depth Texture (if cannot copy depth buffer instead, and the DepthNormals below isn't used)
		Pass {
			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }

			ColorMask 0
			ZWrite On
			ZTest LEqual

			HLSLPROGRAM
			#pragma vertex LitPassVertex
			#pragma fragment EmptyFragment
			
			ENDHLSL
		}

		// DepthNormals, used for SSAO & other custom renderer features that request it
		Pass {
			Name "DepthNormals"
			Tags { "LightMode"="DepthNormals" }

			ZWrite On
			ZTest LEqual

			HLSLPROGRAM
			#pragma vertex LitPassVertex
			#pragma fragment EmptyFragment
			
			ENDHLSL
		}

	}
	Fallback "Diffuse" 
}