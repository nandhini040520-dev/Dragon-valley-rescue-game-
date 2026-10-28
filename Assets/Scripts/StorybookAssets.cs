using System.Collections.Generic;
using UnityEngine;
using static DragonValley.WorldArt;
namespace DragonValley
{
    // Hand-authored component models. The editor exporter saves these as reusable mesh assets and prefabs.
    public static class StorybookAssets
    {
        public static readonly Color Ivory=new Color(.88f,.83f,.66f), Brass=new Color(.64f,.39f,.12f), Leather=new Color(.3f,.16f,.09f), Jade=new Color(.19f,.47f,.39f), Blue=new Color(.16f,.35f,.5f);
        static Mesh orb;
        public static Transform Root(Transform parent,string name,Vector3 p)
        { var t=new GameObject(name).transform; t.SetParent(parent,false); t.localPosition=p; return t; }
        public static GameObject Oval(Transform p,string name,Vector3 pos,Vector3 scale,Color c,bool emission=false)
        {
            if(!orb) {
                var vertices=new List<Vector3>(); var indices=new List<int>(); var uv=new List<Vector2>();
                const int rings=10,sides=16;
                for(int y=0;y<=rings;y++) for(int x=0;x<=sides;x++) {
                    float a=x*Mathf.PI*2/sides,b=y*Mathf.PI/rings;
                    vertices.Add(new Vector3(Mathf.Sin(b)*Mathf.Cos(a),Mathf.Cos(b),Mathf.Sin(b)*Mathf.Sin(a))*.5f); uv.Add(new Vector2((float)x/sides,(float)y/rings));
                }
                for(int y=0;y<rings;y++) for(int x=0;x<sides;x++) { int k=y*(sides+1)+x; indices.AddRange(new[]{k,k+1,k+sides+1,k+1,k+sides+2,k+sides+1}); }
                orb=new Mesh { name="Storybook rounded shell" }; orb.SetVertices(vertices); orb.SetTriangles(indices,0); orb.SetUVs(0,uv); orb.RecalculateNormals();
            }
            return MeshPart(p,name,pos,scale,orb,c,emission);
        }
        public static GameObject MeshPart(Transform parent,string name,Vector3 pos,Vector3 scale,Mesh mesh,Color c,bool glow=false)
        {
            var root=Root(parent,name,pos); root.localScale=scale; root.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh; root.gameObject.AddComponent<MeshRenderer>().sharedMaterial=Mat(c,glow); return root.gameObject;
        }
        public static void Rod(Transform p,string name,Vector3 a,Vector3 b,float radius,Color c)
        {
            var go=Cylinder(name,p,(a+b)*.5f,new Vector3(radius*2,(a-b).magnitude*.5f,radius*2),c,false); go.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);
        }
        public static void Curve(Transform p,string name,Vector3[] points,float radius,Color c)
        { for(int i=1;i<points.Length;i++) Rod(p,name,points[i-1],points[i],radius,c); }
        public static GameObject Ring(Transform p,string name,Vector3 pos,float radius,float tube,Color color,Quaternion? rotation=null,int count=24)
        {
            var vertices=new List<Vector3>(); var tris=new List<int>(); const int slices=6;
            for(int i=0;i<=count;i++) for(int j=0;j<=slices;j++) {
                float a=i*Mathf.PI*2/count,b=j*Mathf.PI*2/slices;
                vertices.Add(new Vector3((radius+tube*Mathf.Cos(b))*Mathf.Cos(a),(radius+tube*Mathf.Cos(b))*Mathf.Sin(a),tube*Mathf.Sin(b)));
            }
            for(int i=0;i<count;i++) for(int j=0;j<slices;j++) { int k=i*(slices+1)+j; tris.AddRange(new[]{k,k+slices+1,k+1,k+1,k+slices+1,k+slices+2}); }
            var mesh=new Mesh {name=name}; mesh.SetVertices(vertices); mesh.SetTriangles(tris,0); mesh.RecalculateNormals();
            var go=MeshPart(p,name,pos,Vector3.one,mesh,color); go.transform.localRotation=rotation??Quaternion.identity; return go;
        }
        public static GameObject Leaf(Transform parent,string name,Vector3 p,Vector3 scale,Color c,Quaternion rotation)
        {
            var v=new[]{new Vector3(0,-.5f,0),new Vector3(-.35f,-.18f,0),new Vector3(-.28f,.22f,0),new Vector3(0,.65f,0),new Vector3(.28f,.22f,0),new Vector3(.35f,-.18f,0),new Vector3(0,0,.13f)};
            var tri=new List<int>(); for(int i=0;i<6;i++) { tri.AddRange(new[]{6,i,(i+1)%6}); }
            var mesh=DoubleSided(name,v,tri.ToArray());
            var go=MeshPart(parent,name,p,scale,mesh,c); go.transform.localRotation=rotation; return go;
        }
        public static GameObject Robot(Transform parent,RobotKind kind) => FunobotzModels.Create(parent,kind);
        public static Transform Explorer(Transform parent)
        {
            var t=Root(parent,"Explorer Engineer • storybook model",Vector3.zero);
            Color skin=new Color(.59f,.34f,.2f), cloth=new Color(.15f,.36f,.33f), hair=new Color(.11f,.075f,.055f);
            Oval(t,"Tailored tunic",new Vector3(0,1,0),new Vector3(.63f,.73f,.4f),cloth);
            for(int s=-1;s<=1;s+=2) {
                var leg=Root(t,s<0?"Leg L":"Leg R",new Vector3(s*.16f,.71f,0));
                Rod(leg,"Canvas trousers",Vector3.zero,new Vector3(0,-.4f,0),.108f,Ivory);
                Oval(leg,"Leather boot",new Vector3(0,-.48f,.08f),new Vector3(.25f,.4f,.38f),Leather);
                Box("Boot sole",leg,new Vector3(0,-.66f,.07f),new Vector3(.25f,.035f,.39f),Ink,false);
                for(int j=0;j<2;j++) Box("Boot strap",leg,new Vector3(0,-.35f-j*.13f,.15f),new Vector3(.26f,.055f,.04f),Brass,false);
                var arm=Root(t,s<0?"Arm L":"Arm R",new Vector3(s*.32f,1.19f,0));
                Rod(arm,"Tunic sleeve",Vector3.zero,new Vector3(s*.085f,-.28f,0),.1f,cloth);
                Rod(arm,"Shirt cuff",new Vector3(s*.08f,-.25f,0),new Vector3(s*.085f,-.34f,0),.11f,Ivory);
                Oval(arm,"Leather glove",new Vector3(s*.09f,-.41f,.02f),new Vector3(.16f,.2f,.17f),Leather);
            }
            Oval(t,"Head",new Vector3(0,1.54f,.015f),new Vector3(.51f,.59f,.46f),skin);
            for(int s=-1;s<=1;s+=2) {
                Oval(t,"Ear",new Vector3(s*.255f,1.53f,.015f),new Vector3(.1f,.15f,.075f),skin);
                Oval(t,"Eye white",new Vector3(s*.11f,1.59f,.22f),new Vector3(.1f,.11f,.03f),Ivory);
                Oval(t,"Warm brown eye",new Vector3(s*.11f,1.585f,.24f),new Vector3(.062f,.073f,.025f),hair);
                Oval(t,"Eye highlight",new Vector3(s*.11f-.015f,1.606f,.256f),Vector3.one*.019f,Color.white);
                Rod(t,"Eyebrow",new Vector3(s*.06f,1.665f,.22f),new Vector3(s*.16f,1.665f,.2f),.019f,hair);
            }
            Oval(t,"Nose",new Vector3(0,1.51f,.255f),new Vector3(.07f,.08f,.08f),skin);
            Curve(t,"Smile",new[]{new Vector3(-.08f,1.447f,.21f),new Vector3(0,1.433f,.244f),new Vector3(.08f,1.447f,.21f)},.012f,hair);
            for(int j=0;j<8;j++) { float a=j*Mathf.PI/4; Oval(t,"Tousled hair",new Vector3(Mathf.Cos(a)*.21f,1.76f,Mathf.Sin(a)*.19f),new Vector3(.19f,.18f,.14f),hair); }
            Oval(t,"Travel hat brim",new Vector3(0,1.8f,0),new Vector3(.83f,.075f,.71f),Leather);
            Oval(t,"Travel hat crown",new Vector3(0,1.92f,0),new Vector3(.57f,.3f,.52f),new Color(.44f,.29f,.12f));
            for(int s=-1;s<=1;s+=2) { Ring(t,"Goggle brass frame",new Vector3(s*.135f,1.91f,.255f),.092f,.025f,Brass); Oval(t,"Blue goggle lens",new Vector3(s*.135f,1.91f,.255f),new Vector3(.14f,.14f,.025f),Cyan); }
            Rod(t,"Goggle bridge",new Vector3(-.04f,1.91f,.27f),new Vector3(.04f,1.91f,.27f),.018f,Brass);
            Oval(t,"Golden scarf collar",new Vector3(0,1.32f,0),new Vector3(.52f,.14f,.46f),Gold);
            Leaf(t,"Scarf tail",new Vector3(.09f,1.13f,.24f),new Vector3(.24f,.39f,.35f),Gold,Quaternion.Euler(0,0,160));
            Box("Leather belt",t,new Vector3(0,.82f,.19f),new Vector3(.58f,.085f,.05f),Leather,false);
            Box("Belt buckle",t,new Vector3(0,.82f,.235f),new Vector3(.115f,.095f,.04f),Brass,false);
            Oval(t,"Engineering backpack",new Vector3(0,1.02f,-.31f),new Vector3(.54f,.68f,.28f),Leather);
            Box("Pack flap",t,new Vector3(0,1.25f,-.43f),new Vector3(.5f,.19f,.06f),new Color(.43f,.26f,.11f),false);
            for(int s=-1;s<=1;s+=2) Rod(t,"Backpack strap",new Vector3(s*.2f,1.3f,.01f),new Vector3(s*.2f,.84f,.21f),.025f,Leather);
            var roll=Cylinder("Bedroll",t,new Vector3(0,1.35f,-.33f),new Vector3(.18f,.31f,.18f),Ivory,false); roll.transform.localRotation=Quaternion.Euler(0,0,90);
            Ring(t,"Compass pendant",new Vector3(-.11f,1.08f,.24f),.055f,.013f,Brass);
            t.gameObject.AddComponent<StorybookMotion>(); return t;
        }
        public static Transform Dragon(Transform parent,Vector3 position)
        {
            var t=Root(parent,"Aster • storybook valley dragon",position); Color body=new Color(.19f,.44f,.35f), belly=new Color(.77f,.81f,.53f), wing=new Color(.71f,.35f,.16f);
            Oval(t,"Pear shaped dragon body",new Vector3(0,2.15f,.2f),new Vector3(3.7f,4.1f,4.4f),body);
            Oval(t,"Cream breastplate",new Vector3(0,2.15f,-1.3f),new Vector3(2.65f,3.3f,1.75f),belly);
            for(int j=0;j<6;j++) {
                float y=1+j*.4f; float relative=(y-2.15f)/1.65f;
                float z=-1.3f-.875f*Mathf.Sqrt(1-relative*relative)-.075f;
                Oval(t,"Overlapping belly scale",new Vector3(0,y,z),new Vector3(2.05f-j*.1f,.5f,.13f),Color.Lerp(belly,Ivory,.08f*j));
            }
            Oval(t,"Curved neck",new Vector3(0,3.7f,-1),new Vector3(2.35f,2.4f,2.5f),body);
            Oval(t,"Expressive head",new Vector3(0,4.7f,-1.7f),new Vector3(3.15f,2.5f,2.8f),body);
            Oval(t,"Soft snout",new Vector3(0,4.12f,-3.04f),new Vector3(2.65f,1.22f,1.37f),body*1.18f);
            Oval(t,"Lower jaw",new Vector3(0,3.73f,-2.94f),new Vector3(2.24f,.42f,1.2f),belly);
            Curve(t,"Gentle dragon smile",new[]{new Vector3(-1.03f,3.98f,-3.25f),new Vector3(-.68f,3.89f,-3.62f),new Vector3(0,3.87f,-3.72f),new Vector3(.68f,3.89f,-3.62f),new Vector3(1.03f,3.98f,-3.25f)},.035f,Ink);
            for(int s=-1;s<=1;s+=2) {
                Oval(t,"Eye socket",new Vector3(s*1.06f,4.87f,-2.73f),new Vector3(1.04f,1.11f,.52f),body*.8f);
                Oval(t,"Kind eye white",new Vector3(s*1.06f,4.9f,-2.96f),new Vector3(.75f,.9f,.23f),Ivory);
                Oval(t,"Amber iris",new Vector3(s*1.035f,4.86f,-3.08f),new Vector3(.42f,.58f,.14f),Gold);
                Oval(t,"Dragon pupil",new Vector3(s*1.035f,4.86f,-3.17f),new Vector3(.22f,.47f,.08f),Ink);
                Oval(t,"Eye sparkle",new Vector3(s*1.035f-.09f,5.02f,-3.22f),Vector3.one*.14f,Color.white,true);
                Oval(t,"Nostril",new Vector3(s*.6f,4.28f,-3.66f),new Vector3(.19f,.11f,.08f),Ink);
                Leaf(t,"Dragon ear",new Vector3(s*1.6f,5.02f,-1.23f),new Vector3(1.2f,1.3f,1),wing,Quaternion.Euler(0,0,-s*55));
                var hornPoints=new[]{new Vector3(s*1.02f,5.45f,-1.22f),new Vector3(s*1.17f,6.1f,-1),new Vector3(s*1.08f,6.73f,-.68f)};
                TaperedTail(t,"Curved gold horn",hornPoints,new[]{.31f,.18f,.025f},Brass);
                Oval(t,"Powerful haunch",new Vector3(s*1.5f,1.05f,.66f),new Vector3(1.7f,2.1f,2.1f),body);
                Oval(t,"Soft dragon paw",new Vector3(s*1.52f,.35f,-.16f),new Vector3(1.5f,.62f,1.62f),body);
                Rod(t,"Dragon forearm",new Vector3(s*1.35f,2.4f,-.86f),new Vector3(s*1.64f,.73f,-1.44f),.33f,body);
                Oval(t,"Front paw",new Vector3(s*1.65f,.42f,-1.57f),new Vector3(1,.63f,1.2f),body);
                for(int j=0;j<3;j++) { Oval(t,"Rounded ivory claw",new Vector3(s*1.65f+(j-1)*.25f,.36f,-2.05f),new Vector3(.19f,.18f,.42f),Ivory); }
                DragonWing(t,s,wing,body);
            }
            var tail=new[]{new Vector3(0,1.4f,1.8f),new Vector3(.7f,1.05f,3.15f),new Vector3(2,1,4.15f),new Vector3(3.45f,1.55f,4.22f),new Vector3(4,2.4f,3.8f)};
            TaperedTail(t,"Curling dragon tail",tail,new[]{.8f,.68f,.43f,.22f,.035f},body);
            for(int i=0;i<10;i++) Cone(t,new Vector3(0,4.0f-i*.24f,.1f+i*.3f),.18f,.55f,Brass);
            for(int s=-1;s<=1;s+=2) for(int row=0;row<6;row++) for(int j=0;j<4;j++) {
                float angle=(j*.29f+.3f)*s; var at=new Vector3(Mathf.Sin(angle)*1.84f,1.2f+row*.39f,.2f+Mathf.Cos(angle)*1.99f);
                Oval(t,"Layered jade scale",at,new Vector3(.35f,.3f,.065f),Color.Lerp(body,Gold,.055f*((row+j)%3)));
            }
            t.gameObject.AddComponent<StorybookMotion>(); return t;
        }
        static void DragonWing(Transform p,int side,Color membrane,Color body)
        {
            var wing=Root(p,side<0?"Wing L":"Wing R",new Vector3(side*1.3f,3,.55f));
            var a=Vector3.zero; var b=new Vector3(side*1.35f,2.1f,.3f); var c=new Vector3(side*4.1f,2.6f,1);
            var d=new Vector3(side*3.1f,.6f,1.6f); var e=new Vector3(side*2.5f,.95f,1.3f); var f=new Vector3(side*2.1f,-.65f,1.3f); var h=new Vector3(side*.8f,-1.1f,.6f);
            var v=new[]{a,b,c,d,e,f,h,new Vector3(side*1.65f,.75f,.65f)};
            var tris=new List<int>(); for(int i=0;i<7;i++) tris.AddRange(new[]{7,i,(i+1)%7});
            var mesh=DoubleSided("Sculpted wing membrane",v,tris.ToArray()); MeshPart(wing,"Amber wing membrane",Vector3.zero,Vector3.one,mesh,membrane);
            foreach(var point in new[]{c,d,f,h}) Rod(wing,"Wing finger",b,point,.055f,Brass);
            Curve(wing,"Wing leading edge",new[]{a,b,c},.11f,body);
        }
        public static void TaperedTail(Transform parent,string name,Vector3[] path,float[] radii,Color color)
        {
            const int sides=10; var vertices=new List<Vector3>(); var triangles=new List<int>();
            for(int i=0;i<path.Length;i++) {
                var direction=path[Mathf.Min(i+1,path.Length-1)]-path[Mathf.Max(i-1,0)]; var basis=Quaternion.FromToRotation(Vector3.up,direction.normalized);
                for(int j=0;j<sides;j++) { float a=j*Mathf.PI*2/sides; vertices.Add(path[i]+basis*new Vector3(Mathf.Cos(a)*radii[i],0,Mathf.Sin(a)*radii[i])); }
            }
            for(int i=0;i<path.Length-1;i++) for(int j=0;j<sides;j++) { int a=i*sides+j,b=i*sides+(j+1)%sides; triangles.AddRange(new[]{a,a+sides,b,b,a+sides,b+sides}); }
            var mesh=new Mesh {name=name}; mesh.SetVertices(vertices); mesh.SetTriangles(triangles,0); mesh.RecalculateNormals(); MeshPart(parent,name,Vector3.zero,Vector3.one,mesh,color);
        }
        static Mesh DoubleSided(string name,Vector3[] front,int[] indices)
        {
            var vertices=new Vector3[front.Length*2]; front.CopyTo(vertices,0); front.CopyTo(vertices,front.Length);
            var triangles=new int[indices.Length*2]; indices.CopyTo(triangles,0);
            for(int i=0;i<indices.Length;i+=3) { triangles[indices.Length+i]=indices[i]+front.Length; triangles[indices.Length+i+1]=indices[i+2]+front.Length; triangles[indices.Length+i+2]=indices[i+1]+front.Length; }
            var mesh=new Mesh {name=name}; mesh.vertices=vertices; mesh.triangles=triangles; mesh.RecalculateNormals(); return mesh;
        }
        public static void Lantern(Transform p,Vector3 at,float scale)
        {
            var t=Root(p,"Brass lantern",at); t.localScale=Vector3.one*scale;
            Oval(t,"Warm lantern glass",Vector3.zero,new Vector3(.8f,1.05f,.8f),Gold,true);
            Cylinder("Lantern base",t,new Vector3(0,-.55f,0),new Vector3(.9f,.065f,.9f),Brass,false);
            Cone(t,new Vector3(0,.47f,0),.51f,.4f,Brass,8);
            for(int i=0;i<4;i++) { float a=i*Mathf.PI/2; Rod(t,"Lantern cage",new Vector3(Mathf.Cos(a)*.4f,-.48f,Mathf.Sin(a)*.4f),new Vector3(Mathf.Cos(a)*.4f,.49f,Mathf.Sin(a)*.4f),.035f,Brass); }
            Ring(t,"Lantern handle",new Vector3(0,.94f,0),.2f,.028f,Brass);
        }
        public static Transform Crate(Transform parent,Vector3 p,float size=1)
        {
            var t=Root(parent,"Crafted delivery crate",p); t.localScale=Vector3.one*size;
            Box("Crate core",t,Vector3.zero,new Vector3(.94f,.94f,.94f),Leather,false);
            for(int s=-1;s<=1;s+=2) for(int j=0;j<4;j++) {
                Box("Wood plank",t,new Vector3(-.36f+j*.24f,0,s*.5f),new Vector3(.22f,.93f,.08f),Color.Lerp(Leather,Gold,.16f+(j%2)*.07f),false);
                Box("Crate band",t,new Vector3(s*.37f,0,0),new Vector3(.07f,1.05f,1.1f),Brass,false);
            }
            Rod(t,"Diagonal timber brace",new Vector3(-.43f,-.41f,-.56f),new Vector3(.43f,.41f,-.56f),.055f,Brass);
            Gem(t,new Vector3(0,.58f,0),Cyan,.27f); return t;
        }
        public static Transform Crystal(Transform parent,Vector3 p,Color color,float size=1)
        {
            var t=Root(parent,"Faceted crystal cluster",p); t.localScale=Vector3.one*size;
            for(int j=0;j<3;j++) {
                var r=Root(t,"Crystal shard",new Vector3((j-1)*.26f,0,j==1?0:.13f)); r.localRotation=Quaternion.Euler(j==0?-15:12,0,(j-1)*-22);
                Cylinder("Prism base",r,new Vector3(0,.26f,0),new Vector3(.25f,.26f,.25f),color,false);
                Cone(r,new Vector3(0,.52f,0),.18f,.35f,color,6);
            }
            return t;
        }
    }
}
