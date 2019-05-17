Shader "Allegorithmic/Material layering" {
	Properties{
		_Material1_Color ("Material1 Color", 2D) = "transparent" {}
		_Material1_RM ("Material1 RM", 2D) = "transparent" {}
		_Material1_Normal ("Material1 Normal", 2D) = "transparent" {}

		_Material2_Color ("Material2 Color", 2D) = "transparent" {}
		_Material2_RM ("Material2 RM", 2D) = "transparent" {}
		_Material2_Normal ("Material2 Normal", 2D) = "transparent" {}

		_Material3_Color ("Material3 Color", 2D) = "transparent" {}
		_Material3_RM ("Material3 RM", 2D) = "transparent" {}
		_Material3_Normal ("Material3 Normal", 2D) = "transparent" {}

		_Material4_Color ("Material4 Color", 2D) = "transparent" {}
		_Material4_RM ("Material4 RM", 2D) = "transparent" {}
		_Material4_Normal ("Material4 Normal", 2D) = "transparent" {}

		_Mask ("Mask RGB", 2D) = "transparent" {}

		BaseNormal ("Mesh normal", 2D) = "transparent" {}

		Material1_Scale ("Material1_uScale", Range (0.0,128.0)) = 1.00
		Material1_NormalIntensity ("Material1_NormalIntensity", Range (0.0,1.0)) = 1.00

		Material2_Scale ("Material2_uScale", Range (0.0,128.0)) = 1.00
		Material2_NormalIntensity ("Material2_NormalIntensity", Range (0.0,1.0)) = 1.00
		Material2_NormalFromMaskIntensity ("Material2_NormalFromMaskIntensity", Range (-10.0,10.0)) = 0.00
		Material2_NormalFromMaskOffset ("Material2_NormalFromMaskOffset", Range (-10.0,10.0)) = 0.10

		Material3_Scale ("Material3_uScale", Range (0.0,128.0)) = 1.00
		Material3_NormalIntensity ("Material3_NormalIntensity", Range (0.0,1.0)) = 1.00
		Material3_NormalFromMaskIntensity ("Material3_NormalFromMaskIntensity", Range (-10.0,10.0)) = 0.00
		Material3_NormalFromMaskOffset ("Material3_NormalFromMaskOffset", Range (-10.0,10.0)) = 0.10

		Material4_Scale ("Material4_uScale", Range (0.0,128.0)) = 1.00
		Material4_NormalIntensity ("Material4_NormalIntensity", Range (0.0,1.0)) = 1.00
		Material4_NormalFromMaskIntensity ("Material4_NormalFromMaskIntensity", Range (-10.0,10.0)) = 0.00
		Material4_NormalFromMaskOffset ("Material4_NormalFromMaskOffset", Range (-10.0,10.0)) = 0.10

	}

	SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 100
			CGPROGRAM
			#pragma surface surfaceFunction Standard addshadow fullforwardshadows
			#pragma target 3.0
			#pragma multi_compile USE_NORMAL_FROM_MASK IGNORE_NORMAL_FROM_MASK

			// SAMPLERS
			UNITY_DECLARE_TEX2D (_Material1_Color);
			UNITY_DECLARE_TEX2D_NOSAMPLER (_Material1_RM);
			UNITY_DECLARE_TEX2D_NOSAMPLER (_Material1_Normal);

			UNITY_DECLARE_TEX2D_NOSAMPLER (_Material2_Color);
			UNITY_DECLARE_TEX2D_NOSAMPLER (_Material2_RM);
			UNITY_DECLARE_TEX2D_NOSAMPLER (_Material2_Normal);

			UNITY_DECLARE_TEX2D_NOSAMPLER (_Material3_Color);
			UNITY_DECLARE_TEX2D_NOSAMPLER (_Material3_RM);
			UNITY_DECLARE_TEX2D_NOSAMPLER (_Material3_Normal);

			UNITY_DECLARE_TEX2D_NOSAMPLER (_Material4_Color);
			UNITY_DECLARE_TEX2D_NOSAMPLER (_Material4_RM);
			UNITY_DECLARE_TEX2D_NOSAMPLER (_Material4_Normal);

			UNITY_DECLARE_TEX2D_NOSAMPLER (BaseNormal);

			UNITY_DECLARE_TEX2D_NOSAMPLER (_Mask);

			float Material1_Scale;
			float Material1_NormalIntensity;

			float Material2_Scale;
			float Material2_NormalIntensity;
			float Material2_NormalFromMaskIntensity;
			float Material2_NormalFromMaskOffset;

			float Material3_Scale;
			float Material3_NormalIntensity;
			float Material3_NormalFromMaskIntensity;
			float Material3_NormalFromMaskOffset;

			float Material4_Scale;
			float Material4_NormalIntensity;
			float Material4_NormalFromMaskIntensity;
			float Material4_NormalFromMaskOffset;

			fixed _UseNormalFromMask;


			// SHADER INPUTS
			struct Input {
				float2 uv_Material1_Color;
			};

			// BLENDING FUNCTIONS
			float deg2rad (float deg) {
				return (deg / 180.0) * 3.14159;
			}
			float2 offset (float2 uv, float2 offsetValue) {
				return uv + offsetValue;
			}
			float2 scale (float2 uv, float2 scaleValue) {
				return uv * scaleValue;
			}
			float2 rotate (float2 uv, float angleInDegrees) {
				const float angleInRadians = deg2rad (angleInDegrees);
				const float cosVal = cos (angleInRadians);
				const float sinVal = sin (angleInRadians);
				const float2x2 rotationMatrix = float2x2 (cosVal, -sinVal, sinVal, cosVal);
				return mul (rotationMatrix, uv);
			}

#if USE_NORMAL_FROM_MASK
			float3 NormalFromHeight (fixed index, float Offset, float Intensity, float2 UVs)
			{
				float2 UVs2 = UVs + float2 (Offset * 0.001, 0.0);
				float2 UVs3 = UVs + float2 (0.0, Offset * 0.001);

				float Channel1 = UNITY_SAMPLE_TEX2D_SAMPLER (_Mask, _Material1_Color, UVs2)[index] - UNITY_SAMPLE_TEX2D_SAMPLER (_Mask, _Material1_Color, UVs)[index];
				float Channel2 = UNITY_SAMPLE_TEX2D_SAMPLER (_Mask, _Material1_Color, UVs3)[index] - UNITY_SAMPLE_TEX2D_SAMPLER (_Mask, _Material1_Color, UVs)[index];

				return cross (float3 (1.0, 0.0, Channel1 * Intensity), float3 (0.0, 1.0, Channel2 * Intensity));
			}
			float3 NormalFromMasks (float2 UVs, float3 masks)
			{
				float3 NormalMask1 = NormalFromHeight (0, Material2_NormalFromMaskOffset, Material2_NormalFromMaskIntensity, UVs);
				float3 NormalMask2 = NormalFromHeight (1, Material3_NormalFromMaskOffset, Material3_NormalFromMaskIntensity, UVs);
				float3 NormalMask3 = NormalFromHeight (2, Material4_NormalFromMaskOffset, Material4_NormalFromMaskIntensity, UVs);

				float3 result = float3 (0.0, 0.0, 0.0);

				result = lerp (NormalMask1, NormalMask2, masks.g);
				result = lerp (result, NormalMask3, masks.b);

				return result;
			}
#endif
			float3 mixNormal (float3 masks, float2 uv1, float2 uv2, float2 uv3, float2 uv4)
			{
				float3 result = float3 (0.0, 0.0, 0.0);
				float3 NormalMap1 = UNITY_SAMPLE_TEX2D_SAMPLER (_Material1_Normal, _Material1_Color, uv1).rgb;
				float3 NormalMap2 = UNITY_SAMPLE_TEX2D_SAMPLER (_Material2_Normal, _Material1_Color, uv2).rgb;
				float3 NormalMap3 = UNITY_SAMPLE_TEX2D_SAMPLER (_Material3_Normal, _Material1_Color, uv3).rgb;
				float3 NormalMap4 = UNITY_SAMPLE_TEX2D_SAMPLER (_Material4_Normal, _Material1_Color, uv4).rgb;

				if (NormalMap1.x == 0 && NormalMap1.y == 0 && NormalMap1.z == 0)
					NormalMap1 = float3 (0.5, 0.5, 1.0);

				if (NormalMap2.x == 0 && NormalMap2.y == 0 && NormalMap2.z == 0)
					NormalMap2 = float3 (0.5, 0.5, 1.0);

				if (NormalMap3.x == 0 && NormalMap3.y == 0 && NormalMap3.z == 0)
					NormalMap3 = float3 (0.5, 0.5, 1.0);

				if (NormalMap4.x == 0 && NormalMap4.y == 0 && NormalMap4.z == 0)
					NormalMap4 = float3 (0.5, 0.5, 1.0);

				NormalMap1 = lerp (float3 (0.5, 0.5, 1.0), NormalMap1, Material1_NormalIntensity);
				NormalMap2 = lerp (float3 (0.5, 0.5, 1.0), NormalMap2, Material2_NormalIntensity);
				NormalMap3 = lerp (float3(0.5, 0.5, 1.0), NormalMap3, Material3_NormalIntensity);
				NormalMap4 = lerp (float3(0.5, 0.5, 1.0), NormalMap4, Material4_NormalIntensity);

				result = lerp (NormalMap1, NormalMap2, masks.r);
				result = lerp (result, NormalMap3, masks.g);
				result = lerp (result, NormalMap4, masks.b);

				return result;
			}

			void surfaceFunction (Input IN, inout SurfaceOutputStandard o) {
				// VARIABLES DEFINITION
				float4 BaseColor = (float4)0.0;
				float2 Roughness = (float2)0.0;
				float2 Metallic = (float2)0.0;
				float4 Normal = float4(0.0,0.0,1.0,1.0);

				float3 MaskSampler = UNITY_SAMPLE_TEX2D_SAMPLER (_Mask, _Material1_Color, IN.uv_Material1_Color).rgb;
				float mask1 = MaskSampler.r;
				float mask2 = MaskSampler.g;
				float mask3 = MaskSampler.b;

				float2 uvMaterial1 = scale (IN.uv_Material1_Color, Material1_Scale);
				float2 uvMaterial2 = scale (IN.uv_Material1_Color, Material2_Scale);
				float2 uvMaterial3 = scale (IN.uv_Material1_Color, Material3_Scale);
				float2 uvMaterial4 = scale (IN.uv_Material1_Color, Material4_Scale);

				float3 col = lerp (lerp (lerp (UNITY_SAMPLE_TEX2D_SAMPLER (_Material1_Color, _Material1_Color, uvMaterial1), UNITY_SAMPLE_TEX2D_SAMPLER (_Material2_Color, _Material1_Color, uvMaterial2),mask1), UNITY_SAMPLE_TEX2D_SAMPLER (_Material3_Color, _Material1_Color, uvMaterial3), mask2), UNITY_SAMPLE_TEX2D_SAMPLER (_Material4_Color, _Material1_Color, uvMaterial4), mask3).rgb;

				o.Albedo = col;

				float4 roughMetal = lerp (lerp (lerp (UNITY_SAMPLE_TEX2D_SAMPLER (_Material1_RM, _Material1_Color, uvMaterial1), UNITY_SAMPLE_TEX2D_SAMPLER (_Material2_RM, _Material1_Color, uvMaterial2),mask1), UNITY_SAMPLE_TEX2D_SAMPLER (_Material3_RM, _Material1_Color, uvMaterial3), mask2), UNITY_SAMPLE_TEX2D_SAMPLER (_Material4_RM, _Material1_Color, uvMaterial4), mask3);

				o.Smoothness = roughMetal.a;

				o.Metallic = roughMetal.x;

				float4 mesh_normal = float4(UnpackNormal (UNITY_SAMPLE_TEX2D_SAMPLER (BaseNormal, _Material1_Color, IN.uv_Material1_Color)), 1.0);
				float4 normal = mesh_normal;

				float3 layerNormals = mixNormal (MaskSampler, uvMaterial1, uvMaterial2, uvMaterial3, uvMaterial4);
				normal.xyz = normalize (float3 (layerNormals.xy + mesh_normal.xy, mesh_normal.z));

#if USE_NORMAL_FROM_MASK
				float3 normalMask = NormalFromMasks (IN.uv_Material1_Color, MaskSampler);
				float3 normalWithMask = normalize (float3 (normal.xy + normalMask.xy, normal.z)); //UDN combine method
				normal.xyz = normalWithMask;
#endif

				o.Normal = normal;

				o.Occlusion = 1.0;
				o.Alpha = 1.0;
			}
			ENDCG
		}
	CustomEditor "AlgLayeredMaterialEditor"
}
