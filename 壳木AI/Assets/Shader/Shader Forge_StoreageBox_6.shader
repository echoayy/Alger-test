Shader "Shader Forge/StoreageBox" {
	Properties {
		_Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
		_Color ("Color", Vector) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0, 1)) = 0.5
		_Metallic ("Metallic", Range(0, 1)) = 0
		_Custom ("Custom", 2D) = "white" {}
		_BrightLimit ("CustomLightLimit", Range(0, 1)) = 1
		_DarkLimit ("CustomDarkLimtit", Range(0, 1)) = 0
		_Lerp ("Lerp", Range(0, 1)) = 1
		_CustomColor ("CustomColor", Vector) = (1,1,1,1)
		_Lerp2 ("Lerp2", Range(0, 1)) = 1
		_CustomColor2 ("CustomColor2", Vector) = (1,1,1,1)
		_Lerp3 ("Lerp3", Range(0, 1)) = 1
		_CustomColor3 ("CustomColor3", Vector) = (1,1,1,1)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _Custom;
		half4 _Color;
		half _Glossiness;
		half _Metallic;
		half _Lerp;
		half4 _CustomColor;
		half _Lerp2;
		half4 _CustomColor2;
		half _Lerp3;
		half4 _CustomColor3;

		struct Input {
			float2 uv_MainTex;
			float2 uv_Custom;
		};

		void surf(Input IN, inout SurfaceOutputStandard o) {
			fixed4 col = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			fixed4 custom = tex2D(_Custom, IN.uv_Custom);

			col.rgb = lerp(col.rgb, col.rgb * _CustomColor.rgb, custom.r * _Lerp);
			col.rgb = lerp(col.rgb, col.rgb * _CustomColor2.rgb, custom.g * _Lerp2);
			col.rgb = lerp(col.rgb, col.rgb * _CustomColor3.rgb, custom.b * _Lerp3);

			o.Albedo = col.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = col.a;
		}
		ENDCG
	}
	Fallback "Diffuse"
}
