Shader "UI/Grayscale"
{
    Properties
    {
        _Color("Tint", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags 
        {
            "Queue"="Overlay"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }

        GrabPass { "_GrabTexture" }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 grabPos : TEXCOORD1;
            };

            sampler2D _GrabTexture;
            float4 _GrabTexture_TexelSize;
            fixed4 _Color;

            v2f vert (appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.uv = IN.uv;
                OUT.grabPos = ComputeGrabScreenPos(OUT.vertex);
                return OUT;
            }

            fixed4 frag (v2f IN) : SV_Target
            {
                fixed4 c = tex2Dproj(_GrabTexture, IN.grabPos);
                float gray = dot(c.rgb, float3(0.299, 0.587, 0.114));
                c.rgb = float3(gray, gray, gray);
                return c * _Color;
            }
            ENDCG
        }
    }
}
