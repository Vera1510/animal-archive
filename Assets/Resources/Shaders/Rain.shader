Shader "Ark/Rain" {SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent"}Pass {ZWrite Off Blend SrcAlpha OneMinusSrcAlpha Cull Off CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct v2f {float4 pos:SV_POSITION;};
 v2f vert(appdata_base v){v2f o;float4 p=v.vertex;p.y=frac((p.y-_Time.y*7)/22)*22-3;p.x+=sin(_Time.y*.2)*.5;o.pos=UnityObjectToClipPos(p);return o;}
 fixed4 frag(v2f i):SV_Target{return fixed4(.75,.80,.79,.3);}
 ENDCG}}}
