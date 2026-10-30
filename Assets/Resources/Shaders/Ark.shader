Shader "Ark/Faceted" {
 Properties {_Color("Pigment",Color)=(1,1,1,1)}
 SubShader {Tags {"RenderType"="Opaque"} Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 fixed4 _Color;
 struct appdata {float4 vertex:POSITION;float3 normal:NORMAL;};
 struct v2f {float4 pos:SV_POSITION;float shade:TEXCOORD0;};
 v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);float d=dot(UnityObjectToWorldNormal(v.normal),normalize(float3(-.6,.85,-.45)));o.shade=d>.45?1.08:d>-.05?.86:.57;return o;}
 fixed4 frag(v2f i):SV_Target {float3 c=_Color.rgb*i.shade;return fixed4(lerp(float3(.16,.12,.22),c,.93),1);}
 ENDCG }}
}
