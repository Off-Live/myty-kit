Shader "Sprites/SoftLight"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
	}

		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}
			Cull Off
			Lighting Off
			ZWrite Off
			Blend One OneMinusSrcAlpha

			GrabPass
			{
				"_BGTex"
			}

			Pass
			{

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile _ PIXELSNAP_ON
				#include "UnityCG.cginc"

				struct appdata_t
				{
					float4 vertex   : POSITION;
					float4 color    : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f
				{
					float4 vertex   : SV_POSITION;
					fixed4 color : COLOR;

					float4 grabPos : TEXCOORD0;
					float2 texcoord : TEXCOORD1;
				};

				fixed4 _Color;

				v2f vert(appdata_t IN)
				{
					v2f OUT;
					OUT.vertex = UnityObjectToClipPos(IN.vertex);
					OUT.texcoord = IN.texcoord;

					OUT.color = IN.color * _Color;
					#ifdef PIXELSNAP_ON
					OUT.vertex = UnityPixelSnap(OUT.vertex);
					#endif
					OUT.grabPos = ComputeGrabScreenPos(OUT.vertex);

					return OUT;
				}

				sampler2D _MainTex;
				sampler2D _AlphaTex;
				sampler2D _BGTex;
				sampler2D _emtex;


				float _AlphaSplitEnabled;

				fixed4 SampleSpriteTexture(float2 uv)
				{
					fixed4 color = tex2D(_MainTex, uv);

	#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
					if (_AlphaSplitEnabled)
						color.a = tex2D(_AlphaTex, uv).r;
	#endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED

					return color;
				}

				//https://github.com/penandlim/JL-s-Unity-Blend-Modes/
				float softLightComp(float s, float d)
				{
					return (s < 0.5) ? d - (1.0 - 2.0 * s) * d * (1.0 - d)
						: (d < 0.25) ? d + (2.0 * s - 1.0) * d * ((16.0 * d - 12.0) * d + 3.0)
						: d + (2.0 * s - 1.0) * (sqrt(d) - d);
				}

				fixed4 frag(v2f IN) : SV_Target
				{
					fixed4 bg = tex2D(_BGTex, IN.grabPos.xy / IN.grabPos.w);
					fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
					
					c.r = softLightComp(bg.r, c.r);
					c.g = softLightComp(bg.g, c.g);
					c.b = softLightComp(bg.b, c.b);

					c.rgb *= c.a;
					return  c;
				}
			ENDCG
			}


		}
}