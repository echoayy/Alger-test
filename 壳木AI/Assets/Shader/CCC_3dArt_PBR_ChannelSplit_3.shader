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
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _NormalTex;
		sampler2D _MRA_Tex;
		sampler2D _EmissionTex;

		half4 _Color;
		half _Multiplier;
		half _NormalStrength;
		half _Metallic;
		half _Smoothness;
		half _AmbientStrength;
		half _EmissionStrength;

		struct Input {
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o) {
			fixed4 col = tex2D(_MainTex, IN.uv_MainTex) * _Color * _Multiplier;
			o.Albedo = col.rgb;
			o.Alpha = col.a;

			fixed3 nrm = UnpackNormal(tex2D(_NormalTex, IN.uv_MainTex));
			nrm.xy *= _NormalStrength;
			o.Normal = normalize(nrm);

			fixed4 mra = tex2D(_MRA_Tex, IN.uv_MainTex);
			o.Metallic = mra.r * _Metallic;
			o.Smoothness = (1.0 - mra.g) + _Smoothness;
			o.Occlusion = lerp(1.0, mra.b, 1.0 + _AmbientStrength);

			o.Emission = tex2D(_EmissionTex, IN.uv_MainTex).rgb * _EmissionStrength;
		}
		ENDCG
	}
	Fallback "Standard"
}
