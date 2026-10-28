using System;
using System.Collections.Generic;
using UnityEngine;
namespace DragonValley
{
    public sealed class MechanismController : MonoBehaviour
    {
        public int Region;
        public MechanismKind Kind;
        public string Title;
        public readonly List<MechanismComponent> Components=new List<MechanismComponent>();
        public readonly Dictionary<MechanismAction,InteractableBase> Controls=new Dictionary<MechanismAction,InteractableBase>();
        public readonly List<Renderer> Indicators=new List<Renderer>();
        public readonly List<Transform> Gears=new List<Transform>();
        public readonly List<Transform> Flow=new List<Transform>();
        public readonly List<Transform> SignalPulses=new List<Transform>();
        public Transform Obstruction, ValveHandle, ReflectorA, ReflectorB;
        public Transform Output, Counterweight, LockPin, SwitchHandle, Dial;
        public Vector3 OutputClosed, OutputOpen;
        public Transform[] RouteDoors;
        public bool Inspected, Collected, Installed, PowerOn, PinInstalled, Unlocked, Driving, BlockRemoved, ValveOpen, BeaconOn, Completed;
        public bool OptionalClue, Temporary, TimedOut;
        public bool HiddenFound;
        public int Route, Alignment, NodeA, NodeB, Direction, Floor;
        public int Actions, Incorrect, Unnecessary;
        public float Progress, Elapsed, TemporaryRemaining;
        public MechanismState State { get; private set; }
        public event Action<MechanismState> StateChanged;
        public bool Ready => Components.Count>0 && Components[Components.Count-1].Output;
        public string Prefix => "machine_"+Region;
        public string Hint => !Inspected ? "Approach the machine and press F to inspect its connected parts." :
            !Collected ? "Find the loose power crystal beside the trail." : !Installed ? "Install the crystal in the empty socket." :
            !PowerOn ? "The source is installed. Close the power switch." : !RouteCorrect ? "Follow the brass cable: choose the working output, not the drain." :
            UsesGears && Alignment!=2 && !Temporary ? "Match the gear's pointer to the two brass marks on its housing." :
            UsesGears && !PinInstalled && !Temporary ? "A loose coupling pin waits on the opposite side of the trail." :
            UsesWater && !BlockRemoved ? "Clear the obstruction at the channel intake." :
            UsesWater && !ValveOpen ? "Open the restoration valve to let the water through." :
            UsesSignal && (NodeA!=1 || NodeB!=2) ? "Read the reflector marks: upstream I, downstream II." :
            Kind==MechanismKind.Lift && (Direction!=1 || Floor!=1) ? "Select UP and floor 1 to reach the elevated checkpoint." :
            Kind==MechanismKind.Restoration && !BeaconOn ? "Enable the beacon after the signal path is connected." :
            !Unlocked ? "Release the mechanical brake before driving the actuator." :
            Progress<1 ? "Operate the drive lever; watch the chain transmit motion. Keep power on." :
            Kind==MechanismKind.Bridge && Unlocked && !Temporary ? "Stop the drive and engage the safety lock before crossing." :
            "Reach the output checkpoint and verify the restored mechanism.";
        public bool UsesGears => Kind==MechanismKind.Gate || Kind==MechanismKind.Bridge || Kind==MechanismKind.Lift || Kind==MechanismKind.Restoration;
        public bool UsesWater => Kind==MechanismKind.Water || Kind==MechanismKind.Garden;
        public bool UsesSignal => Kind==MechanismKind.Signal || Kind==MechanismKind.Restoration;
        public bool RouteCorrect => Kind==MechanismKind.Gate || Route==1;
        public bool SafeOutput => Progress>=.999f && Ready && (!Temporary || TemporaryRemaining>0) &&
            (Kind!=MechanismKind.Bridge || Temporary || !Unlocked);
        public void Initialize()
        {
            foreach(string name in new[]{"SOURCE","SWITCH","CABLE / ROUTE","TRANSMISSION","SHAFT / SENSOR","ACTUATOR"}) {
                var c=new MechanismComponent(name); if(Components.Count>0) c.Connect(Components[Components.Count-1]); Components.Add(c);
            }
            Refresh();
        }
        public void Inspect(bool optional=false)
        {
            Inspected=true; if(optional) OptionalClue=true;
            AdventureGame.Instance.InspectedMechanism=this;
            AdventureGame.Instance.InspectionUntil=Time.unscaledTime+16;
            AdventureGame.Instance.Player.CameraRig.Focus(transform.position+new Vector3(0,2,0));
            AdventureGame.Instance.Notify(Title, optional ? "Service marks: gear II; reflector I then II; lift UP / floor 1. Bridge bypass lasts 25 seconds. Permanent repair needs a pin." : Hint);
        }
        bool Reject(string why)
        {
            if(AdventureGame.TestMode) Debug.Log("MECHANISM_REJECT "+Region+" "+why+" progress="+Progress+" driving="+Driving);
            Incorrect++; Driving=false; SetState(MechanismState.FAILED);
            AdventureGame.Instance.Notify("Machine stopped - adjust and retry",why,true); return false;
        }
        public bool Operate(MechanismAction action)
        {
            if(Completed) return false;
            Actions++;
            if(action==MechanismAction.Clue) { if(OptionalClue) Unnecessary++; Inspect(true); return true; }
            if(action==MechanismAction.Discovery) { if(HiddenFound) Unnecessary++; HiddenFound=true; AdventureGame.Instance.Notify("Woodland alcove discovered","The old engineer's journal rewards the longer route. Exploration bonus earned."); return true; }
            if(!Inspected) return Reject("Inspect with F before operating this machine.");
            switch(action) {
                case MechanismAction.Crystal:
                    if(Collected) { Unnecessary++; break; } Collected=true;
                    var crystal=Controls[action].transform.Find("Energy crystal"); if(crystal) crystal.gameObject.SetActive(false); break;
                case MechanismAction.Install: if(!Collected) return Reject("The socket needs a power crystal. Search beside the path."); if(Installed) Unnecessary++; Installed=true; break;
                case MechanismAction.Power: PowerOn=!PowerOn; break;
                case MechanismAction.Route: Route=(Route+1)%2; break;
                case MechanismAction.Gear: Alignment=(Alignment+1)%3; break;
                case MechanismAction.Pin: if(PinInstalled) Unnecessary++; PinInstalled=true; break;
                case MechanismAction.Unlock: if(Driving) return Reject("Stop the drive before changing the safety brake."); Unlocked=!Unlocked; break;
                case MechanismAction.Blocker: if(BlockRemoved) Unnecessary++; BlockRemoved=true; break;
                case MechanismAction.Valve: ValveOpen=!ValveOpen; break;
                case MechanismAction.Direction: Direction=1-Direction; break;
                case MechanismAction.Floor: Floor=1-Floor; break;
                case MechanismAction.NodeA: NodeA=(NodeA+1)%3; break;
                case MechanismAction.NodeB: NodeB=(NodeB+1)%3; break;
                case MechanismAction.Beacon: BeaconOn=!BeaconOn; break;
                case MechanismAction.Temporary:
                    if(Kind!=MechanismKind.Bridge) return false;
                    Temporary=!Temporary; TemporaryRemaining=Temporary?25:0; TimedOut=false;
                    AdventureGame.Instance.Notify("Bridge route choice",Temporary ? "Temporary hydraulic bypass: 25 seconds. Fewer parts, but cross before pressure runs out." : "Permanent gear drive selected. Align gear II and fit the coupling pin."); break;
                case MechanismAction.Drive:
                    Refresh();
                    if(Driving) { Driving=false; break; }
                    if(!Ready || !Unlocked) return Reject(Hint);
                    Driving=true; AdventureGame.Instance.Audio.Play("gate open"); break;
                case MechanismAction.Confirm:
                    Refresh(); if(!SafeOutput) return Reject("Output is not safe yet. "+Hint);
                    Completed=true; Driving=false; SetState(MechanismState.COMPLETED);
                    AdventureGame.Instance.Missions.CompleteMechanism(this);
                    foreach(var r in AdventureGame.Instance.Robots.Members) r.Celebrate();
                    if(Region==6) { AdventureGame.Instance.World.Apply("rescue"); AdventureGame.Instance.FinishAdventure(); }
                    return true;
            }
            Refresh(); AdventureGame.Instance.Audio.Play("interact"); return true;
        }
        public void Refresh()
        {
            if(Components.Count==0) return;
            // Constraints are evaluated independently of mission step. A wrong input
            // immediately removes downstream output; no objective flag can bypass it.
            Components[0].Configure(Installed);
            Components[1].Configure(PowerOn);
            Components[2].Configure(RouteCorrect);
            bool transmission=(UsesGears ? Temporary || (Alignment==2 && PinInstalled) : !UsesWater || BlockRemoved) && (!UsesSignal || NodeA==1);
            Components[3].Configure(transmission);
            bool secondary=(!UsesWater || ValveOpen) && (!UsesSignal || NodeB==2);
            Components[4].Configure(secondary);
            Components[5].Configure((Kind!=MechanismKind.Lift || Direction==Floor) && (Kind!=MechanismKind.Restoration || BeaconOn) && !TimedOut);
            if(!Ready) Driving=false;
            if(Completed) SetState(MechanismState.COMPLETED);
            else if(!Installed || !PowerOn) SetState(MechanismState.OFF);
            else if(!RouteCorrect) SetState(MechanismState.BLOCKED);
            else if(UsesGears && !Temporary && Alignment!=2 || UsesSignal && (NodeA!=1 || NodeB!=2)) SetState(MechanismState.MISALIGNED);
            else if(!Ready) SetState(TimedOut?MechanismState.FAILED:MechanismState.PARTIAL);
            else SetState(Driving?MechanismState.ACTIVE:MechanismState.POWERED);
            for(int i=0;i<Indicators.Count;i++) Indicators[i].sharedMaterial=WorldArt.Mat(Components[Mathf.Min(i,5)].Output?WorldArt.Mint:WorldArt.Ink,Components[Mathf.Min(i,5)].Output);
            if(SwitchHandle) SwitchHandle.localRotation=Quaternion.Euler(PowerOn?-35:35,0,0);
            if(Dial && !Driving) Dial.localRotation=Quaternion.Euler(0,0,Alignment*120);
            if(LockPin) LockPin.localPosition=new Vector3(Unlocked?1.5f:0,1,0);
            if(Obstruction) Obstruction.localPosition=new Vector3(BlockRemoved?6:4,.6f,-5);
            if(ValveHandle) ValveHandle.localRotation=Quaternion.Euler(0,0,ValveOpen?90:0);
            if(ReflectorA) ReflectorA.localRotation=Quaternion.Euler(0,NodeA*60,0);
            if(ReflectorB) ReflectorB.localRotation=Quaternion.Euler(0,NodeB*60,0);
        }
        void SetState(MechanismState value) { if(State==value) return; State=value; StateChanged?.Invoke(value); }
        void Update()
        {
            var g=AdventureGame.Instance; if(!g || !g.IsPlaying) return;
            if(Completed) { ApplyPose(); return; }
            if(Region!=g.Missions.Region) return;
            if(Inspected) Elapsed+=Time.deltaTime;
            if(Temporary && TemporaryRemaining>0) {
                TemporaryRemaining=Mathf.Max(0,TemporaryRemaining-Time.deltaTime);
                if(TemporaryRemaining==0) {
                    TimedOut=true; Driving=false; Progress=0;
                    var local=g.Player.transform.position-transform.parent.position;
                    if(local.z>-6 && local.z<5) g.Player.Teleport(g.World.Spawn(Region));
                    g.Notify("Bypass pressure exhausted","Bridge lowered safely. Recharge the bypass or make a permanent repair.",true); Refresh();
                }
            }
            if(Driving && Ready && Unlocked) {
                float destination=Kind==MechanismKind.Lift && Direction==0?0:1;
                Progress=Mathf.MoveTowards(Progress,destination,Time.deltaTime/(Kind==MechanismKind.Restoration?9:5));
                for(int i=0;i<Gears.Count;i++) {
                    if(i==3) Gears[i].Rotate(100*Time.deltaTime,0,0,Space.Self);
                    else Gears[i].Rotate(0,0,(i%2==0?1:-1)*100*Time.deltaTime,Space.Self);
                }
                if(Progress==destination) { Driving=false; Refresh(); g.Audio.Play("objective complete"); }
            }
            ApplyPose();
        }
        public void ApplyPose()
        {
            if(Output) {
                Vector3 previous=Output.position;
                Output.localPosition=Vector3.Lerp(OutputClosed,OutputOpen,Progress);
                if(Kind==MechanismKind.Lift) {
                    var p=AdventureGame.Instance.Player;
                    if(p && Mathf.Abs(p.transform.position.x-previous.x)<1.7f && Mathf.Abs(p.transform.position.z-previous.z)<1.7f && Mathf.Abs(p.transform.position.y-(previous.y+.2f))<.45f)
                        p.GetComponent<CharacterController>().Move(Output.position-previous);
                }
            }
            if(Counterweight) Counterweight.localPosition=new Vector3(7,5-Progress*4,0);
            if(RouteDoors!=null) for(int i=0;i<RouteDoors.Length;i++) RouteDoors[i].localPosition=new Vector3(i==0?-3:3,2+((Route==i)?Progress*5:0),0);
            for(int i=0;i<Flow.Count;i++) {
                bool flowing=Components[4].Output && Progress>0;
                Flow[i].gameObject.SetActive(flowing);
                if(flowing) Flow[i].localPosition=new Vector3(4,.3f,-5+Mathf.Repeat(Time.time*2+i*1.2f,12));
            }
            for(int i=0;i<SignalPulses.Count;i++) {
                bool active=Components[Mathf.Min(i+2,5)].Output;
                SignalPulses[i].gameObject.SetActive(active);
                if(active) SignalPulses[i].localPosition=new Vector3(-5+i*2+Mathf.Repeat(Time.time*1.8f,2),2.7f,-3);
            }
            if(Kind==MechanismKind.Restoration && Progress>0) {
                var lamps=AdventureGame.Instance.World.ValleyLights;
                for(int i=0;i<lamps.Count;i++) lamps[i].SetActive(Progress>(i+1)/10f);
            }
        }
        public void Restore()
        {
            Inspected=Collected=Installed=PowerOn=PinInstalled=BlockRemoved=ValveOpen=BeaconOn=true;
            Route=1; Alignment=NodeB=2; NodeA=Direction=Floor=1;
            Unlocked=false; Progress=1; Completed=true; Refresh(); ApplyPose();
        }
        public string StatusText()
        {
            string s=Title+"  /  "+State+"\n";
            foreach(var c in Components) s+=c.Name+": "+(c.Output?"CONNECTED":"OFF / BLOCKED")+"\n";
            s+="Route "+Route+"  |  Gear "+Alignment+"  |  Brake "+(Unlocked?"released":"engaged")+"\n";
            if(UsesSignal) s+="Reflectors "+NodeA+" / "+NodeB+"\n";
            if(Kind==MechanismKind.Lift) s+="Direction "+(Direction==1?"UP":"DOWN")+" / Floor "+Floor+"\n";
            if(Temporary) s+="BYPASS: "+Mathf.CeilToInt(TemporaryRemaining)+" seconds\n";
            return s+"Travel: "+Mathf.RoundToInt(Progress*100)+"%\n"+Hint;
        }
    }
}
