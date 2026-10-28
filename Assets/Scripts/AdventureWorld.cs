using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DragonValley.WorldArt;
namespace DragonValley
{
    public class AdventureWorld : MonoBehaviour
    {
        public const float ZoneLength=44;
        public readonly List<InteractableBase> Interactables=new List<InteractableBase>();
        public readonly Dictionary<string,Action> Effects=new Dictionary<string,Action>();
        public readonly List<Transform> Gates=new List<Transform>();
        public readonly List<GameObject> ValleyLights=new List<GameObject>();
        public Transform Dragon, FinalBarrier, Portal;
        public Transform[] Zones=new Transform[7];
        public MechanismController[] Machines=new MechanismController[7];
        public bool Restored { get; private set; }
        Color grass=new Color(.25f,.48f,.37f), stone=new Color(.48f,.59f,.59f), sand=new Color(.78f,.74f,.55f);
        public Vector3 Spawn(int region)=>new Vector3(0,.15f,region*ZoneLength-14);
        public void Build()
        {
            UnityEngine.Random.InitState(7301);
            RenderSettings.ambientLight=new Color(.57f,.66f,.73f); RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.fog=true; RenderSettings.fogMode=FogMode.ExponentialSquared; RenderSettings.fogDensity=.008f; RenderSettings.fogColor=new Color(.62f,.78f,.8f);
            var sun=new GameObject("Warm afternoon sun").AddComponent<Light>(); sun.transform.SetParent(transform); sun.type=LightType.Directional; sun.color=new Color(1,.94f,.83f); sun.intensity=.95f; sun.shadows=LightShadows.Soft; sun.transform.rotation=Quaternion.Euler(48,-35,0);
            Color[] terrain={new Color(.35f,.59f,.43f),grass,new Color(.33f,.57f,.53f),new Color(.43f,.55f,.52f),new Color(.2f,.25f,.34f),new Color(.46f,.64f,.45f),new Color(.34f,.48f,.51f)};
            for(int i=0;i<7;i++)
            {
                var zone=new GameObject((i+1)+" • "+AdventureGame.Instance.Missions.Missions[i].zone).transform; zone.SetParent(transform); zone.position=Vector3.forward*i*ZoneLength; Zones[i]=zone;
                if(i==2) {
                    Box("Near river bank",zone,new Vector3(0,-1,-13.5f),new Vector3(34,2,17),terrain[i]);
                    Box("Far river bank",zone,new Vector3(0,-1,12.5f),new Vector3(34,2,19),terrain[i]);
                    Box("River",zone,new Vector3(0,-3.6f,-1),new Vector3(65,.15f,20),new Color(.15f,.58f,.76f),false,true);
                } else Box("Floating island",zone,new Vector3(0,-1,0),new Vector3(34,2,44),terrain[i]);
                if(i!=2) Box("Discovery trail",zone,new Vector3(0,.025f,0),new Vector3(5.5f,.05f,44),i==4?stone*.6f:sand,false);
                for(int s=-1;s<=1;s+=2) {
                    var edge=Box("Safety boundary",zone,new Vector3(s*17,5,0),new Vector3(.2f,16,44),stone); edge.GetComponent<Renderer>().enabled=false;
                    for(int j=0;j<7;j++) {
                        float z=-18+j*6;
                        var rock=Ball("Faceted island rock",zone,new Vector3(s*(15+UnityEngine.Random.value*3),-.5f,z),new Vector3(5,4,5),stone*(.6f+UnityEngine.Random.value*.4f));
                        rock.transform.rotation=UnityEngine.Random.rotation;
                        if(i!=3 && i!=4 && i!=6) Tree(zone,new Vector3(s*(10+UnityEngine.Random.value*4),0,z),.7f+UnityEngine.Random.value*.6f,i==5?new Color(.49f,.66f,.48f):new Color(.2f,.43f,.36f));
                    }
                }
                if(i<6) {
                    var gate=Box("Region lock • "+i,zone,new Vector3(0,2.7f,21),new Vector3(33,5.4f,.5f),new Color(.18f,.33f,.39f)); Gates.Add(gate.transform);
                    for(int s=-1;s<=1;s+=2) { Box("Gateway pier",zone,new Vector3(s*4,3,21),new Vector3(1.4f,6,1.5f),stone); Gem(zone,new Vector3(s*4,6.4f,21),Gold,.6f); }
                    Box("Arch lintel",zone,new Vector3(0,5.8f,21),new Vector3(9,1,1.5f),stone);
                    Label(zone,"NEXT DISCOVERY  →",new Vector3(0,5.85f,20.15f),.18f,Ink);
                }
                // Tall landmarks remain legible from the previous island.
                // Keep the arrival sign outside the crystal approach and camera corridor.
                Box("Expedition sign",zone,new Vector3(-10,2.8f,-17),new Vector3(5,2.1f,.3f),Ink,false);
                Cylinder("Sign post",zone,new Vector3(-10,1,-17),new Vector3(.2f,1,.2f),Gold,false);
                Label(zone,"0"+(i+1)+" / "+AdventureGame.Instance.Missions.Missions[i].zone.ToUpperInvariant().Replace(" ","\n"),new Vector3(-10,2.8f,-17.18f),.17f);
                for(int j=0;j<6;j++) { var step=Box("Trail chevron",zone,new Vector3(0,.07f,-16+j*6),new Vector3(.34f,.04f,.34f),Color.white,false,true); step.transform.rotation=Quaternion.Euler(0,45,0); }
            }
            Box("Gateway rear boundary",Zones[0],new Vector3(0,3,-22),new Vector3(34,6,.5f),grass);
            MechanismWorld.Build(this);
            Effects["rescue"]=()=>StartCoroutine(Rescue());
            // A ring of distant low-poly peaks frames the linear expedition.
            for(int i=0;i<20;i++) for(int s=-1;s<=1;s+=2) Cone(transform,new Vector3(s*(35+UnityEngine.Random.value*20),-9,i*18-20),10+UnityEngine.Random.value*12,20+UnityEngine.Random.value*20,new Color(.38f,.56f,.61f));
            StorybookEnvironment.Enhance(this);
        }
        public InteractableBase Find(string id) => Interactables.Find(x=>x.objectiveId==id && (x.Mechanism==null || x.Operation==MechanismAction.Drive) && x.action!=ActionKind.UnsafePlace);
        InteractableBase Task(int zone,string id,string title,Vector3 p,string description,RobotKind robot=RobotKind.None,ActionKind action=ActionKind.Activate,bool inspect=false)
        {
            var root=new GameObject(title).transform; root.SetParent(Zones[zone],false); root.localPosition=p;
            var item=root.gameObject.AddComponent<InteractableBase>(); item.objectiveId=id; item.region=zone; item.displayName=title; item.inspection=description; item.requiredRobot=robot; item.action=action; item.needsInspection=inspect;
            var trigger=root.gameObject.AddComponent<SphereCollider>(); trigger.radius=.65f; trigger.isTrigger=true;
            Color c=action==ActionKind.UnsafePlace?Pink:robot==RobotKind.None?Gold:robot==RobotKind.Petalo?Pink:robot==RobotKind.Quacky?Gold:robot==RobotKind.Tolly?Cyan:Mint;
            Cylinder("Interaction pedestal",root,new Vector3(0,-p.y+.14f,0),new Vector3(1.4f,.12f,1.4f),Ink,false);
            Gem(root,Vector3.up*.2f,c,.36f);
            Label(root,title,new Vector3(0,1.05f,0),.14f);
            Interactables.Add(item); return item;
        }
        Transform Pad(int zone,Vector3 p,string label,bool safe=true)
        {
            var color=safe?Mint:Pink; Box(label,Zones[zone],p,new Vector3(3.4f,.13f,3.4f),color,false,true);
            for(int s=-1;s<=1;s+=2) Box("Pad edge",Zones[zone],p+new Vector3(s*1.7f,.08f,0),new Vector3(.12f,.14f,3.5f),Color.white,false);
            Label(Zones[zone],label,p+new Vector3(0,.5f,1.7f),.17f,color);
            var anchor=new GameObject(label+" snap point").transform; anchor.SetParent(Zones[zone],false); anchor.localPosition=p+Vector3.up*.6f; return anchor;
        }
        void Placement(int zone,string id,Vector3 at,InteractableBase source,string title)
        {
            var anchor=Pad(zone,new Vector3(at.x,.08f,at.z),"SAFE PLACEMENT");
            var place=Task(zone,id,title,at,"The cyan foundation supports this load without harming the habitat.",RobotKind.Tiko,ActionKind.Place); place.source=source; place.destination=anchor;
        }
        void Protected(int zone,Vector3 at)
        {
            Pad(zone,new Vector3(at.x,.06f,at.z),"PROTECTED PLANTS",false);
            for(int j=0;j<7;j++) Flower(Zones[zone],new Vector3(at.x+UnityEngine.Random.Range(-1.2f,1.2f),.12f,at.z+UnityEngine.Random.Range(-1.2f,1.2f)),Pink);
            Task(zone,"unsafe_"+zone,"Protected habitat",at,"Heavy loads crush the roots. Choose the cyan safe area.",RobotKind.Tiko,ActionKind.UnsafePlace);
        }
        void Gateway()
        {
            var z=Zones[0];
            for(int s=-1;s<=1;s+=2) { Box("Fork route",z,new Vector3(s*4,.05f,-5),new Vector3(3,.08f,13),sand,false); Box("Monument",z,new Vector3(s*10,4,10),new Vector3(2.6f,8,2.6f),stone); Gem(z,new Vector3(s*10,8.8f,10),Mint,1.3f); }
            Task(0,"route","River route",new Vector3(-4,1,-6),"A river route teaches balance and careful observation. Both routes meet beyond the gate.",RobotKind.None,ActionKind.Activate,true);
            Task(0,"route","Woodland route",new Vector3(4,1,-6),"The woodland route teaches how to protect plants. Both routes join the forest expedition.",RobotKind.None,ActionKind.Activate,true);
            Task(0,"gateway","Tolly's checkpoint",new Vector3(0,1,12),"Tolly operates access systems. Select [3] to lift the Grand Gateway.",RobotKind.Tolly);
            Label(z,"DISCOVERY WORLD",new Vector3(0,7,20),.48f,Gold);
            Effects["route"]=()=> { foreach(var item in Interactables) if(item.objectiveId=="route") item.done=true; };
        }
        void Forest()
        {
            var z=Zones[1];
            var branch=Cylinder("Fallen branch",z,new Vector3(0,.8f,-5),new Vector3(1,3.7f,1),new Color(.39f,.25f,.18f)); branch.transform.rotation=Quaternion.Euler(0,0,82);
            // The branch is carried by script; remove solid geometry while held to keep placement reliable.
            Destroy(branch.GetComponent<Collider>());
            var pick=Task(1,"branch","Fallen branch",new Vector3(0,1,-7),"The branch blocks the trail. Tiko can lift it. Place it on cyan soil, away from the pink plants.",RobotKind.Tiko,ActionKind.Carry,true); pick.movable=branch.transform; pick.originalPosition=branch.transform.position;
            Placement(1,"branch_place",new Vector3(-5,1,0),pick,"Safe woodland clearing"); Protected(1,new Vector3(5,1,0));
            var sign=Box("Fallen route sign",z,new Vector3(0,.6f,11),new Vector3(2.2f,1.1f,.22f),Gold,false); sign.transform.rotation=Quaternion.Euler(0,0,28);
            Task(1,"forest_marker","Restore route marker",new Vector3(0,1,10),"The restored marker guides explorers away from delicate plants.");
            Effects["forest_marker"]=()=> { sign.transform.rotation=Quaternion.identity; sign.transform.localPosition=new Vector3(0,2,11); };
        }
        void Bridge()
        {
            var z=Zones[2];
            Color[] rainbow={Pink,new Color(1,.58f,.3f),Gold,Mint,Cyan,new Color(.62f,.49f,.9f)};
            for(int i=0;i<9;i++) { if(i==4) continue; Box("Rainbow deck",z,new Vector3(0,.05f,-5+i),new Vector3(5,.3f,.94f),rainbow[i%6]); }
            var missing=Box("Broken bridge section",z,new Vector3(0,-3,-1),new Vector3(5,.3f,1),Gold); missing.transform.rotation=Quaternion.Euler(0,0,16);
            for(int s=-1;s<=1;s+=2) { Box("Bridge rail",z,new Vector3(s*2.6f,1,-1),new Vector3(.12f,.18f,9),Color.white); for(int j=0;j<3;j++) Box("Rail post",z,new Vector3(s*2.6f,.55f,-5+j*4),new Vector3(.18f,1.1f,.18f),Color.white); }
            Task(2,"bridge_inspect","Survey the bridge",new Vector3(0,1,-10),"The deck needs a stable support. Find the heavy block, align it at the cyan socket, then deliver the repair kit.",RobotKind.None,ActionKind.Activate,true);
            var block=Box("Support block",z,new Vector3(-6,.65f,-9),new Vector3(1.6f,1.3f,1.6f),stone,false);
            var pick=Task(2,"support","Bridge support",new Vector3(-6,1,-10),"A broad support spreads the weight of the bridge.",RobotKind.Tiko,ActionKind.Carry); pick.movable=block.transform; pick.originalPosition=block.transform.position;
            Placement(2,"support_place",new Vector3(-1,1,-6.5f),pick,"Bridge alignment socket");
            Effects["support_place"]=()=> { missing.transform.localPosition=new Vector3(0,.05f,-1); missing.transform.rotation=Quaternion.identity; AdventureGame.Instance.Audio.Play("bridge repair"); };
            var parcel=Box("Bridge repair kit",z,new Vector3(5,.7f,-7),Vector3.one*.65f,Gold,false);
            var supply=Task(2,"bridge_supply","Deliver repair kit",new Vector3(5,1,-9),"Quacky can carry small supplies across the aligned deck.",RobotKind.Quacky); supply.movable=parcel.transform; supply.destination=Pad(2,new Vector3(1,.1f,7),"SUPPLY DROP");
            Task(2,"bridge_cross","Crossing checkpoint",new Vector3(0,1,12),"You've crossed a balanced bridge. Check in to unlock Castle Place.");
        }
        void Castle()
        {
            var z=Zones[3];
            for(int s=-1;s<=1;s+=2) { Cylinder("Castle tower",z,new Vector3(s*11,4,7),new Vector3(5,4,5),stone); Cone(z,new Vector3(s*11,8,7),3.4f,4,new Color(.37f,.36f,.56f)); Box("Courtyard wall",z,new Vector3(s*10,2,-5),new Vector3(13,4,.8f),stone); }
            var door=Box("Castle checkpoint gate",z,new Vector3(0,2,-5),new Vector3(7,4,.5f),Ink);
            Task(3,"castle_gate","Castle checkpoint",new Vector3(0,1,-10),"Tolly verifies access and opens the courtyard.",RobotKind.Tolly); Effects["castle_gate"]=()=>door.SetActive(false);
            var crystal=Gem(z,new Vector3(-4,1.6f,0),Cyan,.9f);
            Task(3,"crystal","Energy crystal",new Vector3(-4,1,-1),"This crystal stores energy. Collect it and load the tower's delivery container."); Effects["crystal"]=()=>crystal.SetActive(false);
            var container=Box("Crystal container",z,new Vector3(3,.6f,2),new Vector3(1.3f,1.2f,1.3f),Gold,false);
            Task(3,"container_load","Load delivery container",new Vector3(3,1,1),"Place the collected crystal inside the sturdy delivery container."); Effects["container_load"]=()=>container.GetComponent<Renderer>().sharedMaterial=Mat(Cyan,true);
            var pick=Task(3,"container","Lift loaded container",new Vector3(3,1,1),"Tiko carries the loaded container onto the cyan lift foundation.",RobotKind.Tiko,ActionKind.Carry); pick.movable=container.transform; pick.originalPosition=container.transform.position;
            var platform=Box("Tower elevator",z,new Vector3(5,.2f,7),new Vector3(3.7f,.4f,3.7f),Gold);
            Box("Lift guide",z,new Vector3(7,3.5f,7),new Vector3(.35f,7,.35f),Ink);
            Placement(3,"lift_load",new Vector3(5,1,5.5f),pick,"Tower lift platform");
            Effects["lift_load"]=()=>StartCoroutine(RaiseLift(platform.transform,container.transform));
            Task(3,"castle_light","Tower light receiver",new Vector3(0,1,13),"The lift powers this photosensitive receiver. Petalo's light completes the circuit.",RobotKind.Petalo);
            Effects["castle_light"]=()=>AdventureGame.Instance.Audio.Play("crystal activation");
        }
        IEnumerator RaiseLift(Transform platform,Transform cargo)
        {
            cargo.SetParent(platform,true); yield return InteractionManager.Move(platform,platform.position+Vector3.up*5,1.4f);
        }
        void Cave()
        {
            var z=Zones[4];
            Box("Cavern roof",z,new Vector3(0,9,1),new Vector3(34,2,35),stone*.4f);
            for(int s=-1;s<=1;s+=2) { Box("Cavern wall",z,new Vector3(s*10,4,0),new Vector3(4,8,35),stone*.45f); for(int j=0;j<6;j++) { Cone(z,new Vector3(s*7,0,-12+j*5),.7f,2.8f,new Color(.38f,.41f,.65f)); Gem(z,new Vector3(s*8,2,-10+j*5),new Color(.36f,.39f,.8f),.7f); } }
            var hidden=new GameObject("Hidden light markers"); hidden.transform.SetParent(z,false);
            for(int j=0;j<6;j++) Gem(hidden.transform,new Vector3(j%2==0?-2:2,.45f,-8+j*4),Cyan,.4f);
            hidden.SetActive(false);
            Task(4,"cave_reveal","Unlit marker stone",new Vector3(0,1,-12),"Light reveals reflective symbols. Petalo illuminates the route; Quacky fits the narrow side tunnel.",RobotKind.Petalo,ActionKind.Activate,true);
            Effects["cave_reveal"]=()=>hidden.SetActive(true);
            Beacon(4,"cave_one",new Vector3(-3,1,-5),"Signal beacon I");
            Box("Narrow tunnel wall",z,new Vector3(5,1.8f,1),new Vector3(.5f,3.6f,8),stone*.5f);
            Box("Tiny tunnel roof",z,new Vector3(6.5f,1.1f,1),new Vector3(2.5f,.35f,8),stone*.5f);
            var signal=Gem(z,new Vector3(6.5f,.55f,-2),Cyan,.45f);
            var q=Task(4,"cave_remote","Quacky's narrow passage",new Vector3(3.6f,1,-1),"Only Quacky fits. Retrieve the signal crystal from the passage to close the remote circuit.",RobotKind.Quacky); q.movable=signal.transform; q.destination=Pad(4,new Vector3(6.5f,.05f,4),"REMOTE SWITCH");
            Beacon(4,"cave_two",new Vector3(-3,1,6),"Signal beacon II"); Beacon(4,"cave_three",new Vector3(1,1,13),"Signal beacon III");
        }
        void Beacon(int zone,string id,Vector3 at,string title)
        {
            var bulb=Ball(title+" lens",Zones[zone],at+Vector3.up*1.5f,Vector3.one*.9f,stone);
            Cylinder("Beacon tower",Zones[zone],new Vector3(at.x,1,at.z),new Vector3(.35f,1,.35f),stone,false);
            var light=bulb.AddComponent<Light>(); light.type=LightType.Point; light.color=Mint; light.range=10; light.intensity=0;
            Task(zone,id,title,at,"Petalo's light activates this photosensitive beacon.",RobotKind.Petalo);
            Effects[id]=()=> { bulb.GetComponent<Renderer>().sharedMaterial=Mat(Mint,true); light.intensity=2; AdventureGame.Instance.Audio.Play("beacon"); };
        }
        void Garden()
        {
            var z=Zones[5];
            for(int i=0;i<35;i++) { float x=UnityEngine.Random.Range(7,14)*(i%2==0?1:-1); Flower(z,new Vector3(x,0,UnityEngine.Random.Range(-17,17)),i%2==0?Pink:Gold,1.6f); }
            for(int i=0;i<4;i++) { float x=i%2==0?-9:9; float zz=-8+i*6; Cylinder("Fairy cottage",z,new Vector3(x,.8f,zz),new Vector3(1.6f,.8f,1.6f),new Color(.89f,.8f,.68f)); Cone(z,new Vector3(x,1.5f,zz),1.4f,1.5f,Pink); }
            var parcel=Box("Garden path kit",z,new Vector3(3,.6f,-9),Vector3.one*.65f,Gold,false);
            var delivery=Task(5,"garden_supply","Small garden delivery",new Vector3(3,1,-10),"Quacky delivers the tiny parts without disturbing flower beds.",RobotKind.Quacky); delivery.movable=parcel.transform; delivery.destination=Pad(5,new Vector3(6,.08f,-5),"SMALL DELIVERY");
            var block=Box("Construction stone",z,new Vector3(-4,.6f,-4),new Vector3(2,1.2f,2),stone,false);
            var pick=Task(5,"stone","Heavy construction stone",new Vector3(-4,1,-5),"Build on the cyan foundation. Pink areas protect fragile plant roots.",RobotKind.Tiko,ActionKind.Carry,true); pick.movable=block.transform; pick.originalPosition=block.transform.position;
            Box("Broken garden trail",z,new Vector3(0,.08f,4),new Vector3(5.4f,.1f,4),Ink,false);
            Placement(5,"stone_place",new Vector3(0,1,4),pick,"Garden foundation"); Protected(5,new Vector3(5,1,3));
            Effects["stone_place"]=()=> { block.transform.localPosition=new Vector3(0,.12f,4); block.transform.localScale=new Vector3(5.4f,.2f,4); };
            Task(5,"garden_marker","Restored flower path",new Vector3(0,1,12),"A safe route protects the garden and leads to the valley.");
        }
        void Valley()
        {
            var z=Zones[6];
            for(int s=-1;s<=1;s+=2) { Cone(z,new Vector3(s*14,0,15),6,20,new Color(.31f,.4f,.48f)); Box("Valley machinery",z,new Vector3(s*10,1,-6),new Vector3(3,2,8),Ink); }
            Dragon=StorybookAssets.Dragon(z,new Vector3(0,.1f,14));
            FinalBarrier=Box("Dragon safety barrier",z,new Vector3(0,2.6f,8),new Vector3(15,5.2f,.3f),Ink).transform;
            FinalBarrier.GetComponent<Renderer>().enabled=false;
            for(int i=0;i<7;i++) Box("Barrier energy stripe",FinalBarrier,new Vector3((i-3)*.13f,0,-.55f),new Vector3(.015f,.9f,.15f),Cyan,false,true);
            var station=Pad(6,new Vector3(-8,.1f,-2),"ENERGY STATION");
            var parcel=Gem(z,new Vector3(-4,.8f,-12),Gold,.6f);
            var energy=Task(6,"energy","Energy delivery tunnel",new Vector3(-4,1,-12),"Quacky carries this crystal through the narrow machine passage to the energy station.",RobotKind.Quacky); energy.movable=parcel.transform; energy.destination=station;
            Box("Delivery tunnel canopy",z,new Vector3(-8,1.2f,-8),new Vector3(2,.4f,7),stone);
            Effects["energy"]=()=>Gem(z,new Vector3(-8,2,-2),Gold,1);
            var block=Box("Broken mechanical block",z,new Vector3(4,.6f,-9),new Vector3(1.5f,1.2f,1.5f),stone,false);
            var pick=Task(6,"mechanical_block","Mechanical block",new Vector3(4,1,-10),"Use Tiko to align this block with the cyan repair socket.",RobotKind.Tiko,ActionKind.Carry); pick.movable=block.transform; pick.originalPosition=block.transform.position;
            Placement(6,"mechanism",new Vector3(5,1,-4),pick,"Mechanical alignment socket");
            Effects["mechanism"]=()=>block.GetComponent<Renderer>().sharedMaterial=Mat(Mint,true);
            Beacon(6,"beacon",new Vector3(-4,1,2),"Valley beacon");
            Task(6,"access","Final access checkpoint",new Vector3(4,1,2),"Tolly enables the final restoration control. The barrier remains closed until the rescue.",RobotKind.Tolly);
            Task(6,"rescue","Restoration altar",new Vector3(0,1,5),"Four restored systems work together. Energy + mechanics + light + access = a free dragon.",RobotKind.None,ActionKind.Rescue);
            for(int i=0;i<8;i++) {
                var lamp=Ball("Restoration lantern",z,new Vector3(i%2==0?-6:6,3,-12+i*4),Vector3.one*.7f,Mint,true); lamp.SetActive(false); ValleyLights.Add(lamp);
            }
            Portal=new GameObject("Final discovery portal").transform; Portal.SetParent(z,false); Portal.localPosition=new Vector3(0,2,21);
            for(int i=0;i<12;i++) { float a=i*Mathf.PI/6; Gem(Portal,new Vector3(Mathf.Cos(a)*2.5f,Mathf.Sin(a)*2.5f,0),Mint,.65f); } Portal.gameObject.SetActive(false);
            Effects["beacon"]+=()=> { foreach(var lamp in ValleyLights) lamp.SetActive(true); };
            Effects["access"]=()=>Gem(z,new Vector3(4,2.5f,2),Gold,.6f);
            Effects["rescue"]=()=>StartCoroutine(Rescue());
        }
        public void Apply(string id) { if(Effects.TryGetValue(id,out var effect)) effect(); }
        public void OpenRegion(int finishedRegion,bool immediate=false)
        {
            if(finishedRegion<0 || finishedRegion>=Gates.Count) return;
            if(immediate) Gates[finishedRegion].gameObject.SetActive(false);
            else StartCoroutine(OpenGate(Gates[finishedRegion]));
        }
        IEnumerator OpenGate(Transform gate) { AdventureGame.Instance.Audio.Play("gate open"); yield return InteractionManager.Move(gate,gate.position+Vector3.down*6,1); gate.gameObject.SetActive(false); }
        public void RestoreCheckpoint()
        {
            var m=AdventureGame.Instance.Missions;
            for(int i=0;i<(m.Finished?7:m.Region);i++) Machines[i].Restore();
            if(m.Finished) { Restored=true; FinalBarrier.gameObject.SetActive(false); Portal.gameObject.SetActive(true); foreach(var light in ValleyLights) light.SetActive(true); }
            foreach(var item in Interactables) {
                if(!m.Completed.Contains(item.objectiveId)) continue;
                item.done=true; item.inspected=true;
                if(item.action==ActionKind.Place && item.source && item.source.movable && item.destination)
                    item.source.movable.position=item.destination.position;
                Apply(item.objectiveId);
            }
            for(int i=0;i<m.Region;i++) OpenRegion(i,true);
        }
        IEnumerator Rescue()
        {
            Restored=true; FinalBarrier.gameObject.SetActive(false); Portal.gameObject.SetActive(true);
            foreach(var l in ValleyLights) l.SetActive(true);
            var start=Dragon.position;
            for(float t=0;t<3;t+=Time.deltaTime) {
                Dragon.position=start+new Vector3(0,Mathf.Sin(t/3*Mathf.PI)*2,-t*1.6f);
                Dragon.rotation=Quaternion.Euler(0,Mathf.Sin(t*3)*6,0); yield return null;
            }
            for(int i=0;i<24;i++) Flower(Zones[6],new Vector3(UnityEngine.Random.Range(-7,7),0,UnityEngine.Random.Range(-12,8)),i%2==0?Pink:Gold,1.5f);
        }
    }
}
