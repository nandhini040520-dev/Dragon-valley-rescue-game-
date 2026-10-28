Shader "DragonValley/StorybookPaint" {
 Properties { _Color("Paint color",Color)=(1,1,1,1) _Glossiness("Soft sheen",Range(0,1))=0.17 _Metallic("Metal",Range(0,1))=0 _EmissionColor("Glow",Color)=(0,0,0,0) }
 SubShader {
  Tags { "RenderType"="Opaque" } LOD 200
  CGPROGRAM
  #pragma surface surf Standard fullforwardshadows
  #pragma target 3.0
  fixed4 _Color; half _Glossiness; half _Metallic; fixed4 _EmissionColor;
  struct Input { float3 worldPos; };
  float hash(float3 p) { return frac(sin(dot(p,float3(17.13,91.71,43.27)))*43758.5453); }
  float grain(float3 p) { float3 i=floor(p),f=frac(p); f=f*f*(3-2*f); return lerp(lerp(lerp(hash(i),hash(i+float3(1,0,0)),f.x),lerp(hash(i+float3(0,1,0)),hash(i+float3(1,1,0)),f.x),f.y),lerp(lerp(hash(i+float3(0,0,1)),hash(i+float3(1,0,1)),f.x),lerp(hash(i+float3(0,1,1)),hash(i+1),f.x),f.y),f.z); }
  void surf(Input IN,inout SurfaceOutputStandard o) {
   float brush=grain(IN.worldPos*5)*.08+grain(IN.worldPos*32)*.025;
   o.Albedo=_Color.rgb*(.93+brush); o.Metallic=_Metallic; o.Smoothness=_Glossiness; o.Emission=_EmissionColor.rgb; o.Alpha=1;
  }
  ENDCG
 }
 Fallback "Standard"
}
