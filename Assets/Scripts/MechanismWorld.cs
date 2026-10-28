using UnityEngine;
using static DragonValley.WorldArt;
namespace DragonValley
{
    // Authored machines reuse one linked-state controller but have distinct physical outputs.
    public static class MechanismWorld
    {
        public static readonly string[] Titles={"Ancient Gate Control","Forest Water Restoration","Rainbow Bridge Machine","Castle Vertical Transport","Cave Signal Network","Garden Irrigation","Valley Restoration Core"};
        public static void Build(AdventureWorld world)
        {
            for(int i=0;i<7;i++) {
                var zone=world.Zones[i]; var root=new GameObject(Titles[i]).transform; root.SetParent(zone,false); root.localPosition=new Vector3(0,0,-8);
                var m=root.gameObject.AddComponent<MechanismController>(); m.Region=i; m.Kind=(MechanismKind)i; m.Title=Titles[i]; world.Machines[i]=m;
                m.Initialize();
                // Inputs are spaced along the accessible near bank, before the output.
                Add(world,m,MechanismAction.Crystal,"Loose power crystal",new Vector3(-5,1,-17),"Pick up the crystal yourself; then fit it into the socket.");
                Add(world,m,MechanismAction.Install,"Power crystal socket",new Vector3(-3,1,-11),"The source feeds a switch, cable, transmission and actuator.");
                Add(world,m,MechanismAction.Power,"Power switch",new Vector3(0,1,-11),"E toggles power. Removing power stops downstream motion.");
                Add(world,m,MechanismAction.Route,i==0?"Route selector: left / right":"Energy router: drain / machine",new Vector3(3,1,-11),i==0?"Select LEFT (0) for an optional clue detour or RIGHT (1) for the direct route.":"Route 0 drains power; route 1 follows the brass cable into the machine.");
                Add(world,m,MechanismAction.Unlock,"Safety brake",new Vector3(-3,1,-7),"Stop the drive before engaging or releasing the brake. Bridge must be locked before crossing.");
                Add(world,m,MechanismAction.Drive,"Drive lever",new Vector3(0,1,-7),"E starts or stops the actuator. All upstream dependencies must work.");
                Add(world,m,MechanismAction.Clue,"Service inscription",new Vector3(7,1,-17),"Optional clue: inspect for matching service marks and exploration credit.");
                if(m.UsesGears) {
                    Add(world,m,MechanismAction.Gear,"Gear alignment dial",new Vector3(-6,1,-9),"Rotate 0 / I / II. Two brass marks identify the meshing position.");
                    Add(world,m,MechanismAction.Pin,"Coupling pin",new Vector3(6,1,-14),"Collect and fit the missing coupling pin to transmit torque.");
                }
                if(m.UsesWater) {
                    Add(world,m,MechanismAction.Blocker,"Channel obstruction",new Vector3(-6,1,-9),"Remove debris from the intake before opening the sluice.");
                    Add(world,m,MechanismAction.Valve,"Restoration valve",new Vector3(6,1,-9),"Open the valve. The wrong router sends water to the drainage channel.");
                    Box("Irrigation channel",zone,new Vector3(4,.12f,1),new Vector3(1,.12f,14),Ink,false);
                    Box("Drain channel",zone,new Vector3(7,.12f,1),new Vector3(1,.12f,14),Pink,false);
                    m.Obstruction=Box("Movable intake debris",zone,new Vector3(4,.6f,-5),new Vector3(1,.9f,.6f),Gold,false).transform;
                    m.ValveHandle=Box("Valve wheel pointer",zone,new Vector3(6,1.5f,-9),new Vector3(.8f,.12f,.12f),Gold,false).transform;
                    for(int n=0;n<10;n++) m.Flow.Add(Ball("Moving water",zone,new Vector3(4,.3f,n),new Vector3(.6f,.18f,.5f),Cyan,true).transform);
                    for(int n=0;n<6;n++) Flower(zone,new Vector3(5.2f,0,n*2-4),Pink,1.4f);
                }
                if(m.UsesSignal) {
                    Add(world,m,MechanismAction.NodeA,"Reflector I",new Vector3(-6,1,-5),"Rotate the upstream reflector. Its housing carries one brass mark.");
                    Add(world,m,MechanismAction.NodeB,"Reflector II",new Vector3(6,1,-5),"Rotate the downstream reflector. Its housing carries two brass marks.");
                    m.ReflectorA=Box("Upstream reflector",zone,new Vector3(-6,2,-5),new Vector3(.8f,.8f,.15f),Cyan,false).transform;
                    m.ReflectorB=Box("Downstream reflector",zone,new Vector3(6,2,-5),new Vector3(.8f,.8f,.15f),Cyan,false).transform;
                    Label(zone,"I",new Vector3(-6,2.8f,-5),.2f,Gold); Label(zone,"II",new Vector3(6,2.8f,-5),.2f,Gold);
                    for(int n=0;n<4;n++) m.SignalPulses.Add(Ball("Traveling signal pulse",zone,Vector3.zero,Vector3.one*.23f,Cyan,true).transform);
                }
                if(i==2) Add(world,m,MechanismAction.Temporary,"Hydraulic bypass (25 seconds)",new Vector3(6,1,-7),"Alternative solution: no gear or pin needed, but pressure expires after 25 seconds. E toggles bypass.");
                if(i==3) {
                    Add(world,m,MechanismAction.Direction,"Lift direction: down / up",new Vector3(6,1,-7),"UP + floor 1 raises the platform. DOWN + floor 0 recalls it if you fall off.");
                    Add(world,m,MechanismAction.Floor,"Lift floor: 0 / 1",new Vector3(6,1,-4),"Match the destination to direction: UP / 1 or DOWN / 0. Board before driving upwards.");
                }
                if(i==6) Add(world,m,MechanismAction.Beacon,"Valley beacon enable",new Vector3(6,1,-2),"The beacon receives power only after both signal reflectors are aligned.");
                BuildChain(m,zone);
                BuildOutput(world,m,zone);
                m.Refresh();
            }
        }
        static void Add(AdventureWorld w,MechanismController m,MechanismAction action,string title,Vector3 p,string description)
        {
            var go=new GameObject(title); go.transform.SetParent(w.Zones[m.Region],false); go.transform.localPosition=p;
            var item=go.AddComponent<InteractableBase>(); item.region=m.Region; item.objectiveId=m.Prefix; item.displayName=title; item.inspection=description;
            item.Mechanism=m; item.Operation=action; m.Controls[action]=item; w.Interactables.Add(item);
            Cylinder("Control plinth",go.transform,new Vector3(0,-.55f,0),new Vector3(.6f,.4f,.6f),Ink,false);
            var knob=Gem(go.transform,Vector3.zero,action==MechanismAction.Confirm?Mint:Gold,.32f);
            if(action==MechanismAction.Power) m.SwitchHandle=knob.transform;
            Label(go.transform,title,new Vector3(0,.65f,0),.11f);
            if(action==MechanismAction.Gear) Label(go.transform,"ALIGN: II",new Vector3(0,1.1f,0),.13f,Mint);
        }
        static void BuildChain(MechanismController m,Transform zone)
        {
            // Visible cable stages illuminate sequentially after their upstream conditions pass.
            for(int n=0;n<6;n++) {
                var p=new Vector3(-5+n*2,2.7f,-3);
                var light=Ball(m.Components[n].Name,zone,p,Vector3.one*.42f,Ink,true);
                m.Indicators.Add(light.GetComponent<Renderer>());
                Label(zone,m.Components[n].Name,p+Vector3.up*.65f,.10f);
                if(n<5) StorybookAssets.Rod(zone,"Connected brass cable",p,p+Vector3.right*2,.07f,Gold);
            }
            for(int n=0;n<3;n++) {
                var gear=new GameObject("Transmission gear "+n).transform; gear.SetParent(zone,false); gear.localPosition=new Vector3(-4+n*1.2f,1.4f,-3);
                StorybookAssets.Ring(gear,"Gear rim",Vector3.zero,.54f,.12f,Gold);
                for(int tooth=0;tooth<10;tooth++) {
                    float a=tooth*Mathf.PI/5;
                    var t=Box("Gear tooth",gear,new Vector3(Mathf.Cos(a)*.58f,Mathf.Sin(a)*.58f,0),new Vector3(.22f,.22f,.28f),Gold,false);
                    t.transform.localRotation=Quaternion.Euler(0,0,a*Mathf.Rad2Deg);
                }
                m.Gears.Add(gear); if(n==0) m.Dial=gear;
            }
            var shaft=new GameObject("Rotating output shaft").transform; shaft.SetParent(zone,false); shaft.localPosition=new Vector3(.4f,1.4f,-3);
            Box("Shaft axle",shaft,Vector3.zero,new Vector3(3.8f,.14f,.14f),Cyan,false);
            Box("Shaft rotation marker",shaft,new Vector3(.8f,.16f,0),new Vector3(.35f,.16f,.14f),Gold,false); m.Gears.Add(shaft);
            var lockRoot=new GameObject("Actuator brake").transform; lockRoot.SetParent(zone,false); lockRoot.localPosition=new Vector3(3,0,-3);
            m.LockPin=Box("Sliding lock pin",lockRoot,new Vector3(0,1,0),new Vector3(1.2f,.22f,.22f),Pink,false).transform;
        }
        static void BuildOutput(AdventureWorld w,MechanismController m,Transform z)
        {
            Vector3 confirm=new Vector3(0,1,12);
            if(m.Kind==MechanismKind.Bridge) {
                m.Output=Box("Movable rainbow bridge deck",z,new Vector3(0,-3,-1),new Vector3(5,.3f,9),Gold).transform;
                m.OutputClosed=m.Output.localPosition; m.OutputOpen=new Vector3(0,.05f,-1);
                for(int i=0;i<9;i++) Box("Rainbow slat",m.Output,new Vector3(0,.53f,-.44f+i*.11f),new Vector3(.99f,.06f,.105f),Color.HSVToRGB(i/9f,.6f,1),false);
            } else if(m.Kind==MechanismKind.Lift) {
                m.Output=Box("Rideable lift platform",z,new Vector3(0,.2f,0),new Vector3(3.8f,.4f,3.8f),Gold).transform;
                m.OutputClosed=m.Output.localPosition; m.OutputOpen=new Vector3(0,4.2f,0);
                Box("Upper landing",z,new Vector3(0,4.1f,4),new Vector3(4,.4f,4),Ink);
                m.Counterweight=Box("Counterweight",z,new Vector3(7,5,0),new Vector3(1,1.6f,1),Gold,false).transform;
                StorybookAssets.Rod(z,"Lift guide left",new Vector3(-2,0,0),new Vector3(-2,7,0),.1f,Gold);
                StorybookAssets.Rod(z,"Lift guide right",new Vector3(2,0,0),new Vector3(2,7,0),.1f,Gold);
                confirm=new Vector3(0,5.2f,4);
                // A second drive lever on the platform lets the player board before starting.
                var board=new GameObject("Lift onboard lever"); board.transform.SetParent(m.Output,false); board.transform.localPosition=new Vector3(.3f,2.5f,0);
                var b=board.AddComponent<InteractableBase>(); b.Mechanism=m; b.Operation=MechanismAction.Drive; b.region=m.Region; b.objectiveId=m.Prefix; b.displayName="Onboard lift drive"; b.inspection="Configure the machine, board the lift, then E to travel."; w.Interactables.Add(b);
            } else {
                m.Output=Box(m.UsesWater?"Water sluice gate":m.Kind==MechanismKind.Signal?"Signal receiver gate":m.Kind==MechanismKind.Restoration?"Restoration barrier":"Gate actuator",z,new Vector3(0,2,6),new Vector3(6,4,.4f),Ink).transform;
                m.OutputClosed=m.Output.localPosition; m.OutputOpen=m.OutputClosed+Vector3.up*5;
                if(m.Kind==MechanismKind.Gate) {
                    m.RouteDoors=new Transform[2];
                    var routes=new GameObject("Two destination routes").transform; routes.SetParent(z,false); routes.localPosition=new Vector3(0,0,9);
                    for(int n=0;n<2;n++) m.RouteDoors[n]=Box(n==0?"Woodland detour":"Direct river route",routes,new Vector3(n==0?-3:3,2,0),new Vector3(3,4,.4f),Gold).transform;
                    Box("Route center divider",routes,new Vector3(0,2,0),new Vector3(2.8f,4,.4f),Ink);
                    for(int side=-1;side<=1;side+=2) Box("Route side wall",routes,new Vector3(side*11,2,0),new Vector3(12,4,.4f),Ink);
                    Label(z,"LEFT: CLUE DETOUR / RIGHT: DIRECT",new Vector3(0,6,9),.16f);
                    Add(w,m,MechanismAction.Discovery,"Hidden engineer's journal",new Vector3(-8,1,14),"A sheltered woodland alcove holds an old journal. E records a hidden-area discovery.");
                    Box("Alcove shelter",z,new Vector3(-8,3,16),new Vector3(5,.25f,4),Gold,false);
                    confirm=new Vector3(3,1,14);
                }
            }
            Add(w,m,MechanismAction.Confirm,m.Kind==MechanismKind.Restoration?"Final restoration console":"Verify restored system",confirm,"Verify the actual output after all components have responded. Bridge must be locked and lift checkpoint is upstairs.");
            if(m.Kind==MechanismKind.Restoration) {
                w.Dragon=StorybookAssets.Dragon(z,new Vector3(0,.1f,17)); w.FinalBarrier=m.Output;
                w.Portal=new GameObject("Discovery portal").transform; w.Portal.SetParent(z,false); w.Portal.localPosition=new Vector3(0,2,21);
                StorybookAssets.Ring(w.Portal,"Restored portal",Vector3.zero,2,.2f,Mint); w.Portal.gameObject.SetActive(false);
                for(int n=0;n<8;n++) { var lamp=Ball("Valley restoration lamp",z,new Vector3(n%2==0?-6:6,3,-10+n*4),Vector3.one*.6f,Mint,true); lamp.SetActive(false); w.ValleyLights.Add(lamp); }
            }
        }
    }
}
