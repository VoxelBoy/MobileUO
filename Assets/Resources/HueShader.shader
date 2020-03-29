Shader "Unlit/HueShader"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("SrcBlend", Float) = 1.0
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("DstBlend", Float) = 10
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Blend [_SrcBlend] [_DstBlend]
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            static const int NOCOLOR = 0;
            static const int COLOR = 1;
            static const int PARTIAL_COLOR = 2;
            static const int HUE_TEXT_NO_BLACK = 3;
            static const int HUE_TEXT = 4;
            static const int LAND = 6;
            static const int LAND_COLOR = 7;
            static const int SPECTRAL = 10;
            static const int SHADOW = 12;
            static const int LIGHTS = 13;
            static const int COLOR_SWAP = 32;

            static const int HUES_DELTA = 3000;
            static const float3 LIGHT_DIRECTION = float3(-1.0f, -1.0f, .5f);
            static const float3 VEC3_ZERO = float3(0, 0, 0);

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 Normal : NORMAL;
                float4 pos : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Hue;
            float _uvMirrorX;
            float _Debug;
            float _Scissor;
            float4 _ScissorRect;

            sampler2D _HueTex1;
            sampler2D _HueTex2;

            float3 get_rgb(float red, float hue, bool swap)
            {
                if (hue < HUES_DELTA)
                {
                    if (swap)
                        hue += HUES_DELTA;
                    //NOTE: We invert the y coordinate to read from the proper place in the hue texture
                    return tex2D(_HueTex1, float2(red, 1 - hue / 6000.0f)).rgb;
                }
                //NOTE: We invert the y coordinate to read from the proper place in the hue texture
                return tex2D(_HueTex2, float2(red, 1 - (hue - 3000.0f) / 3000.0f)).rgb;
            }

            float3 get_light(float3 norm)
            {
                float3 light = normalize(LIGHT_DIRECTION);
                float3 normal = normalize(norm);
                return max((dot(normal, light) + 0.5f), 0.0f);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.Normal = v.normal;
                return o;
            }

            fixed4 frag (v2f IN) : SV_Target
            {
                if(_Scissor == 1)
                {
                    #if UNITY_UV_STARTS_AT_TOP == false
                    IN.pos.y = _ScreenParams.y - IN.pos.y;
                    #endif
                    if(IN.pos.x < _ScissorRect.x || IN.pos.x > _ScissorRect.z || IN.pos.y < _ScissorRect.y || IN.pos.y > _ScissorRect.w)
                    {
                        discard;
                    }
                }

                if(_uvMirrorX == 1)
                {
                    IN.uv.x = 1 - IN.uv.x;
                }

                float4 color = tex2D(_MainTex, IN.uv);

                if (color.a == 0.0f)
                    discard;

                int mode = int(_Hue.y);
                float alpha = 1 - _Hue.z;

                if (mode > NOCOLOR)
                {
                    bool swap = false;
                    if (mode >= COLOR_SWAP)
                    {
                        mode -= COLOR_SWAP;
                        swap = true;
                    }

                    if (mode == COLOR || (mode == PARTIAL_COLOR && color.r == color.g && color.r == color.b))
                    {
                        color.rgb = get_rgb(color.r, _Hue.x, swap);
                    }
                    else if (mode > 5)
                    {
                        if (mode > 9)
                        {
                            float red = color.r;

                            if (mode > 10)
                            {
                                if (mode > 11)
                                {
                                    if (mode > 12)
                                    {
                                        if (_Hue.x != 0.0f)
                                        {
                                            color.rgb *= get_rgb(color.r, _Hue.x, swap);
                                        }
                                        return color * alpha;
                                    }

                                    red = 0.6f;
                                }
                                else
                                {
                                    red *= 0.5f;
                                }
                            }
                            else
                            {
                                red *= 1.5f;
                            }

                            alpha = 1 - red;
                            color.rgb = VEC3_ZERO;
                        }
                        else
                        {
                            float3 norm = get_light(IN.Normal);

                            if (mode > 6)
                            {
                                color.rgb = get_rgb(color.r, _Hue.x, swap) * norm;
                            }
                            else
                            {
                                color.rgb *= norm;
                            }
                        }
                    }
                    else if (mode == 4 || (mode == 3 && (color.r > 0.08 || color.g > 0.08 || color.b > 0.08)) || (mode == 5 && color.r > 0.08))
                    {
                        color.rgb = get_rgb(color.r + 90, _Hue.x, swap);
                    }
                }

                return color * alpha;
            }
            ENDCG
        }
    }
}
