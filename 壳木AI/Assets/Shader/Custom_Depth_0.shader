Shader "Custom/Depth" {
	Properties {
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float depth : TEXCOORD0;
			};

			v2f vert(appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.depth = COMPUTE_DEPTH_01;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				return fixed4(i.depth, i.depth, i.depth, 1.0);
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
