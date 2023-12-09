#ifndef Include_Bloom
#define Include_Bloom

TEXTURE2D_X(_CameraOpaqueTexture);
SAMPLER(sampler_CameraOpaqueTexture);

sampler2D _MainTex;
float4 _MainTex_TexelSize;

float _Threshold, _SoftThreshold;

float _DownDelta;
float _UpDelta;

sampler2D _OriginalTex;
float _Intensity;

float4 _BlitTexture_TexelSize;

float4 Sample(float2 uv)
{
	return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);
}

float4 SampleBox(float2 uv, float delta)
{
	float4 o = _BlitTexture_TexelSize.xyxy * float2(-delta, delta).xxyy;
	float4 s = Sample(uv + o.xy) + Sample(uv + o.xw) + Sample(uv + o.zy) + Sample(uv + o.zw);

	return s* 0.25f;
}

// prefilter and downsample
float4 Prefilter(float4 col)
{

	// Thresholding
	// half brightness = Max3(col.r, col.g, col.b);
	// half softness = clamp(brightness - Threshold + ThresholdKnee, 0.0, 2.0 * ThresholdKnee);
	// softness = (softness * softness) / (4.0 * ThresholdKnee + 1e-4);
	// half multiplier = max(brightness - Threshold, softness) / max(brightness, 1e-4);
	// color *= multiplier;

	
	half brightness = max(col.r, max(col.g, col.b)); // 0 ~ 1
	half knee = _Threshold * _SoftThreshold;
	half soft = brightness - _Threshold + knee;
	soft = clamp(soft, 0, 2 * knee);
	soft = soft * soft / (4 * knee + 1e-4);
	half contribution = max(soft, brightness - _Threshold);
	contribution /= max(contribution, 1e-4);

	return col * contribution;
}

float4 PrefilterAndDownSample (Varyings i) : SV_Target
{
	return Prefilter(SampleBox(i.texcoord, _DownDelta));
}

float4 DownSample (Varyings i) : SV_Target
{
	return SampleBox(i.texcoord, _DownDelta);
}

float4 UpSample (Varyings i) : SV_Target
{
	return SampleBox(i.texcoord, _UpDelta);
}

float4 BlendBloom (Varyings i) : SV_Target
{
	float4 col = tex2D(_OriginalTex, i.texcoord);
	// col.rgb += pow(_Intensity * pow(SampleBox(i.texcoord, 0.5f), 1.0f / 2.2f), 2.2f);
	col.rgb += _Intensity * SampleBox(i.texcoord, 0.5f);
	
	return col;
}

#endif