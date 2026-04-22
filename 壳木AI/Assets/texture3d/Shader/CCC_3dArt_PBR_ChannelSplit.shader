Shader "CCC/3dArt/PBR_ChannelSplit" {
	Properties {
		_Multiplier ("Multiplier", Range(0.5, 10)) = 1
		_Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
		_Color ("Color", Vector) = (1,1,1,1)
		[NoScaleOffset] _MainTex ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _NormalTex ("Normal Texture (RGB)", 2D) = "bump" {}
		_NormalStrength ("Normal Strength", Range(-1, 2)) = 1
		[NoScaleOffset] _MRA_Tex ("Metallic(R), Roughness(G), AmbientOcclusion (B)", 2D) = "black" {}
		_Metallic ("Metallic", Range(-0, 1)) = 0
		_Smoothness ("Smoothness", Range(-0.5, 1)) = 0
		_HeightStrength ("Height Strength", Range(-0.25, 0.25)) = 0
		_AmbientStrength ("Ambient", Range(-2, 2)) = 0
		_EmissionTex ("Emission Tex (RGB)", 2D) = "black" {}
		_EmissionStrength ("Emission Strength", Float) = 1
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4x4 unity_ObjectToWorld;
			float4x4 unity_MatrixVP;
			float4 _MainTex_ST;

			struct Vertex_Stage_Input
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Vertex_Stage_Output
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
			};

			Vertex_Stage_Output vert(Vertex_Stage_Input input)
			{
				Vertex_Stage_Output output;
				output.uv = (input.uv.xy * _MainTex_ST.xy) + _MainTex_ST.zw;
				output.pos = mul(unity_MatrixVP, mul(unity_ObjectToWorld, input.pos));
				return output;
			}

			Texture2D<float4> _MainTex;
			SamplerState sampler_MainTex;
			float4 _Color;

			struct Fragment_Stage_Input
			{
				float2 uv : TEXCOORD0;
			};

			float4 frag(Fragment_Stage_Input input) : SV_TARGET
			{
				return _MainTex.Sample(sampler_MainTex, input.uv.xy) * _Color;
			}

			ENDHLSL
		}
	}
	Fallback "Transparent/Cutout/VertexLit"
}