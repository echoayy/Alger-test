Shader "CCC/Cartoon/Water" {
	Properties {
		_NoiseMap ("Noise Map", 2D) = "white" {}
		_DepthColor ("Depth Color", Vector) = (1,1,1,1)
		_FoamColor ("Foam Color", Vector) = (1,1,1,1)
		_ReflectionColor ("Reflection Color", Vector) = (1,1,1,1)
		_FresnelColor ("Fresnel Color", Vector) = (1,1,1,1)
		_Tile ("Tile Size", Range(0.005, 0.1)) = 0.05
		_RefractionAmt ("Refraction Amount", Range(0, 1)) = 1
		_SunLightDir ("Sun light direction", Vector) = (0,0.1,-0.5,0)
		_SunSpecular ("Sun specular color", Vector) = (1,1,1,1)
		_MoonLightDir ("Moon light direction", Vector) = (0,0.1,-0.5,0)
		_MoonSpecular ("Moon specular color", Vector) = (1,1,1,1)
		_Shininess ("Shininess", Range(2, 1200)) = 1200
		_DepthScale ("DepthScale", Range(0, 5)) = 1
		_AlphaOffset ("Alpha Offset", Range(-1, 1)) = 0
		_FoamSpeed ("Foam Speed", Range(-1, 5)) = 0
		_FoamOffset ("Foam Offset", Range(-100, 100)) = 0
		_UseFixOffset ("Fix Offset", Range(0, 1)) = 0
		_FoamStrength ("Foam Strength", Range(0, 1)) = 1
	}
	SubShader {
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard alpha:fade vertex:vert
		#pragma target 3.0

		sampler2D _NoiseMap;
		half4 _DepthColor;
		half4 _FresnelColor;
		half _Tile;
		half _Shininess;
		half _FoamSpeed;

		struct Input {
			float3 worldPos;
			float3 viewDir;
			float3 worldViewDir;
		};

		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
			o.worldViewDir = normalize(_WorldSpaceCameraPos - worldPos);
		}

		void surf(Input IN, inout SurfaceOutputStandard o) {
			float2 uv = IN.worldPos.xz * _Tile;
			uv += _Time.y * _FoamSpeed * 0.01;

			fixed4 noise = tex2D(_NoiseMap, uv);
			fixed4 noise2 = tex2D(_NoiseMap, uv * 0.7 + float2(0.3, 0.1) + _Time.y * _FoamSpeed * 0.005);

			// Fresnel using world-space view direction and world up normal
			half NdotV = saturate(dot(IN.worldViewDir, float3(0, 1, 0)));
			half fresnel = pow(1.0 - NdotV, 3.0);
			fresnel = fresnel * 0.6; // clamp max fresnel effect

			half3 col = lerp(_DepthColor.rgb, _FresnelColor.rgb, fresnel);
			col += (noise.r * noise2.r) * 0.1;

			o.Albedo = col;
			o.Smoothness = 1.0 - (1.0 / _Shininess);
			o.Metallic = 0.0;
			o.Normal = float3((noise.rg - 0.5) * 0.3, 1.0);
			o.Alpha = saturate(0.8 + fresnel * 0.1);
		}
		ENDCG
	}
	Fallback "Transparent/Diffuse"
}
