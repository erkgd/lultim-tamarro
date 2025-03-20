// CircularMaskShader.shader
Shader "UI/CircularMask"
{
    Properties
    {
        _Radius ("Radius", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
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
            
            float _Radius;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Centrar les coordenades UV
                float2 centeredUV = i.uv * 2.0 - 1.0;
                
                // Distància al centre
                float dist = length(centeredUV);
                
                // Màscara circular amb vores dures
                // 1.0 (transparent) dins del radi, 0.0 (opac) fora
                float circle = step(dist, _Radius);
                
                // Retorn el color negre amb transparència al centre
                return fixed4(0, 0, 0, 1.0 - circle);
            }
            ENDCG
        }
    }
}