/*
	Author: Alberto Mellado Cruz
	Date: 09/11/2017

	Comments:
	This is just a test that would depend on the 3D Model used.
	Vertex animations would allow the use of GPU Instancing,
	enabling the use of a dense amount of animated fish.
	The code may not be optimized but it was just a test
*/

Shader "Custom/FishAnimation" {
	Properties{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Color("Main Color", Color) = (1,1,1,1)
		_SpeedX("SpeedX", Range(0, 10)) = 1
		_FreqX("FrequencyX", Range(0, 10)) = 1
		_AmpX("AmplitudeX", Range(0, 0.2)) = 1
		_SpeedY("SpeedY", Range(0, 10)) = 1
		_FreqY("FrequencyY", Range(0, 10)) = 1
		_AmpY("AmplitudeY", Range(0, 0.2)) = 1
		_SpeedZ("SpeedZ", Range(0, 10)) = 1
		_FreqZ("FrequencyZ", Range(0, 10)) = 1
		_AmpZ("AmplitudeZ", Range(0,  2)) = 1
		_HeadLimit("HeadLimit", Range(-2,  2)) = 0.05

	}
		SubShader{
		Tags{ "RenderType" = "Transparent" }
		Cull off

		Pass{

		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			// X AXIS

			float _SpeedX;
			float _FreqX;
			float _AmpX;

			// Y AXIS

			float _SpeedY;
			float _FreqY;
			float _AmpY;

			// Z AXIS

			float _SpeedZ;
			float _FreqZ;
			float _AmpZ;

			// Head Limit (Head wont shake so much)

			float _HeadLimit;

			// Tint Color

			fixed4 _Color;

			v2f vert(appdata_base v)
			{
				v2f o;

				//Z AXIS

				v.vertex.z += sin((v.vertex.z + _Time.y * _SpeedX) * _FreqX) * _AmpX;

				//Y AXIS

				v.vertex.y += sin((v.vertex.z + _Time.y * _SpeedY) * _FreqY) * _AmpY;

				//X AXIS

				if (v.vertex.z > _HeadLimit)
				{
					v.vertex.x += sin((0.05 + _Time.y * _SpeedZ) * _FreqZ) * _AmpZ * _HeadLimit;
				}
				else
				{
					v.vertex.x += sin((v.vertex.z + _Time.y * _SpeedZ) * _FreqZ) * _AmpZ * v.vertex.z;
				}


				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;

			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);

				col.rgb = col.rgb * _Color;
				return col;
			}

		ENDCG

	}
		}
			FallBack "Diffuse"
}
