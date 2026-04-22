Shader "Wonderm/StandardDithering_2Side_CustomActor" {
	Properties {
		_MainTex ("AlbedoTex", 2D) = "white" {}
		_AlbedoColor ("AlbedoColor", Vector) = (0.8823529,0.8823529,0.8823529,1)
		[MaterialToggle] _UseDither ("UseDither", Float) = 1
		_StartDither ("StartDither", Range(0, 5)) = 0
		_EndDither ("EndDither", Range(0, 10)) = 1
		_BumpMap ("BumpTex", 2D) = "bump" {}
		_BumpScales ("BumpScales", Range(0, 1)) = 0
		_MetalicTex ("MetalicTex", 2D) = "white" {}
		_Metallic ("Metallic", Range(0, 1)) = 0
		_Smoothness ("Smoothness", Range(0, 1)) = 0
		[MaterialToggle] _EmissionToogle ("EmissionToogle", Float) = 0
		_EmissionTex ("EmissionTex", 2D) = "gray" {}
		_EmissionScale ("EmissionScale", Range(0, 1)) = 1
		_EmissionColor ("EmissionColor", Vector) = (0.5,0.5,0.5,1)
		[MaterialToggle] _RimToogle ("RimToogle", Float) = 0
		_RimScale ("RimScale", Range(0.5, 1)) = 0.5
		[HDR] _RimColor ("RimColor", Vector) = (0.5,0.5,0.5,1)
		[HideInInspector] _NoRimColor ("NoRimColor", Vector) = (0,0,0,1)
		_OutLineWdith ("OutLineWdith", Range(-0.2, 0.2)) = 0
		_OutlineColor ("OutlineColor", Vector) = (0,0,0,1)
		[MaterialToggle] _Custom ("Custom", Float) = 0
		_Brightness ("Brightness", 2D) = "white" {}
		_Mask ("Mask", 2D) = "white" {}
		_CustomColor_RChannel ("CustomColor_RChannel", Vector) = (0.5,0.5,0.5,1)
		_CustomColor_RChannel_Light ("CustomColor_RChannel_Light", Vector) = (0.5,0.5,0.5,1)
		_CustomColor_GChannel ("CustomColor_GChannel", Vector) = (0.5,0.5,0.5,1)
		_CustomColor_GChannel_Light ("CustomColor_GChannel_Light", Vector) = (0.5,0.5,0.5,1)
		_CustomColor_BChannel ("CustomColor_BChannel", Vector) = (0.5,0.5,0.5,1)
		_CustomColor_BChannel_Light ("CustomColor_BChannel_Light", Vector) = (0.5,0.5,0.5,1)
		_Saturation ("Saturation", Vector) = (0.5,0.5,0.5,1)
		_Light_Pos_0 ("Light_Pos_0", Vector) = (0,0,0,0)
		_Light_Range_0 ("Light_Range_0", Float) = 0
		_Light_Color_0 ("Light_Color_0", Vector) = (0.5,0.5,0.5,1)
		_SimAttenRange ("SimAttenRange", Range(0, 1)) = 0.48
		_SimPower ("SimPower", Range(0, 1)) = 0.81
		_Light_Pos_1 ("Light_Pos_1", Vector) = (0,0,0,0)
		_Light_Range_1 ("Light_Range_1", Float) = 0
		_Light_Color_1 ("Light_Color_1", Vector) = (0.5,0.5,0.5,1)
		_Light_Pos_2 ("Light_Pos_2", Vector) = (0,0,0,0)
		_Light_Range_2 ("Light_Range_2", Float) = 0
		_Light_Color_2 ("Light_Color_2", Vector) = (0.5,0.5,0.5,1)
		_Light_Num ("Light_Num", Float) = 0
		[MaterialToggle] _SimLight ("SimLight", Float) = 0
		_SimIntensity ("SimIntensity", Range(0, 1)) = 0.52
		[HideInInspector] _Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
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

			struct Fragment_Stage_Input
			{
				float2 uv : TEXCOORD0;
			};

			float4 frag(Fragment_Stage_Input input) : SV_TARGET
			{
				return _MainTex.Sample(sampler_MainTex, input.uv.xy);
			}

			ENDHLSL
		}
	}
	//CustomEditor "ShaderForgeMaterialInspector"
}