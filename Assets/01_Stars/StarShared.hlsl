#pragma once

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Random.hlsl"

struct appdata
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float2 uv : TEXCOORD0;

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
    float2 uv : TEXCOORD0;
    float4 positionCS : SV_POSITION;
    float3 normal : TEXCOORD1;
    float4 positionWS : TEXCOORD2;
    float emissionMod : TEXCOORD3;

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct StarData
{
    float4 position;
    float4x4 rsMat;
};

sampler2D _MainTex;

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial) // Props
UNITY_DEFINE_INSTANCED_PROP(float3, _Emission)
UNITY_DEFINE_INSTANCED_PROP(float, _EmissionDistanceModifier)
UNITY_DEFINE_INSTANCED_PROP(float, _MinSize)
UNITY_DEFINE_INSTANCED_PROP(float, _MaxSize)
UNITY_DEFINE_INSTANCED_PROP(float, _MaxOffset)
UNITY_DEFINE_INSTANCED_PROP(float, _MinEmissionMod)
UNITY_DEFINE_INSTANCED_PROP(float, _MaxEmissionMod)
UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)




StructuredBuffer<StarData> _StarsBuffer;

// uint instanceID : SV_INSTANCEID 이거 지우고 위에거 넣어도 동일
v2f vert (appdata v, uint instanceID : SV_INSTANCEID)
{
    v2f o;
    float4 starPosition = _StarsBuffer[instanceID].position;
    float4 localPostion = v.vertex;

    float sizeMod = lerp(_MinSize, _MaxSize, Random(instanceID));
    localPostion *= sizeMod;

    localPostion = mul(_StarsBuffer[instanceID].rsMat, localPostion);

    // 각 별들 모양 수정
    float xOffset = lerp(-_MaxOffset, _MaxOffset, Random(starPosition.x + localPostion.x));
    float yOffset = lerp(-_MaxOffset, _MaxOffset, Random(starPosition.y + localPostion.y));
    float zOffset = lerp(-_MaxOffset, _MaxOffset, Random(starPosition.z + localPostion.z));
    
    float3 offset = float3(xOffset, yOffset, zOffset);
    localPostion.xyz += offset;
    
    float4 worldPosition = localPostion + starPosition;
    
    o.positionCS = TransformWorldToHClip(worldPosition.xyz);
    o.positionWS = worldPosition;
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    o.normal = mul(_StarsBuffer[instanceID].rsMat, float4(normalize(v.normal), 1.0)).xyz;
    o.emissionMod = lerp(_MinEmissionMod, _MaxEmissionMod, Random(instanceID));
    
    return o;
}

float4 frag (v2f i) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(i);
    
    float4 col = tex2D(_MainTex, i.uv);
    Light mainLight = GetMainLight();

    float ndotl = saturate(dot(i.normal, mainLight.direction)) * 0.5f + 0.5f;

    col *= ndotl;

    float viewDistance = length(_WorldSpaceCameraPos - i.positionWS.xyz);
    float emissionFactor = (_EmissionDistanceModifier / sqrt(log(2))) * viewDistance;
    emissionFactor = exp2(-emissionFactor);

    float4 maxEmission = float4(col.rgb + saturate(_Emission - i.emissionMod), 1.0f);
    
    return float4(lerp(maxEmission, col, emissionFactor * 0.95f).rgb, 1.0);
}

float4 fragShadow(v2f i) : SV_TARGET
{
    return 0;
}

void fragDepth(v2f i)
{
    
}