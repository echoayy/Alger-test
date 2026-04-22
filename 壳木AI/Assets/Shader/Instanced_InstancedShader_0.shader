Shader "Instanced/InstancedShader" {
	Properties {
		_OriginPos ("_OriginPos", Vector) = (0,0,0,0)
		_sliceParameter ("_sliceParameter", Vector) = (0,0,0,0)
		_ColorMap ("_ColorMap", 2D) = "white" {}
		_HeightParam ("_HeightParam", Vector) = (0,0,0,0)
		_HsvGain ("_HsvGain", Vector) = (1,1,1,1)
		_DensityTex_0 ("_DensityTex_0", 2D) = "white" {}
		[NoScaleOffset] _WindTex ("_WindTex", 2D) = "white" {}
		_WindSpeed ("_WindSpeed", Float) = 2
		_WindSize ("_WindSize", Float) = 5
		Vector1_607785E7 ("normalOffset", Range(-1, 1)) = 0
		_BaseColor ("_BaseColor", Vector) = (0.07637937,0.5377358,0.0228284,0)
		_PlayerPostion ("玩家位置 (xyz), Radius (w)", Vector) = (0,0,0,0)
		_cullDistance ("Cull Distance", Range(0, 100)) = 30
		_Height ("高度", Float) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Lambert
		#pragma target 3.0
		#pragma multi_compile_instancing

		sampler2D _ColorMap;
		half4 _BaseColor;

		struct Input {
			float2 uv_ColorMap;
		};

		void surf(Input IN, inout SurfaceOutput o) {
			fixed4 col = tex2D(_ColorMap, IN.uv_ColorMap);
			o.Albedo = col.rgb * _BaseColor.rgb;
			o.Alpha = 1.0;
		}
		ENDCG
	}
	Fallback "Diffuse"
}
