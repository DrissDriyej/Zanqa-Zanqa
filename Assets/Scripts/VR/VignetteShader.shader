Shader "Custom/Vignette"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Strength ("Strength", Range(0,1)) = 0.5
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _Strength;

            fixed4 frag(v2f_img i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float2 uv = i.uv * 2.0 - 1.0;
                float vignette = 1.0 - dot(uv, uv) * _Strength;
                col.rgb *= saturate(vignette);
                return col;
            }
            ENDCG
        }
    }
}   