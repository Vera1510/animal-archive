Shader "Archive/Foreground" {
 Properties { _MainTex ("Harbor",2D)="white"{} }
 SubShader {Cull Off ZWrite Off ZTest Always Pass {
 CGPROGRAM
 #pragma target 3.0
 #pragma vertex vert_img
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex;
 float4 frag(v2f_img i):SV_Target {
  float2 p=float2(i.uv.x,1-i.uv.y);float4 c=tex2D(_MainTex,i.uv);
  // Exact original pixels return above the moving cast; masked ground stays transparent.
  float wallLeft=step(.690+.86*p.x,p.y)*step(p.x,.37);
  float plantLeft=step(.515+1.16*p.x,p.y)*step(p.x,.385);
  float plantRight=step(.825,p.x)*step(.68+.66*(1-p.x),p.y);
  float wallRight=step(.895+.28*(1-p.x),p.y)*step(.81,p.x);
  float foliage=step(c.r,c.g*1.25)*step(c.r,.66)*step(c.b,.62);
  float flower=step(c.r,.67)*step(c.g,.61)*step(c.r,c.b*1.35);
  c.a=saturate(wallLeft+wallRight+(plantLeft+plantRight)*max(foliage,flower));return c;
 }
 ENDCG
 }}
}

