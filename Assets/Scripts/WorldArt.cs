using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace DragonValley
{
    public static class WorldArt
    {
        static readonly Dictionary<string,Material> mats=new Dictionary<string,Material>();
        public static Color Ink=new Color(.055f,.14f,.2f), Mint=new Color(.24f,.94f,.72f), Gold=new Color(1,.69f,.24f), Pink=new Color(.97f,.37f,.61f), Cyan=new Color(.21f,.79f,1);
        public static Material Mat(Color color,bool glow=false)
        {
            string key=color.ToString()+glow;
            if(mats.TryGetValue(key,out var existing) && existing) return existing;
            var template=Resources.Load<Material>(glow?"GlowSurface":"PlainSurface");
            var m=template?new Material(template):new Material(Shader.Find("Standard"));
            m.color=color; m.SetFloat("_Glossiness",.22f); m.enableInstancing=true;
            if(glow) { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor",color*.65f); }
            mats[key]=m; return m;
        }
        public static GameObject Shape(string name,PrimitiveType type,Transform parent,Vector3 p,Vector3 s,Color color,bool solid=true,bool glow=false)
        {
            var go=GameObject.CreatePrimitive(type); go.name=name; go.transform.SetParent(parent,false); go.transform.localPosition=p; go.transform.localScale=s;
            go.GetComponent<Renderer>().sharedMaterial=Mat(color,glow);
            if(!solid) { var c=go.GetComponent<Collider>(); if(Application.isPlaying) Object.Destroy(c); else Object.DestroyImmediate(c); }
            return go;
        }
        public static GameObject Box(string name,Transform parent,Vector3 p,Vector3 s,Color c,bool solid=true,bool glow=false)=>Shape(name,PrimitiveType.Cube,parent,p,s,c,solid,glow);
        public static GameObject Ball(string name,Transform parent,Vector3 p,Vector3 s,Color c,bool glow=false)=>Shape(name,PrimitiveType.Sphere,parent,p,s,c,false,glow);
        public static GameObject Cylinder(string name,Transform parent,Vector3 p,Vector3 s,Color c,bool solid=true)=>Shape(name,PrimitiveType.Cylinder,parent,p,s,c,solid);
        public static void Label(Transform parent,string text,Vector3 p,float size=.26f,Color? color=null)
        {
            var go=new GameObject(text); go.transform.SetParent(parent,false); go.transform.localPosition=p;
            var tm=go.AddComponent<TextMesh>(); tm.text=text; tm.fontSize=48; tm.characterSize=size*.4f; tm.anchor=TextAnchor.MiddleCenter; tm.alignment=TextAlignment.Center; tm.color=color??Color.white;
            go.transform.rotation=Quaternion.identity;
            var font=Resources.Load<Font>("ExpeditionSans"); if(font) { tm.font=font; go.GetComponent<MeshRenderer>().sharedMaterial=font.material; }
            go.AddComponent<WorldLabel>();
        }
        public static GameObject Gem(Transform parent,Vector3 p,Color color,float size=1)
        {
            var g=Box("Energy crystal",parent,p,Vector3.one*size,color,false,true); g.transform.localRotation=Quaternion.Euler(20,0,45); return g;
        }
        public static void Tree(Transform parent,Vector3 p,float scale,Color color)
        {
            StorybookEnvironment.Tree(parent,p,scale,color);
        }
        public static GameObject Cone(Transform parent,Vector3 p,float radius,float height,Color color,int sides=7)
        {
            var go=new GameObject("Faceted canopy"); go.transform.SetParent(parent,false); go.transform.localPosition=p;
            var vertices=new List<Vector3>(); var tris=new List<int>();
            for(int i=0;i<sides;i++) {
                float a=i*Mathf.PI*2/sides,b=(i+1)*Mathf.PI*2/sides; int v=vertices.Count;
                vertices.Add(new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius)); vertices.Add(Vector3.up*height); vertices.Add(new Vector3(Mathf.Cos(b)*radius,0,Mathf.Sin(b)*radius));
                tris.Add(v); tris.Add(v+1); tris.Add(v+2);
                vertices.Add(Vector3.zero); vertices.Add(new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius)); vertices.Add(new Vector3(Mathf.Cos(b)*radius,0,Mathf.Sin(b)*radius));
                tris.Add(v+3); tris.Add(v+4); tris.Add(v+5);
            }
            var mesh=new Mesh(); mesh.SetVertices(vertices); mesh.SetTriangles(tris,0); mesh.RecalculateNormals();
            go.AddComponent<MeshFilter>().sharedMesh=mesh; go.AddComponent<MeshRenderer>().sharedMaterial=Mat(color); return go;
        }
        public static void Flower(Transform parent,Vector3 p,Color color,float scale=1)
        {
            Cylinder("Stem",parent,p+Vector3.up*.3f*scale,new Vector3(.06f,.3f,.06f)*scale,new Color(.21f,.5f,.29f),false);
            for(int i=0;i<5;i++) { float a=i*Mathf.PI*2/5; Ball("Petal",parent,p+new Vector3(Mathf.Cos(a)*.2f,.62f,Mathf.Sin(a)*.2f)*scale,new Vector3(.34f,.13f,.34f)*scale,color); }
            Ball("Pollen",parent,p+Vector3.up*.68f*scale,Vector3.one*.16f*scale,Gold,true);
        }
    }
}
