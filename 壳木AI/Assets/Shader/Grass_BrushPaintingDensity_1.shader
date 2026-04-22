Shader "Grass/BrushPaintingDensity" {
	Properties {
		_MainTex ("Main Texture", 2D) = "white" {}
		_MaskTex ("Mask Texture", 2D) = "white" {}
		_BrushTex ("Brush Texture", 2D) = "white" {}
		_BrushUvOfsScale ("Brush UV Offset & Scale", Vector) = (0,0,1,1)
		_BrushOpacity ("Brush Opacity", Vector) = (1,0,0,0)
		_TargetStrength ("Target Strength", Vector) = (1,1,1,1)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				return tex2D(_MainTex, i.uv);
			}
			ENDCG
		}
	}
}
