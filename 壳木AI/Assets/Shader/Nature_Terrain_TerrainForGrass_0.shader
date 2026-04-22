Shader "Nature/Terrain/TerrainForGrass" {
	Properties {
		[HideInInspector] _Control ("Control (RGBA)", 2D) = "red" {}
		[HideInInspector] _Splat3 ("Layer 3 (A)", 2D) = "white" {}
		[HideInInspector] _Splat2 ("Layer 2 (B)", 2D) = "white" {}
		[HideInInspector] _Splat1 ("Layer 1 (G)", 2D) = "white" {}
		[HideInInspector] _Splat0 ("Layer 0 (R)", 2D) = "white" {}
		[HideInInspector] _Normal3 ("Normal 3 (A)", 2D) = "bump" {}
		[HideInInspector] _Normal2 ("Normal 2 (B)", 2D) = "bump" {}
		[HideInInspector] _Normal1 ("Normal 1 (G)", 2D) = "bump" {}
		[HideInInspector] _Normal0 ("Normal 0 (R)", 2D) = "bump" {}
		[HideInInspector] [Gamma] _Metallic0 ("Metallic 0", Range(0, 1)) = 0
		[HideInInspector] [Gamma] _Metallic1 ("Metallic 1", Range(0, 1)) = 0
		[HideInInspector] [Gamma] _Metallic2 ("Metallic 2", Range(0, 1)) = 0
		[HideInInspector] [Gamma] _Metallic3 ("Metallic 3", Range(0, 1)) = 0
		[HideInInspector] _Smoothness0 ("Smoothness 0", Range(0, 1)) = 1
		[HideInInspector] _Smoothness1 ("Smoothness 1", Range(0, 1)) = 1
		[HideInInspector] _Smoothness2 ("Smoothness 2", Range(0, 1)) = 1
		[HideInInspector] _Smoothness3 ("Smoothness 3", Range(0, 1)) = 1
		[HideInInspector] _MainTex ("BaseMap (RGB)", 2D) = "white" {}
		[HideInInspector] _Color ("Main Color", Vector) = (1,1,1,1)
	}
	SubShader {
		Tags { "Queue"="Geometry-100" "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0

		sampler2D _Control;
		sampler2D _Splat0, _Splat1, _Splat2, _Splat3;
		sampler2D _Normal0, _Normal1, _Normal2, _Normal3;

		half _Metallic0, _Metallic1, _Metallic2, _Metallic3;
		half _Smoothness0, _Smoothness1, _Smoothness2, _Smoothness3;

		struct Input {
			float2 uv_Control : TEXCOORD0;
			float2 uv_Splat0 : TEXCOORD1;
			float2 uv_Splat1 : TEXCOORD2;
			float2 uv_Splat2 : TEXCOORD3;
			float2 uv_Splat3 : TEXCOORD4;
		};

		void surf(Input IN, inout SurfaceOutputStandard o) {
			fixed4 splat_control = tex2D(_Control, IN.uv_Control);

			fixed4 col  = splat_control.r * tex2D(_Splat0, IN.uv_Splat0);
			col         += splat_control.g * tex2D(_Splat1, IN.uv_Splat1);
			col         += splat_control.b * tex2D(_Splat2, IN.uv_Splat2);
			col         += splat_control.a * tex2D(_Splat3, IN.uv_Splat3);

			fixed3 nrm  = splat_control.r * UnpackNormal(tex2D(_Normal0, IN.uv_Splat0));
			nrm         += splat_control.g * UnpackNormal(tex2D(_Normal1, IN.uv_Splat1));
			nrm         += splat_control.b * UnpackNormal(tex2D(_Normal2, IN.uv_Splat2));
			nrm         += splat_control.a * UnpackNormal(tex2D(_Normal3, IN.uv_Splat3));

			half metallic   = splat_control.r * _Metallic0
			                + splat_control.g * _Metallic1
			                + splat_control.b * _Metallic2
			                + splat_control.a * _Metallic3;

			half smoothness = splat_control.r * _Smoothness0
			                + splat_control.g * _Smoothness1
			                + splat_control.b * _Smoothness2
			                + splat_control.a * _Smoothness3;

			o.Albedo = col.rgb;
			o.Alpha = col.a;
			o.Normal = normalize(nrm);
			o.Metallic = metallic;
			o.Smoothness = smoothness;
		}
		ENDCG
	}
	Fallback "Nature/Terrain/Diffuse"
}
