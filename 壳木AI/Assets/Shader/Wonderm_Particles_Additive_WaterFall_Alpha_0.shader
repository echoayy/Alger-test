Shader "Wonderm/Particles/Additive_WaterFall_Alpha" {
	Properties {
		_TintColor ("Tint Color", Vector) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01, 3)) = 1
		_MixLight ("MixLight", Range(0, 1)) = 1
	}
	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		LOD 100
		Blend SrcAlpha One
		ZWrite Off
		Cull Off

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_particles
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _TintColor;

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
			};

			v2f vert(appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				fixed4 col = 2.0 * i.color * _TintColor * tex2D(_MainTex, i.uv);
				return col;
			}
			ENDCG
		}
	}
	Fallback "Particles/Additive"
}
