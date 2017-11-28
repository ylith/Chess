Shader "Unlit/Highlight"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Outline Color", Color) = (1, 1, 0, 1)
	}
	SubShader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		LOD 100

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha 

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			#define SEGMENT_SIZE 27

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

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;
			float4 _Segments[64];
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				for (int j = 0; j < SEGMENT_SIZE; j++) {
					if (i.uv.x > _Segments[j][0] && i.uv.x < _Segments[j][1] && i.uv.y > _Segments[j][2] && i.uv.y < _Segments[j][3]) { //5%
						return _Color;
					}
				}

				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				return col;
			}
			ENDCG
		}
	}
}
