using System.Collections.Generic;
using UnityEngine;
using static DragonValley.StorybookAssets;
using static DragonValley.WorldArt;
namespace DragonValley
{
    // Authored from the participant's four real-kit reference photographs.
    // Unseen surfaces are simple matching card panels, not invented humanoid features.
    public static class FunobotzModels
    {
        static readonly Color Paper=new Color(.94f,.95f,.96f), Edge=new Color(1,.45f,.035f), Black=new Color(.035f,.035f,.04f);
        static readonly Color Yellow=new Color(1,.73f,.015f), Red=new Color(.85f,.035f,.055f), Green=new Color(.18f,.68f,.035f);
        public static GameObject Create(Transform parent,RobotKind kind)
        {
            var root=Root(parent,kind+" - reference kit",Vector3.zero);
            if(kind==RobotKind.Petalo) Petalo(root);
            if(kind==RobotKind.Quacky) Quacky(root);
            if(kind==RobotKind.Tolly) Tolly(root);
            if(kind==RobotKind.Tiko) Tiko(root);
            // Unity front is +Z here. Mirror the layout so handed details match
            // the supplied front views, then retain readable printed lettering.
            root.localScale=new Vector3(-root.localScale.x,root.localScale.y,root.localScale.z);
            foreach(var text in root.GetComponentsInChildren<TextMesh>()) {
                var scale=text.transform.localScale; text.transform.localScale=new Vector3(-scale.x,scale.y,scale.z);
            }
            var motion=root.gameObject.AddComponent<FunobotzMotion>(); motion.AnimateCrawling=kind==RobotKind.Quacky;
            foreach(var child in root.GetComponentsInChildren<Transform>()) if(child.name=="Carry socket") motion.CarrySocket=child;
            return root.gameObject;
        }
        static void CardBox(Transform t,string name,Vector3 p,Vector3 size)
        {
            Box(name,t,p,size,Paper,false);
            for(int a=-1;a<=1;a+=2) for(int b=-1;b<=1;b+=2) {
                Rod(t,"Orange card edge",p+new Vector3(-size.x/2,a*size.y/2,b*size.z/2),p+new Vector3(size.x/2,a*size.y/2,b*size.z/2),.005f,Edge);
                Rod(t,"Orange card edge",p+new Vector3(a*size.x/2,-size.y/2,b*size.z/2),p+new Vector3(a*size.x/2,size.y/2,b*size.z/2),.005f,Edge);
                Rod(t,"Orange card edge",p+new Vector3(a*size.x/2,b*size.y/2,-size.z/2),p+new Vector3(a*size.x/2,b*size.y/2,size.z/2),.005f,Edge);
            }
        }
        // A convex card profile extruded along Z, with flat faces and folded orange seams.
        static void Card(Transform t,string name,Vector2[] outline,float depth,Vector3 at)
        {
            float area=0;
            for(int i=0;i<outline.Length;i++) { var a=outline[i]; var b=outline[(i+1)%outline.Length]; area+=a.x*b.y-b.x*a.y; }
            if(area<0) System.Array.Reverse(outline);
            var r=Root(t,name,at); var v=new List<Vector3>(); var triangles=new List<int>();
            for(int face=0;face<2;face++) {
                int start=v.Count; float z=(face==0?1:-1)*depth*.5f;
                foreach(var p in outline) v.Add(new Vector3(p.x,p.y,z));
                for(int i=1;i<outline.Length-1;i++) {
                    if(face==0) triangles.AddRange(new[]{start,start+i,start+i+1});
                    else triangles.AddRange(new[]{start,start+i+1,start+i});
                }
            }
            for(int i=0;i<outline.Length;i++) {
                var a=outline[i]; var b=outline[(i+1)%outline.Length]; int n=v.Count;
                v.AddRange(new[]{new Vector3(a.x,a.y,depth/2),new Vector3(a.x,a.y,-depth/2),new Vector3(b.x,b.y,-depth/2),new Vector3(b.x,b.y,depth/2)});
                triangles.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});
                foreach(float z in new[]{-depth/2,depth/2}) Rod(r,"Printed orange outline",new Vector3(a.x,a.y,z),new Vector3(b.x,b.y,z),.004f,Edge);
                Rod(r,"Fold edge",new Vector3(a.x,a.y,-depth/2),new Vector3(a.x,a.y,depth/2),.004f,Edge);
            }
            var mesh=new Mesh{name=name}; mesh.SetVertices(v); mesh.SetTriangles(triangles,0); mesh.RecalculateNormals();
            MeshPart(r,name,Vector3.zero,Vector3.one,mesh,Paper);
        }
        static void Base(Transform t,float radius=.48f)
        {
            // Horizontal octagonal base, built as a rotated extruded profile.
            var r=Root(t,"Octagonal folded-card base",new Vector3(0,-.47f,0)); r.localRotation=Quaternion.Euler(90,0,0);
            var points=new Vector2[8];
            for(int i=0;i<8;i++) { float a=(i*45+22.5f)*Mathf.Deg2Rad; points[i]=new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius; }
            Card(r,"Octagonal shell",points,.24f,Vector3.zero);
            Logo(t,new Vector3(0,-.48f,radius*.924f+.008f),radius*.69f);
        }
        static void Logo(Transform t,Vector3 p,float width,Quaternion? rotation=null)
        {
            var label=Root(t,"FUNOBOTZ printed label",p); label.localRotation=rotation??Quaternion.Euler(0,180,0);
            var text=label.gameObject.AddComponent<TextMesh>(); text.text="F U N <color=#ED8400>O B O</color> T Z"; text.richText=true;
            text.fontSize=64; text.characterSize=.04f; text.anchor=TextAnchor.MiddleCenter; text.alignment=TextAlignment.Center; text.color=Black;
            var font=Resources.Load<Font>("ExpeditionSans"); if(font) { text.font=font; label.GetComponent<MeshRenderer>().sharedMaterial=font.material; }
            var bounds=label.GetComponent<Renderer>().localBounds; if(bounds.size.x>.001f) label.localScale=Vector3.one*(width/bounds.size.x);
        }
        static void Disc(Transform t,string name,Vector3 p,float radius,float depth,Color color,bool glow=false)
        {
            var go=Cylinder(name,t,p,new Vector3(radius*2,depth/2,radius*2),color,false); go.transform.localRotation=Quaternion.Euler(90,0,0);
            if(glow) go.GetComponent<Renderer>().sharedMaterial=Mat(color,true);
        }
        static void Petalo(Transform t)
        {
            Base(t);
            Card(t,"White tapered flower pot",new[]{new Vector2(-.24f,-.31f),new Vector2(.24f,-.31f),new Vector2(.37f,.24f),new Vector2(-.37f,.24f)},.42f,new Vector3(0,-.07f,0));
            Logo(t,new Vector3(0,-.28f,.217f),.38f);
            Box("Green square stem",t,new Vector3(0,.31f,0),new Vector3(.09f,.3f,.085f),Green,false);
            var head=Root(t,"Flower head",new Vector3(0,.77f,0));
            for(int i=0;i<8;i++) {
                float a=i*Mathf.PI/4; var pos=new Vector3(Mathf.Sin(a)*.35f,Mathf.Cos(a)*.35f,0);
                var rim=Oval(head,"Black petal outline",pos,new Vector3(.305f,.40f,.07f),Black); rim.transform.localRotation=Quaternion.Euler(0,0,-i*45);
                var petal=Oval(head,"Yellow flower petal",pos+Vector3.forward*.015f,new Vector3(.281f,.378f,.069f),Yellow); petal.transform.localRotation=rim.transform.localRotation;
                var back=Oval(head,"Yellow petal reverse",pos-Vector3.forward*.015f,new Vector3(.281f,.378f,.069f),Yellow); back.transform.localRotation=rim.transform.localRotation;
            }
            Disc(head,"Black flower rim",Vector3.forward*.028f,.303f,.085f,Black);
            Disc(head,"Red flower surround",Vector3.forward*.078f,.288f,.04f,Red);
            Disc(head,"Dark inner outline",Vector3.forward*.105f,.229f,.02f,Black);
            Disc(head,"Brown smiling face",Vector3.forward*.118f,.218f,.016f,new Color(.31f,.055f,.065f));
            foreach(int side in new[]{-1,1}) {
                var at=new Vector3(side*.095f,.066f,.15f);
                Ring(head,"LED eye ring",at,.045f,.009f,Yellow);
                Oval(head,"LED light eye",at,new Vector3(.071f,.071f,.04f),Yellow,true);
                Oval(head,"Eye bright centre",at+new Vector3(-.01f,.012f,.018f),Vector3.one*.023f,Color.white,true);
            }
            Curve(head,"White printed smile",new[]{new Vector3(-.15f,-.026f,.142f),new Vector3(-.09f,-.075f,.142f),new Vector3(0,-.095f,.142f),new Vector3(.1f,-.085f,.142f),new Vector3(.157f,-.03f,.142f)},.01f,Color.white);
            Oval(head,"Printed playful tongue",new Vector3(.084f,-.11f,.148f),new Vector3(.095f,.069f,.01f),Color.white);
        }
        static void FlexFoot(Transform t,string name,Vector3 point,Color color)
        {
            var r=Root(t,name,point);
            // The kit uses rounded cross connectors with an upright centre peg.
            foreach(var d in new[]{Vector3.left,Vector3.right,Vector3.forward,Vector3.back}) {
                Rod(r,"Cross connector stem",Vector3.zero,d*.083f,.031f,color);
                Oval(r,"Rounded connector end",d*.083f,Vector3.one*.062f,color);
            }
            Cylinder("Upright connector peg",r,new Vector3(0,.036f,0),new Vector3(.082f,.041f,.082f),color,false);
            Oval(r,"Rounded peg cap",new Vector3(0,.077f,0),new Vector3(.082f,.025f,.082f),color);
        }
        static void QuackyBase(Transform t)
        {
            var points=new Vector2[8];
            for(int i=0;i<8;i++) { float a=(i*45+22.5f)*Mathf.Deg2Rad; points[i]=new Vector2(Mathf.Cos(a),Mathf.Sin(a))*.60f; }
            var lower=Root(t,"Octagonal lower body",new Vector3(0,-.49f,0)); lower.localRotation=Quaternion.Euler(90,0,0);
            Card(lower,"Lower folded-card tier",(Vector2[])points.Clone(),.20f,Vector3.zero);
            var lid=Root(t,"Octagonal upper body",new Vector3(0,-.32f,0)); lid.localRotation=Quaternion.Euler(90,0,0);
            Card(lid,"Upper folded-card tier",points,.14f,Vector3.zero);
            KitLogo(t,new Vector3(-.07f,-.245f,-.055f),.70f,Quaternion.Euler(90,180,0));
        }
        static void KitLogo(Transform t,Vector3 at,float width,Quaternion rotation)
        {
            var label=Root(t,"Quacky FUNOBOTZ print",at); label.localRotation=rotation;
            var text=label.gameObject.AddComponent<TextMesh>(); text.text="<b>F U N <color=#171717>O <color=#FFFFFF>B</color> O</color> T Z</b>";
            text.richText=true; text.fontSize=80; text.characterSize=.04f; text.anchor=TextAnchor.MiddleCenter; text.alignment=TextAlignment.Center; text.color=Black;
            var font=Resources.Load<Font>("ExpeditionSans");
            if(font) { text.font=font; label.GetComponent<MeshRenderer>().sharedMaterial=font.material; }
            label.gameObject.AddComponent<KitPrint>().Configure(font,Shader.Find("DragonValley/KitPrint"));
            var bounds=label.GetComponent<Renderer>().localBounds;
            if(bounds.size.x>.001f) label.localScale=Vector3.one*(width/bounds.size.x);
            // A separate card print backing stays aligned when the common factory mirrors the lettering.
            var print=Root(t,"Orange brand panel",at); print.localRotation=rotation;
            Box("Orange OBO panel",print,new Vector3(-width*.077f,0,.002f),new Vector3(width*.39f,width*.16f,.001f),Edge,false);
            Rod(print,"Printed brand antenna",new Vector3(-width*.077f,width*.075f,.001f),new Vector3(-width*.077f,width*.13f,.001f),width*.009f,Edge);
            Oval(print,"Printed antenna dot",new Vector3(-width*.077f,width*.139f,.001f),new Vector3(width*.044f,width*.027f,.001f),Edge);
        }
        static void QuackyHead(Transform t)
        {
            var head=Root(t,"Triangular head",new Vector3(.045f,.42f,.59f));
            // Four corners form the folded wedge in the supplied photo. Its lower side
            // panels meet the neck; a thin extruded triangle would leave a visible gap.
            var a=new Vector3(-.29f,.28f,-.13f); var b=new Vector3(.29f,.37f,-.13f);
            var c=new Vector3(.23f,-.25f,.22f); var d=new Vector3(-.29f,-.25f,-.13f);
            var vertices=new List<Vector3>(); var triangles=new List<int>();
            var centre=(a+b+c+d)*.25f;
            foreach(var face in new[]{new[]{a,b,c},new[]{a,c,d},new[]{b,d,c},new[]{a,d,b}}) {
                if(Vector3.Dot(Vector3.Cross(face[1]-face[0],face[2]-face[0]),(face[0]+face[1]+face[2])/3-centre)<0) System.Array.Reverse(face);
                int start=vertices.Count; vertices.AddRange(face); triangles.AddRange(new[]{start,start+1,start+2});
            }
            var mesh=new Mesh{name="Quacky folded triangular wedge"}; mesh.SetVertices(vertices); mesh.SetTriangles(triangles,0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            MeshPart(head,"Folded triangular white head",Vector3.zero,Vector3.one,mesh,Paper);
            foreach(var pair in new[]{new[]{a,b},new[]{b,c},new[]{c,a},new[]{a,d},new[]{d,b},new[]{d,c}}) Rod(head,"Orange head fold",pair[0],pair[1],.004f,Edge);
            var normal=Vector3.Cross(c-a,b-a).normalized;
            var eyes=Root(head,"Eyes on sloping face",Vector3.zero);
            foreach(var weights in new[]{new Vector2(.23f,.33f),new Vector2(.57f,.28f)}) {
                var eye=Root(eyes,"Eye mount",a+(b-a)*weights.x+(c-a)*weights.y+normal*.005f);
                eye.localRotation=Quaternion.FromToRotation(Vector3.forward,normal);
                Disc(eye,"Black circular eye",Vector3.zero,.046f,.004f,Black);
            }
            var sideNormal=Vector3.Cross(d-b,c-b).normalized;
            if(Vector3.Dot(sideNormal,(b+c+d)/3-centre)<0) sideNormal=-sideNormal;
            var printRotation=Quaternion.LookRotation(-sideNormal,Vector3.Cross(-sideNormal,(b-c).normalized));
            KitLogo(head,(b+c+d)/3+sideNormal*.006f,.34f,printRotation);
        }
        static void Quacky(Transform t)
        {
            QuackyBase(t);
            CardBox(t,"Slim upright neck",new Vector3(.015f,-.14f,.54f),new Vector3(.18f,.70f,.17f));
            KitLogo(t,new Vector3(.015f,-.08f,.631f),.53f,Quaternion.Euler(0,180,90));
            QuackyHead(t);
            foreach(int side in new[]{-1,1}) {
                var arm=Root(t,side<0?"Flex arm L":"Flex arm R",new Vector3(side*.50f,-.29f,-.035f));
                Rod(arm,"Yellow flexible upper joint",Vector3.zero,new Vector3(side*.255f,0,0),.038f,Yellow);
                FlexFoot(arm,"Red connector",new Vector3(side*.27f,0,0),Red);
                Rod(arm,"Yellow flexible lower joint",new Vector3(side*.28f,-.005f,.022f),new Vector3(side*.30f,-.175f,.23f),.038f,Yellow);
                FlexFoot(arm,"Green crawling foot",new Vector3(side*.30f,-.21f,.25f),Green);
            }
        }
        static void Tolly(Transform t)
        {
            CardBox(t,"Square checkpoint base",new Vector3(0,-.49f,0),new Vector3(.65f,.22f,.56f));
            CardBox(t,"Tall square card column",new Vector3(0,-.12f,0),new Vector3(.41f,.54f,.34f));
            CardBox(t,"Smiling square head",new Vector3(0,.4f,0),new Vector3(.46f,.45f,.36f));
            Logo(t,new Vector3(0,-.51f,.287f),.54f);
            Logo(t,new Vector3(-.15f,-.13f,.178f),.37f,Quaternion.Euler(0,180,90));
            foreach(int side in new[]{-1,1}) {
                var eye=new Vector3(side*.105f,.47f,.189f); Disc(t,"Eye surround",eye,.03f,.015f,new Color(.7f,.7f,.66f)); Disc(t,"Black eye",eye+Vector3.forward*.013f,.018f,.017f,Black);
            }
            Curve(t,"Printed checkpoint smile",new[]{new Vector3(-.057f,.333f,.189f),new Vector3(-.025f,.303f,.19f),new Vector3(.011f,.31f,.19f),new Vector3(.046f,.345f,.189f),new Vector3(.062f,.322f,.189f)},.008f,Black);
            CardBox(t,"Dual indicator mast",new Vector3(-.07f,.79f,-.018f),new Vector3(.18f,.34f,.12f));
            Disc(t,"Green access indicator",new Vector3(-.07f,.86f,.048f),.026f,.032f,Green,true);
            Disc(t,"Red stop indicator",new Vector3(-.07f,.73f,.048f),.026f,.032f,Red,true);
            foreach(int side in new[]{-1,1}) {
                var button=Cylinder(side<0?"Red checkpoint button":"Green checkpoint button",t,new Vector3(side*.22f,-.35f,.17f),new Vector3(.076f,.025f,.076f),side<0?Red:Green,false);
            }
            var hinge=Root(t,"Tollgate pivot",new Vector3(.225f,.16f,0));
            CardBox(hinge,"Raised and lowered tollgate arm",new Vector3(.26f,0,0),new Vector3(.52f,.14f,.048f));
            Logo(hinge,new Vector3(.265f,0,.029f),.39f);
            Disc(t,"Tollgate hinge",new Vector3(.225f,.16f,.048f),.045f,.035f,Edge);
        }
        static void Tiko(Transform t)
        {
            // Low card chassis with accordion supports and a five-section lifting arm.
            Card(t,"Angular card chassis",new[]{new Vector2(-.4f,-.19f),new Vector2(-.52f,.04f),new Vector2(-.29f,.26f),new Vector2(.26f,.22f),new Vector2(.34f,-.12f),new Vector2(-.08f,-.28f)},.18f,new Vector3(-.18f,-.12f,0));
            Logo(t,new Vector3(-.25f,-.1f,.099f),.52f);
            foreach(int side in new[]{-1,1}) {
                var leg=Root(t,"Accordion support",new Vector3(-.49f,side<0?-.3f:.09f,0));
                Vector3 last=Vector3.zero;
                for(int j=1;j<=7;j++) {
                    var next=new Vector3(-j*.048f,(j%2==0?0:.08f)+side*j*.019f,0);
                    Rod(leg,"Folded card edge",last,next,.014f,Paper); Rod(leg,"Orange fold line",last+Vector3.forward*.016f,next+Vector3.forward*.016f,.003f,Edge); last=next;
                }
            }
            for(int j=0;j<3;j++) foreach(int side in new[]{-1,1}) {
                var triangle=new[]{new Vector2(-.06f,0),new Vector2(.06f,0),new Vector2(.03f,side*.18f)};
                if(side<0) System.Array.Reverse(triangle);
                Card(t,"Triangular folded fin",triangle,.025f,new Vector3(-.34f+j*.16f,side<0?-.37f:.13f,-.04f));
            }
            var arm=Root(t,"Segmented arm pivot",new Vector3(.13f,-.10f,0));
            var joint=arm;
            for(int i=0;i<5;i++) {
                if(i>0) joint=Root(joint,"Flexible arm joint "+i,new Vector3(.21f,0,0));
                Card(joint,"Segment "+(i+1),new[]{new Vector2(0,-.075f),new Vector2(.21f,-.066f),new Vector2(.21f,.067f),new Vector2(0,.078f)},.13f,Vector3.zero);
                Curve(joint,"Diagonal card brace",new[]{new Vector3(.015f,-.059f,.071f),new Vector3(.195f,.053f,.071f)},.004f,Edge);
                Curve(joint,"Cross card brace",new[]{new Vector3(.015f,.062f,.073f),new Vector3(.195f,-.05f,.073f)},.0035f,new Color(.64f,.63f,.6f));
                foreach(float x in new[]{.035f,.174f}) Disc(joint,"Black pivot fastener",new Vector3(x,0,.077f),.012f,.012f,Black);
            }
            var claw=Root(joint,"End gripper",new Vector3(.21f,0,0));
            foreach(int side in new[]{-1,1}) Card(claw,"Curved card gripper",new[]{new Vector2(0,side*.018f),new Vector2(.26f,side*.035f),new Vector2(.13f,side*.106f),new Vector2(-.015f,side*.065f)},.07f,Vector3.zero);
            Root(claw,"Carry socket",new Vector3(.18f,.05f,0));
            // The real kit is long and low. A matching small scale keeps the path readable.
            t.localScale=Vector3.one*.8f;
            foreach(Transform child in t) child.localPosition+=Vector3.down*.25f;
        }
    }
}
