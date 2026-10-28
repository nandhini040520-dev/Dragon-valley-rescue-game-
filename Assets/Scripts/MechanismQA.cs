using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace DragonValley
{
    // Opt-in -questQA. Uses real interactions; teleport is only test-fixture setup.
    // Critical bridge crossing and lift ride use CharacterController motion.
    public class MechanismQA : MonoBehaviour
    {
        AdventureGame g;
        string folder;
        int failures, errors;
        readonly List<string> results=new List<string>();
        void Awake() { Application.logMessageReceived+=Log; }
        void OnDestroy() { Application.logMessageReceived-=Log; }
        void Log(string text,string stack,LogType type) { if(type==LogType.Error || type==LogType.Exception) { errors++; Debug.LogWarning("PHASE2_ERROR: "+text); } }
        void Check(bool ok,string label) { if(!ok) failures++; results.Add((ok?"PASS ":"FAIL ")+label); Debug.Log(results[results.Count-1]); File.WriteAllLines(Path.Combine(folder,"acceptance-results.txt"),results); }
        IEnumerator Act(MechanismController m,MechanismAction op,bool expected=true)
        {
            var item=m.Controls[op]; var point=item.transform.position+Vector3.back*1.5f; point.y=item.transform.position.y-1;
            g.Player.Teleport(point); g.Player.CameraRig.Yaw=0; g.Player.CameraRig.Pitch=26;
            yield return null;
            bool accepted=g.Interactions.TryInteract(item); Check(accepted==expected,m.Region+" "+op+" accepted="+accepted);
        }
        IEnumerator Configure(MechanismController m,bool temporary=false)
        {
            m.Controls[MechanismAction.Install].Inspect();
            yield return Act(m,MechanismAction.Crystal); yield return Act(m,MechanismAction.Install);
            yield return Act(m,MechanismAction.Power); if(m.Kind!=MechanismKind.Gate) yield return Act(m,MechanismAction.Route);
            if(temporary) yield return Act(m,MechanismAction.Temporary);
            else if(m.UsesGears) { yield return Act(m,MechanismAction.Gear); yield return Act(m,MechanismAction.Gear); yield return Act(m,MechanismAction.Pin); }
            if(m.UsesWater) { yield return Act(m,MechanismAction.Blocker); yield return Act(m,MechanismAction.Valve); }
            if(m.UsesSignal) { yield return Act(m,MechanismAction.NodeA); yield return Act(m,MechanismAction.NodeB); yield return Act(m,MechanismAction.NodeB); }
            if(m.Kind==MechanismKind.Lift) { yield return Act(m,MechanismAction.Direction); yield return Act(m,MechanismAction.Floor); }
            if(m.Kind==MechanismKind.Restoration) yield return Act(m,MechanismAction.Beacon);
            yield return Act(m,MechanismAction.Unlock);
        }
        IEnumerator WaitDrive(MechanismController m)
        {
            float until=Time.realtimeSinceStartup+14;
            while((m.Progress<1 || m.Driving) && Time.realtimeSinceStartup<until) { g.Player.Tick(Vector2.zero,false,Time.deltaTime); yield return null; }
            Check(m.Progress==1 && !m.Driving,m.Region+" actuator reaches real output");
        }
        IEnumerator Walk(Vector3 to)
        {
            g.Player.CameraRig.Yaw=0; g.Player.CameraRig.Pitch=26; g.Player.CameraRig.Snap();
            float until=Time.realtimeSinceStartup+15;
            while(Time.realtimeSinceStartup<until) {
                var d=to-g.Player.transform.position; d.y=0; if(d.magnitude<.3f) break;
                g.Player.Tick(new Vector2(d.x,d.z).normalized*Mathf.Clamp01(d.magnitude*2),false,Time.deltaTime); yield return null;
            }
            var remaining=to-g.Player.transform.position; remaining.y=0;
            Check(remaining.magnitude<.6f,"Physical walk "+to+" actual "+g.Player.transform.position);
        }
        IEnumerator Capture(string name)
        {
            g.InspectionUntil=0; g.NoticeUntil=0; yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(folder,name+".png")); yield return null;
        }
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs(); int at=Array.IndexOf(args,"-qaFolder");
            folder=at>=0?args[at+1]:Path.GetFullPath(Path.Combine(Application.dataPath,"../../../QA/Phase2")); Directory.CreateDirectory(folder);
            g=AdventureGame.Instance; while(!UnityEngine.Rendering.SplashScreen.isFinished) yield return null;
            g.StartAdventure(false); Check(g.State==GameState.TUTORIAL,"New campaign opens new mechanism field guide"); g.CloseTutorial(); yield return null;
            for(int i=0;i<7;i++) {
                var m=g.World.Machines[i];
                Check(g.Missions.Region==i,"Campaign reaches region "+i);
                Check(m.Controls.Values.Count>7,"Multi-step machine has independent controls "+i);
                yield return Act(m,MechanismAction.Drive,false);
                Check(m.Progress==0,"Uninspected drive cannot move output "+i);
                m.Controls[MechanismAction.Install].Inspect();
                yield return Act(m,MechanismAction.Install,false);
                yield return Configure(m);
                Check(m.Ready,"Connected dependencies deliver output "+i);
                // Reverse a configured input: downstream output must turn off without losing upstream state.
                yield return Act(m,MechanismAction.Power); Check(!m.Ready && m.Components[0].Output,"Power loss propagates "+i);
                yield return Act(m,MechanismAction.Drive,false); yield return Act(m,MechanismAction.Power);
                if(m.UsesSignal) { yield return Act(m,MechanismAction.NodeB); Check(!m.Ready,"Wrong reflector blocks downstream"); yield return Act(m,MechanismAction.NodeB); yield return Act(m,MechanismAction.NodeB); }
                if(i==3) {
                    // Board before driving. The second lever is physically parented to the platform.
                    g.Player.Teleport(m.Output.position+Vector3.up*.21f);
                    var onboard=g.World.Interactables.Find(x=>x.displayName=="Onboard lift drive");
                    Check(g.Interactions.TryInteract(onboard),"Onboard lever starts lift");
                } else yield return Act(m,MechanismAction.Drive);
                yield return new WaitForSeconds(.6f); Check(m.Progress>0 && m.Progress<1,"Motion is gradual "+i);
                float before=m.Progress; g.TogglePause(); yield return new WaitForSecondsRealtime(.15f); Check(Mathf.Approximately(before,m.Progress),"Pause freezes machine "+i); g.TogglePause();
                yield return WaitDrive(m);
                if(i==3) {
                    Check(g.Player.transform.position.y>4,"Lift physically carries explorer"); yield return Walk(new Vector3(0,4.5f,3*44+4));
                    yield return Act(m,MechanismAction.Direction); Check(!m.Ready,"Mismatched lift direction and floor blocked");
                    yield return Act(m,MechanismAction.Floor); yield return Act(m,MechanismAction.Drive);
                    float limit=Time.realtimeSinceStartup+9; while(m.Driving && Time.realtimeSinceStartup<limit) yield return null;
                    Check(m.Progress==0 && !m.Driving,"Lift can be recalled after falling off");
                    yield return Act(m,MechanismAction.Direction); yield return Act(m,MechanismAction.Floor);
                    g.Player.Teleport(m.Output.position+Vector3.up*.21f);
                    var onboard=g.World.Interactables.Find(x=>x.displayName=="Onboard lift drive");
                    Check(g.Interactions.TryInteract(onboard),"Recalled lift can be ridden again"); yield return WaitDrive(m);
                }
                if(i==2) {
                    Check(!m.SafeOutput,"Unlocked permanent bridge is unsafe"); yield return Act(m,MechanismAction.Unlock);
                    g.Player.Teleport(new Vector3(0,.2f,2*44-7)); yield return Walk(new Vector3(0,.2f,2*44+10));
                    Check(g.Player.transform.position.y>-.5f,"Bridge collider supports real crossing");
                }
                g.Player.Teleport(new Vector3(-1,.2f,i*44-10)); g.Player.CameraRig.Snap(); yield return Capture("machine-"+i);
                if(i==0) { g.ShowJournal=true; yield return Capture("information-tab"); g.ShowJournal=false; }
                yield return Act(m,MechanismAction.Confirm); Check(m.Completed,"Physical verification completes machine "+i);
                if(!m.Completed) { results.Add("RESULT: FAILED - stopped at region "+i); File.WriteAllLines(Path.Combine(folder,"acceptance-results.txt"),results); Application.Quit(1); yield break; }
                if(i==0) { g.StartAdventure(true); yield return null; Check(g.Missions.Region==1 && g.World.Machines[0].Completed,"Checkpoint reload restores linked outputs"); }
            }
            yield return new WaitForSeconds(4);
            Check(g.State==GameState.GAME_COMPLETE && g.World.Restored,"Restoration chain reaches dragon rescue"); yield return Capture("victory");
            int saved=g.Missions.ExplorerTokens;
            g.ReplayChallenge(); yield return null; var bridge=g.World.Machines[2];
            yield return Configure(bridge,true); yield return Act(bridge,MechanismAction.Drive); yield return WaitDrive(bridge);
            Check(!bridge.PinInstalled && bridge.Alignment==0 && bridge.SafeOutput,"Bypass is a genuinely different valid solution");
            bridge.TemporaryRemaining=.15f; yield return new WaitForSeconds(.3f);
            Check(bridge.TimedOut && bridge.Progress==0 && !bridge.SafeOutput,"Expired bypass retracts bridge and rejects completion");
            yield return Act(bridge,MechanismAction.Temporary); yield return Act(bridge,MechanismAction.Temporary);
            yield return Act(bridge,MechanismAction.Drive); yield return WaitDrive(bridge);
            yield return Act(bridge,MechanismAction.Confirm); Check(g.Missions.Finished && g.Missions.Replay,"Replay challenge records result");
            g.StartAdventure(true); yield return null;
            Check(g.Missions.Finished && !g.Missions.Replay && g.Missions.ExplorerTokens==saved,"Replay preserves campaign save");
            Check(errors==0,"No runtime errors"); results.Add(failures==0?"RESULT: PASSED":"RESULT: FAILED ("+failures+")");
            File.WriteAllLines(Path.Combine(folder,"acceptance-results.txt"),results); Application.Quit(failures==0?0:1);
        }
    }
}
