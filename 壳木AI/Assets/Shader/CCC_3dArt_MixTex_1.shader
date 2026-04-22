Shader "CCC/3dArt/MixTex" {
	Properties {
		_Color ("Color", Vector) = (1,1,1,1)
		_Mask ("UV2Mask(RGB)", 2D) = "white" {}
		_SecondTex ("Layer 3 (Red)", 2D) = "black" {}
		_SecondTex_Emission ("Layer 1 Emission", Range(0, 30)) = 1
		_ThirdTex ("Layer 2 (Green)", 2D) = "black" {}
		_ThirdTex_Emission ("Layer 2 Emission", Range(0, 30)) = 1
		_FourthTex ("Layer 1 (Blue)", 2D) = "black" {}
		_FourthTex_Emission ("Layer 3 Emission", Range(0, 30)) = 1
		_MainTex ("Layer 0 (RGB)", 2D) = "white" {}
		_MainTex_Emission ("Layer 0 Emission", Range(0, 30)) = 1
		_Glossiness ("Smoothness", Range(0, 1)) = 0.5
		_Metallic ("Metallic", Range(0, 1)) = 0
		_UV2Tex ("Uv2Albedo (RGB)", 2D) = "white" {}
		_NormalTex ("Uv2Normal", 2D) = "bump" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _Mask;
		sampler2D _SecondTex;
		sampler2D _ThirdTex;
		sampler2D _FourthTex;
		sampler2D _NormalTex;

		half4 _Color;
		half _Glossiness;
		half _Metallic;
		half _MainTex_Emission;
		half _SecondTex_Emission;
		half _ThirdTex_Emission;
		half _FourthTex_Emission;

		struct Input {
			float2 uv_MainTex;
			float2 uv_Mask;
			float2 uv_NormalTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o) {
			fixed4 mask = tex2D(_Mask, IN.uv_Mask);
			fixed4 base = tex2D(_MainTex, IN.uv_MainTex);
			fixed4 layer1 = tex2D(_SecondTex, IN.uv_MainTex);
			fixed4 layer2 = tex2D(_ThirdTex, IN.uv_MainTex);
			fixed4 layer3 = tex2D(_FourthTex, IN.uv_MainTex);

			fixed3 col = base.rgb * (1.0 - mask.r - mask.g - mask.b);
			col += layer1.rgb * mask.r;
			col += layer2.rgb * mask.g;
			col += layer3.rgb * mask.b;

			o.Albedo = col * _Color.rgb;
			o.Normal = UnpackNormal(tex2D(_NormalTex, IN.uv_NormalTex));
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = _Color.a;
		}
		ENDCG
	}
	Fallback "Diffuse"
}
