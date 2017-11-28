// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Checkboard"
{
	Properties
	{
		ColorA ("Color 1", Color) = (1, 1, 1, 1)
		ColorB ("Color 2", Color) = (0, 0, 0, 1)
		x ("x", int) = 1
		y ("y", int) = 1
	}
	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			float4 ColorA;
			float4 ColorB;
			int x;
			int y;
			
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos (v.vertex);
				o.uv = v.uv;
				return o;
			}


			float4 frag(v2f i) : SV_Target {
				if ((i.uv.x * x) % 2 < 1 && (i.uv.y * y) % 2 > 1 || (i.uv.x * x) % 2 > 1 && (i.uv.y * y) % 2 < 1) {
					return ColorB;
				}

				return ColorA;
			}
			ENDCG
		}
	}
}
