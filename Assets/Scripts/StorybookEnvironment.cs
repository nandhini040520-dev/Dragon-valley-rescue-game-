using System.Collections.Generic;
using UnityEngine;
using static DragonValley.WorldArt;
using static DragonValley.StorybookAssets;
namespace DragonValley
{
    public static class StorybookEnvironment
    {
        public static void Tree(Transform parent,Vector3 p,float scale,Color tint)
        {
            var t=Root(parent,"Old storybook tree",p); t.localScale=Vector3.one*scale;
            TaperedTail(t,"Twisting trunk",new[]{Vector3.zero,new Vector3(.22f,1,0),new Vector3(-.1f,2.3f,.1f),new Vector3(.4f,3.6f,0)},new[]{.52f,.41f,.29f,.1f},Leather);
            for(int s=-1;s<=1;s+=2) {
                TaperedTail(t,"Spreading branch",new[]{new Vector3(0,1.5f,0),new Vector3(s*.9f,2.7f,.25f),new Vector3(s*1.65f,3.7f,.1f)},new[]{.24f,.15f,.045f},Leather);
                Rod(t,"Tree root",new Vector3(0,.25f,0),new Vector3(s*1.15f,.05f,.35f),.13f,Leather);
            }
            for(int j=0;j<7;j++) {
                float a=j*2.4f; float r=j==0?0:1.3f;
                Oval(t,"Painted leafy crown",new Vector3(Mathf.Cos(a)*r,3.8f+(j%3)*.38f,Mathf.Sin(a)*r),new Vector3(2.55f,1.72f,2.5f),Color.Lerp(tint,new Color(.66f,.7f,.35f),(j%3)*.12f));
            }
            for(int j=0;j<5;j++) Leaf(t,"Leaf spray",new Vector3(Mathf.Sin(j)*2.1f,3.4f,Mathf.Cos(j)*1.8f),new Vector3(.5f,.7f,.4f),tint*1.2f,Quaternion.Euler(20,j*70,40));
        }
        public static Transform MushroomHouse(Transform parent,Vector3 at,float scale=1)
        {
            var t=Root(parent,"Fairy mushroom cottage",at); t.localScale=Vector3.one*scale;
            Oval(t,"Plaster cottage",new Vector3(0,.95f,0),new Vector3(1.9f,2,1.65f),Ivory);
            Oval(t,"Copper rose roof",new Vector3(0,1.96f,0),new Vector3(2.7f,.9f,2.4f),new Color(.64f,.22f,.24f));
            for(int j=0;j<7;j++) { float a=j*Mathf.PI*2/7; Oval(t,"Roof spot",new Vector3(Mathf.Sin(a)*.84f,2.3f,Mathf.Cos(a)*.73f),new Vector3(.27f,.04f,.28f),Ivory); }
            Oval(t,"Arched oak door",new Vector3(0,.6f,-.78f),new Vector3(.62f,1.1f,.12f),Leather);
            Ring(t,"Round glowing window",new Vector3(.59f,1.18f,-.63f),.23f,.045f,Brass);
            Oval(t,"Window glass",new Vector3(.59f,1.18f,-.64f),new Vector3(.39f,.39f,.06f),Gold,true);
            for(int s=-1;s<=1;s+=2) Rod(t,"Porch pillar",new Vector3(s*.64f,0,-1.05f),new Vector3(s*.64f,1.2f,-1.05f),.07f,Leather);
            Box("Porch awning",t,new Vector3(0,1.28f,-1),new Vector3(1.4f,.13f,.7f),Brass,false);
            return t;
        }
        static void Arch(Transform parent,Vector3 at,float radius,float height,Color color)
        {
            for(int s=-1;s<=1;s+=2) {
                Cylinder("Carved arch column",parent,at+new Vector3(s*radius,height*.45f,0),new Vector3(.74f,height*.45f,.74f),color,false);
                for(int j=0;j<3;j++) Cylinder("Column moulding",parent,at+new Vector3(s*radius,j*height*.42f+.18f,0),new Vector3(.95f,.1f,.95f),Ivory,false);
            }
            for(int j=0;j<13;j++) {
                float a=j*Mathf.PI/12; var b=Box("Carved arch stone",parent,at+new Vector3(Mathf.Cos(a)*radius,height*.88f+Mathf.Sin(a)*radius*.58f,0),new Vector3(.78f,.57f,1),color,false);
                b.transform.localRotation=Quaternion.Euler(0,0,a*Mathf.Rad2Deg-90);
            }
        }
        static void Pennant(Transform p,Vector3 at,Color cloth,float scale=1)
        {
            Rod(p,"Banner pole",at,at+Vector3.up*4*scale,.045f,Brass);
            Box("Hanging cloth banner",p,at+new Vector3(.48f,2.65f,-.025f)*scale,new Vector3(.85f,1.8f,.04f)*scale,cloth,false);
            Leaf(p,"Banner pointed hem",at+new Vector3(.48f,1.65f,-.025f)*scale,new Vector3(1.2f,.58f,.01f)*scale,cloth,Quaternion.Euler(0,0,180));
            Gem(p,at+new Vector3(.48f,2.65f,-.08f)*scale,Gold,.3f*scale);
        }
        static void Rock(Transform parent,Vector3 at,Vector3 size)
        {
            var rock=Oval(parent,"Mossy sculpted boulder",at,size,new Color(.34f,.39f,.36f)); rock.transform.localRotation=Quaternion.Euler(8,at.z*13,16);
            Oval(parent,"Moss cap",at+Vector3.up*size.y*.37f,new Vector3(size.x*.8f,size.y*.21f,size.z*.72f),new Color(.28f,.4f,.2f));
        }
        public static void Enhance(AdventureWorld world)
        {
            Random.InitState(220917);
            for(int zone=0;zone<7;zone++) {
                var root=Root(world.Zones[zone],"Storybook scenery",Vector3.zero);
                // Worn cobbles, verge plants, lanterns and individual island silhouettes.
                if(zone!=2) for(int row=0;row<15;row++) for(int col=0;col<3;col++) {
                    float x=(col-1)*1.46f+(row%2)*.25f; float z=-19+row*2.7f;
                    var cobble=Oval(root,"Trail flagstone",new Vector3(x,.025f,z),new Vector3(1.25f,.1f,1.75f),zone==4?new Color(.34f,.36f,.43f):new Color(.64f,.61f,.45f)); cobble.transform.localRotation=Quaternion.Euler(0,Random.Range(-9,9),0);
                }
                for(int side=-1;side<=1;side+=2) {
                    for(int j=0;j<7;j++) {
                        float z=-19+j*6; Rock(root,new Vector3(side*(14+Random.value*2),-.1f,z),new Vector3(4,3+Random.value*2,5));
                        if(zone!=4) for(int k=0;k<3;k++) {
                            var at=new Vector3(side*(3.8f+Random.value*3),.04f,z+Random.Range(-2,2));
                            Leaf(root,"Verge fern",at+Vector3.up*.4f,new Vector3(.5f,.8f,.5f),new Color(.33f,.46f,.26f),Quaternion.Euler(20,k*80,side*25));
                            if(k==0) Flower(root,at,zone==5?Pink:new Color(.79f,.69f,.4f),.8f);
                        }
                    }
                    for(int j=0;j<3;j++) {
                        var at=new Vector3(side*6,0,-13+j*14); Rod(root,"Lantern post",at,at+Vector3.up*2.8f,.075f,Leather);
                        Rod(root,"Lantern crook",at+Vector3.up*2.7f,at+new Vector3(-side*.5f,2.7f,0),.055f,Brass);
                        Lantern(root,at+new Vector3(-side*.5f,2.1f,0),.4f);
                    }
                }
                if(zone<6) {
                    Arch(root,new Vector3(0,0,20.55f),4.3f,5.6f,new Color(.65f,.64f,.48f));
                    Pennant(root,new Vector3(-5.3f,2,20),Blue,.9f); Pennant(root,new Vector3(5.3f,2,20),Blue,.9f);
                }
                if(zone==0) Gateway(root);
                if(zone==1) Forest(root);
                if(zone==2) Bridge(root);
                if(zone==3) Castle(root);
                if(zone==4) Cave(root);
                if(zone==5) Garden(root);
                if(zone==6) Valley(root);
                TrailDetails(root,zone);
                CombineScenery(root);
            }
            UpgradePuzzleProps(world);
        }
        static void TrailDetails(Transform root,int zone)
        {
            // Low, non-solid details keep every puzzle approach and bridge crossing clear.
            Color[] accents={Brass,Jade,Cyan,Blue,new Color(.48f,.43f,.68f),new Color(.72f,.40f,.48f),new Color(.41f,.66f,.61f)};
            var accent=accents[zone];
            var border=zone==4?new Color(.29f,.33f,.42f):new Color(.75f,.73f,.59f);
            for(int side=-1;side<=1;side+=2) {
                for(int row=0;row<22;row++) {
                    float z=-20+row*1.9f;
                    if(zone==2 && z>-6 && z<5) continue;
                    var stone=Box("Worn trail border",root,new Vector3(side*2.92f,.07f,z),new Vector3(.30f,.14f,1.64f),border,false);
                    stone.transform.localRotation=Quaternion.Euler(0,(row%3-1)*3,0);
                }
                // Matching region pennants frame the arrival without blocking the view.
                Pennant(root,new Vector3(side*3.65f,0,-18.8f),accent,.62f);
                Cylinder("Wayfinder foot",root,new Vector3(side*3.65f,.10f,-18.8f),new Vector3(.8f,.1f,.8f),border,false);
                if(zone==0 || zone==1 || zone==5) for(int group=0;group<3;group++) {
                    float z=-16+group*16;
                    for(int petal=0;petal<5;petal++) {
                        float a=petal*Mathf.PI*2/5;
                        Flower(root,new Vector3(side*8.8f+Mathf.Cos(a)*.6f,.03f,z+Mathf.Sin(a)*.6f),zone==5?Pink:Gold,.65f);
                    }
                }
            }
            // A brass compass set into the path gives each entry a small focal point.
            Ring(root,"Arrival compass",new Vector3(0,.10f,-18.4f),1.02f,.055f,accent,Quaternion.Euler(90,0,0));
            for(int direction=0;direction<4;direction++) {
                float a=direction*Mathf.PI*.5f;
                var tile=Box("Compass inlay",root,new Vector3(Mathf.Sin(a)*.66f,.105f,-18.4f+Mathf.Cos(a)*.66f),new Vector3(.17f,.035f,.38f),accent,false);
                tile.transform.localRotation=Quaternion.Euler(0,direction*90,0);
            }
        }
        static void Gateway(Transform t)
        {
            for(int s=-1;s<=1;s+=2) {
                Cylinder("Gateway watchtower",t,new Vector3(s*11,4.5f,14),new Vector3(4,4.5f,4),Ivory,false);
                Cone(t,new Vector3(s*11,9,14),2.8f,4,Blue,12);
                for(int j=0;j<5;j++) Box("Tower stone belt",t,new Vector3(s*11,1.5f+j*1.6f,11.99f),new Vector3(2.7f,.16f,.15f),Brass,false);
                Pennant(t,new Vector3(s*11,12.5f,14),Blue,.7f);
                Arch(t,new Vector3(s*5,0,-4),2.1f,2.7f,Ivory);
            }
            Ring(t,"Great clockwork sun",new Vector3(0,7.9f,20),2.15f,.14f,Brass);
            for(int j=0;j<12;j++) { float a=j*Mathf.PI/6; Rod(t,"Sun rays",new Vector3(0,7.9f,20),new Vector3(Mathf.Cos(a)*1.7f,7.9f+Mathf.Sin(a)*1.7f,20),.06f,Brass); }
        }
        static void Forest(Transform t)
        {
            for(int s=-1;s<=1;s+=2) {
                Tree(t,new Vector3(s*8,0,7),1.25f,new Color(.19f,.34f,.2f));
                Curve(t,"Hanging forest vine",new[]{new Vector3(s*7,5.6f,7),new Vector3(s*6,4.1f,7),new Vector3(s*4.4f,4.5f,7)},.035f,Jade);
                for(int j=0;j<4;j++) MushroomHouse(t,new Vector3(s*(9+j%2),0,-12+j*7),.3f);
            }
            var log=Root(t,"Hollow mossy log",new Vector3(8,.7f,-5)); var trunk=Cylinder("Log",log,Vector3.zero,new Vector3(1.25f,2,1.25f),Leather,false); trunk.transform.localRotation=Quaternion.Euler(90,0,0);
            Ring(log,"Log tree rings",new Vector3(0,0,-2),.5f,.06f,Brass);
        }
        static void Bridge(Transform t)
        {
            for(int s=-1;s<=1;s+=2) {
                var points=new Vector3[15]; for(int i=0;i<15;i++) points[i]=new Vector3(s*2.78f,.9f+Mathf.Sin(i*Mathf.PI/14)*.9f,-6+i*.72f);
                Curve(t,"Curved bridge rope",points,.08f,Brass);
                for(int i=0;i<6;i++) Rod(t,"Decorative baluster",new Vector3(s*2.7f,.1f,-5+i*1.6f),new Vector3(s*2.7f,1.3f,-5+i*1.6f),.09f,Leather);
                Arch(t,new Vector3(s*10,-4,0),2.5f,3,new Color(.47f,.48f,.41f));
                Oval(t,"River island",new Vector3(s*9,-3.9f,-1),new Vector3(9,2,12),Jade);
            }
            for(int j=0;j<11;j++) {
                Box("Waterfall ribbon",t,new Vector3(-16+j*.45f,-3.8f,-1),new Vector3(.32f,6,.22f),Color.Lerp(Cyan,Color.white,j%3*.25f),false,true);
                Oval(t,"Foaming river ripple",new Vector3(Random.Range(-14,14),-3.48f,Random.Range(-4,3)),new Vector3(1.6f,.03f,.32f),new Color(.59f,.82f,.79f));
            }
        }
        static void Castle(Transform t)
        {
            for(int s=-1;s<=1;s+=2) {
                for(int j=0;j<3;j++) {
                    float z=-3+j*8; Cylinder("Ivory castle turret",t,new Vector3(s*12,5,z),new Vector3(4,5,4),Ivory,false); Cone(t,new Vector3(s*12,10,z),2.55f,4.3f,Blue,12);
                    for(int k=0;k<3;k++) { Oval(t,"Arched tower window",new Vector3(s*12,2.7f+k*2.4f,z-2.01f),new Vector3(.6f,1.3f,.07f),Ink); Rod(t,"Window mullion",new Vector3(s*12,2.1f+k*2.4f,z-2.06f),new Vector3(s*12,3.1f+k*2.4f,z-2.06f),.045f,Brass); }
                    Pennant(t,new Vector3(s*12,14,z),Blue,.65f);
                }
                for(int j=0;j<8;j++) Box("Castle crenellation",t,new Vector3(s*8,4.5f,-3+j*2.9f),new Vector3(1,.6f,1.2f),Ivory,false);
            }
            Cylinder("Courtyard fountain",t,new Vector3(-7,.3f,5),new Vector3(3.5f,.3f,3.5f),Ivory,false);
            Cylinder("Fountain water",t,new Vector3(-7,.63f,5),new Vector3(3,.03f,3),Cyan,false);
            Crystal(t,new Vector3(-7,1,5),Cyan,1.5f);
            // Dress the actual central lift rather than suggesting a second, inactive lift.
            Rod(t,"Lift overhead beam",new Vector3(-2,7,0),new Vector3(2,7,0),.12f,Brass);
            Ring(t,"Tower lift pulley",new Vector3(0,7,0),.6f,.12f,Brass);
        }
        static void Cave(Transform t)
        {
            for(int j=0;j<7;j++) for(int s=-1;s<=1;s+=2) {
                Crystal(t,new Vector3(s*(6.5f+Random.value),.1f,-15+j*5),Color.Lerp(new Color(.29f,.23f,.49f),Cyan,(j%3)*.18f),1.8f+(j%2));
                var stalactite=Cone(t,new Vector3(s*6.4f,8,-13+j*5),.7f,2.2f,new Color(.25f,.25f,.32f)); stalactite.transform.localRotation=Quaternion.Euler(180,0,0);
            }
            for(int j=0;j<3;j++) {
                Ring(t,"Ancient rune circle",new Vector3(-7.8f,3.6f,-8+j*9),1.2f,.06f,Cyan,Quaternion.Euler(0,90,0));
                Lantern(t,new Vector3(-6.8f,1.1f,-8+j*9),.5f);
            }
        }
        static void Garden(Transform t)
        {
            for(int s=-1;s<=1;s+=2) for(int j=0;j<3;j++) {
                MushroomHouse(t,new Vector3(s*10,0,-11+j*11),.9f+j*.1f);
                Tree(t,new Vector3(s*13,0,-14+j*12),.8f,new Color(.66f,.38f,.44f));
                for(int k=0;k<5;k++) { var at=new Vector3(s*(7+Random.value*2),0,-13+j*12+Random.value*4); Flower(t,at,k%2==0?Pink:Ivory,1.2f); }
            }
            Arch(t,new Vector3(0,0,16),3.6f,3.2f,Jade);
            for(int j=0;j<11;j++) Flower(t,new Vector3(-3.2f+j*.64f,4.1f+Mathf.Sin(j*Mathf.PI/10)*1.2f,16),Pink,.8f);
            for(int side=-1;side<=1;side+=2) for(int j=0;j<8;j++) {
                var at=new Vector3(side*6.2f,0,-16+j*4); Rod(t,"Garden fence picket",at,at+Vector3.up*.95f,.055f,Ivory);
                Rod(t,"Garden fence rail",at+Vector3.up*.5f,at+new Vector3(0,.5f,3.8f),.035f,Brass);
            }
        }
        static void Valley(Transform t)
        {
            for(int s=-1;s<=1;s+=2) {
                for(int j=0;j<4;j++) { Rock(t,new Vector3(s*(12+j%2),6,-12+j*10),new Vector3(8,15+j*2,9)); Tree(t,new Vector3(s*12,12,-6+j*8),.65f,Jade); }
                for(int j=0;j<3;j++) Ring(t,"Restoration gear",new Vector3(s*9,2.8f,-9+j*3),.8f,.13f,Brass);
                Curve(t,"Restoration copper pipe",new[]{new Vector3(s*8,.7f,-12),new Vector3(s*8,2.8f,-9),new Vector3(s*8,2.8f,1),new Vector3(s*5,2.8f,1)},.1f,Brass);
            }
            Arch(t,new Vector3(0,0,21),3.1f,3.5f,Ivory);
            Cylinder("Restoration altar base",t,new Vector3(0,.15f,5),new Vector3(2.3f,.15f,2.3f),Ivory,false);
            Ring(t,"Altar compass",new Vector3(0,.33f,5),.9f,.06f,Brass,Quaternion.Euler(90,0,0));
        }
        static void UpgradePuzzleProps(AdventureWorld world)
        {
            foreach(var item in world.Interactables) {
                if(item.movable) {
                    var old=item.movable; Transform replacement=null; var parent=old.parent; var point=old.localPosition;
                    if(item.objectiveId=="energy" || item.objectiveId=="cave_remote") replacement=Crystal(parent,point,Cyan,.8f);
                    else if(item.requiredRobot==RobotKind.Quacky || item.objectiveId=="container") replacement=Crate(parent,point,item.requiredRobot==RobotKind.Quacky?.65f:1.2f);
                    if(item.objectiveId=="branch") {
                        replacement=Root(parent,"Fallen oak branch",point); TaperedTail(replacement,"Twisting fallen branch",new[]{new Vector3(-3,0,0),new Vector3(-1,.1f,0),new Vector3(1,0,.2f),new Vector3(3,.2f,0)},new[]{.34f,.43f,.32f,.12f},Leather);
                        for(int s=-1;s<=1;s+=2) { Rod(replacement,"Broken twig",new Vector3(s*1.3f,0,0),new Vector3(s*1.7f,.2f,1),.12f,Leather); Leaf(replacement,"Last oak leaf",new Vector3(s*1.7f,.3f,1),Vector3.one*.7f,Jade,Quaternion.Euler(75,0,25)); }
                    }
                    if(replacement) {
                        // Existing effect closures use the original transform; upgrade its visible children in place.
                        var renderer=old.GetComponent<Renderer>(); if(renderer) renderer.enabled=false;
                        old.localScale=Vector3.one; old.localRotation=Quaternion.identity;
                        replacement.SetParent(old,false); replacement.localPosition=Vector3.zero; replacement.localRotation=Quaternion.identity;
                    }
                }
                if(item.objectiveId.Contains("light") || item.objectiveId.StartsWith("cave_") || item.objectiveId=="beacon") {
                    if(item.requiredRobot==RobotKind.Petalo) Lantern(item.transform,new Vector3(0,.3f,0),.45f);
                }
            }
        }
        static void CombineScenery(Transform root)
        {
            var groups=new Dictionary<Material,List<CombineInstance>>(); var sources=new List<GameObject>();
            foreach(Transform child in root) sources.Add(child.gameObject);
            foreach(var filter in root.GetComponentsInChildren<MeshFilter>()) {
                var renderer=filter.GetComponent<MeshRenderer>(); if(!renderer || !renderer.enabled || !filter.sharedMesh) continue;
                if(!groups.TryGetValue(renderer.sharedMaterial,out var list)) groups[renderer.sharedMaterial]=list=new List<CombineInstance>();
                list.Add(new CombineInstance{mesh=filter.sharedMesh,transform=root.worldToLocalMatrix*filter.transform.localToWorldMatrix});
                renderer.enabled=false;
            }
            foreach(var pair in groups) {
                var mesh=new Mesh {name="Storybook static scenery",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32}; mesh.CombineMeshes(pair.Value.ToArray(),true,true);
                var go=Root(root,"Combined painted scenery",Vector3.zero); go.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh; go.gameObject.AddComponent<MeshRenderer>().sharedMaterial=pair.Key;
            }
            foreach(var source in sources) { if(Application.isPlaying) Object.Destroy(source); else Object.DestroyImmediate(source); }
        }
    }
}
