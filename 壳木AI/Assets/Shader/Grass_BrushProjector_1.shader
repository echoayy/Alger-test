Shader "Grass/BrushProjector" {
	Properties {
		_Color ("Main Color", Vector) = (1,1,1,1)
		_ShadowTex ("Brush", 2D) = "white" {}
	}
	SubShader {
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		LOD 200
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		Offset -1, -1

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _ShadowTex;
			fixed4 _Color;
			float4x4 unity_Projector;

			struct appdata {
				float4 vertex : POSITION;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float4 uvShadow : TEXCOORD0;
			};

			v2f vert(appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uvShadow = mul(unity_Projector, v.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				fixed4 tex = tex2Dproj(_ShadowTex, UNITY_PROJ_COORD(i.uvShadow));
				return tex * _Color;
			}
			ENDCG
		}
	}
}
