Shader "UI/SmokeDistort"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("MainTex", 2D) = "white" {}
        _NoiseTex ("NoiseTex", 2D) = "gray" {}
        _Intensity ("Intensity", Range(0,0.05)) = 0.01
        _Speed ("Speed", Vector) = (0.1,0.1,0,0)
        _Tiling ("Tiling", Vector) = (1,1,0,0)
        _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;
            fixed4 _Color;
            float4 _ClipRect;
            float _UseUIAlphaClip;
            float _Intensity;
            float4 _Speed;
            float4 _Tiling;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 nUV = i.uv * _Tiling.xy + _Time.y * _Speed.xy;
                float2 noise = tex2D(_NoiseTex, nUV).rg * 2.0 - 1.0;
                float2 duv = noise * _Intensity;
                float2 uv2 = i.uv + duv;

                fixed4 baseCol = tex2D(_MainTex, uv2) * i.color;
                fixed a0 = tex2D(_MainTex, i.uv).a;
                baseCol.a *= a0;

                baseCol.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                if (_UseUIAlphaClip > 0.5) clip(baseCol.a - 0.001);

                return baseCol;
            }
            ENDHLSL
        }
    }
    FallBack "UI/Default"
}
