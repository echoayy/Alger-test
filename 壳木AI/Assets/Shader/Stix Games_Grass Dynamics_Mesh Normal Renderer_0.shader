Shader "Stix Games/Grass Dynamics/Mesh Normal Renderer" {
	Properties {
		_InfluenceStrength ("Influence Strength", Float) = 1
		_BurnMap ("Burn Map", 2D) = "white" {}
		_BurnStrength ("Burn Strength", Range(0, 1)) = 0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			float _InfluenceStrength;

			struct appdata {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float3 normal : TEXCOORD0;
			};

			v2f vert(appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.normal = UnityObjectToWorldNormal(v.normal);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				float3 n = normalize(i.normal) * _InfluenceStrength;
				return fixed4(n * 0.5 + 0.5, 1.0);
			}
			ENDCG
		}
	}
}
