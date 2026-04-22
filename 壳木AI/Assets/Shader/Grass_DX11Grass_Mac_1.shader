Shader "Grass/DX11Grass_Mac" {
	Properties {
		[Enum(Full,1,Half,2,Quarter,4)] _Resolution ("Resolution", Float) = 1
		_DrawTerrainColor ("Terrain Color", Float) = 1
		_EdgeLength ("Density Falloff", Range(3, 50)) = 8
		_MaxTessellation ("Max Density", Range(1, 6)) = 1
		_LODStart ("LOD Start", Float) = 20
		_LODEnd ("LOD End", Float) = 100
		_LODMax ("Max LOD", Range(1, 5)) = 5
		_GrassFading ("Grass Fade(Start,End,Target,Power)", Vector) = (20,100,0,1)
		_GrassWidening ("Grass Widen(Start,End,Target,Power)", Vector) = (20,100,1.3,1)
		_GrassHeighening ("Grass Heighen(Start,End,Target,Power)", Vector) = (20,100,0.5,1)
		_GrassShadowDistance ("Grass Shadow Distance", Range(0, 100)) = 20
		_GrassShadowClampTo1 ("Grass Shadow Clamp To 1", Range(0, 1)) = 0.81
		_GrassShadowCosClampTo1 ("Grass Shadow Cos Clamp To 1", Range(0, 1)) = 0.7
		_Disorder ("Disorder", Float) = 0.3
		_FadeOrder ("Fade Order", Range(0, 1)) = 1
		_GrassBottomColor ("Grass Bottom Color", Vector) = (0.35,0.35,0.35,1)
		_TextureCutoff ("Texture Cutoff", Range(0, 1)) = 0.1
		_HeightCutoff ("Height Cutoff(Multiplier)", Range(0, 1)) = 0.5
		_ColorMap ("Color Texture (RGB), Height(A)", 2D) = "white" {}
		_Displacement ("Displacement Texture (RG)", 2D) = "bump" {}
		_DensityTex_0 ("Grass Density_0 1(R) 2(G) 3(B) 4(A)", 2D) = "black" {}
		_DensityTex_1 ("Grass Density_1 1(R) 2(G) 3(B) 4(A)", 2D) = "black" {}
		_GrassAllTexAtlas ("Grass Texture Atlas", 2D) = "white" {}
		_WindParams ("Wind WaveStrength(X), WaveSpeed(Y), RippleStrength(Z), RippleSpeed(W)", Vector) = (0.3,1.2,0.15,1.3)
		_WindDirRotation ("Wind Dir(X,Z),Rot(W)", Vector) = (0,0,0,0)
		_GrassTex00 ("Grass Texture", 2D) = "white" {}
		_Softness00 ("Softness", Range(0, 1)) = 0.5
		_Width00 ("Width", Float) = 0.1
		_MinHeight00 ("Min Height", Float) = 0.2
		_MaxHeight00 ("Max Height", Float) = 1.5
		[Toggle] _DrawTerrain00 ("Draw Terrain", Float) = 1
		_GrassTex01 ("Grass Texture", 2D) = "white" {}
		_Softness01 ("Softness", Range(0, 1)) = 0.5
		_Width01 ("Width", Float) = 0.1
		_MinHeight01 ("Min Height", Float) = 0.2
		_MaxHeight01 ("Max Height", Float) = 1.5
		_DrawTerrain01 ("Draw Terrain", Float) = 1
		_GrassTex02 ("Grass Texture", 2D) = "white" {}
		_Softness02 ("Softness", Range(0, 1)) = 0.5
		_Width02 ("Width", Float) = 0.1
		_MinHeight02 ("Min Height", Float) = 0.2
		_MaxHeight02 ("Max Height", Float) = 1.5
		_DrawTerrain02 ("Draw Terrain", Float) = 1
		_GrassTex03 ("Grass Texture", 2D) = "white" {}
		_Softness03 ("Softness", Range(0, 1)) = 0.5
		_Width03 ("Width", Float) = 0.1
		_MinHeight03 ("Min Height", Float) = 0.2
		_MaxHeight03 ("Max Height", Float) = 1.5
		_DrawTerrain03 ("Draw Terrain", Float) = 0
		_GrassTex04 ("Grass Texture", 2D) = "white" {}
		_Softness04 ("Softness", Range(0, 1)) = 0.5
		_Width04 ("Width", Float) = 0.1
		_MinHeight04 ("Min Height", Float) = 0.2
		_MaxHeight04 ("Max Height", Float) = 1.5
		_DrawTerrain04 ("Draw Terrain", Float) = 0
		_GrassTex05 ("Grass Texture", 2D) = "white" {}
		_Softness05 ("Softness", Range(0, 1)) = 0.5
		_Width05 ("Width", Float) = 0.1
		_MinHeight05 ("Min Height", Float) = 0.2
		_MaxHeight05 ("Max Height", Float) = 1.5
		_DrawTerrain05 ("Draw Terrain", Float) = 0
		_GrassTex06 ("Grass Texture", 2D) = "white" {}
		_Softness06 ("Softness", Range(0, 1)) = 0.5
		_Width06 ("Width", Float) = 0.1
		_MinHeight06 ("Min Height", Float) = 0.2
		_MaxHeight06 ("Max Height", Float) = 1.5
		_DrawTerrain06 ("Draw Terrain", Float) = 0
		_GrassTex07 ("Grass Texture", 2D) = "white" {}
		_Softness07 ("Softness", Range(0, 1)) = 0.5
		_Width07 ("Width", Float) = 0.1
		_MinHeight07 ("Min Height", Float) = 0.2
		_MaxHeight07 ("Max Height", Float) = 1.5
		_DrawTerrain07 ("Draw Terrain", Float) = 1
		_GT00_Mask ("Grass Texture Mask 0", 2D) = "white" {}
		_GT01_Mask ("Grass Texture Mask 1", 2D) = "white" {}
		_GT02_Mask ("Grass Texture Mask 2", 2D) = "white" {}
		_GT03_Mask ("Grass Texture Mask 3", 2D) = "white" {}
		_GT04_Mask ("Grass Texture Mask 4", 2D) = "white" {}
		_GT05_Mask ("Grass Texture Mask 5", 2D) = "white" {}
		_GT06_Mask ("Grass Texture Mask 6", 2D) = "white" {}
		_GT07_Mask ("Grass Texture Mask 7", 2D) = "white" {}
	}
	SubShader {
		Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" }
		LOD 200
		Cull Off

		CGPROGRAM
		#pragma surface surf Lambert alphatest:_TextureCutoff addshadow
		#pragma target 3.0

		sampler2D _ColorMap;
		half4 _GrassBottomColor;

		struct Input {
			float2 uv_ColorMap;
			float4 color : COLOR;
		};

		void surf(Input IN, inout SurfaceOutput o) {
			fixed4 col = tex2D(_ColorMap, IN.uv_ColorMap);
			o.Albedo = col.rgb * lerp(_GrassBottomColor.rgb, float3(1,1,1), IN.color.r);
			o.Alpha = col.a;
		}
		ENDCG
	}
	Fallback "Nature/Tree Creator Leaves"
}
