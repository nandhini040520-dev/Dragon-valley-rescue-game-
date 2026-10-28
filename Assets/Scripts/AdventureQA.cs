using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace DragonValley
{
    // Opt-in standalone acceptance run. It drives the same controller and interaction paths as play.
    public class AdventureQA : MonoBehaviour
    {
        readonly List<string> checks=new List<string>();
        string folder;
        AdventureGame g;
        int errors;
        bool failed;
        bool structureVisible;
        string structureText;
        void OnGUI()
        {
            if(!structureVisible || g==null) return;
            GUI.depth=-100;
            GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(Screen.width/1600f,Screen.height/900f,1));
            GUI.color=new Color(.035f,.065f,.10f,.98f); GUI.DrawTexture(new Rect(160,195,1280,550),Texture2D.whiteTexture); GUI.color=Color.white;
            var style=new GUIStyle(GUI.skin.label) { font=Resources.Load<Font>("ExpeditionSans"), fontSize=24, wordWrap=true };
            GUI.Label(new Rect(190,219,1190,62),"DEVELOPMENT VIEW / LIVE RUNTIME STRUCTURE",style);
            style.fontSize=20; GUI.Label(new Rect(190,295,1190,420),structureText,style);
        }
        void Awake() { Application.logMessageReceived+=Log; }
        void OnDestroy() { Application.logMessageReceived-=Log; }
        void Log(string message,string stack,LogType type) { if(type==LogType.Exception || type==LogType.Error) { errors++; if(folder!=null) File.AppendAllText(Path.Combine(folder,"runtime-errors.txt"),message+"\n"+stack+"\n"); } }
        void Check(bool pass,string label)
        {
            string result=(pass?"PASS ":"FAIL ")+label; checks.Add(result); Debug.Log("QUEST_QA "+result);
            if(!pass) failed=true;
            File.WriteAllLines(Path.Combine(folder,"acceptance-results.txt"),checks);
        }
        IEnumerator Start()
        {
            folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../QA")); Directory.CreateDirectory(folder); g=AdventureGame.Instance;
            while(!UnityEngine.Rendering.SplashScreen.isFinished) yield return null;
            yield return new WaitForSeconds(.4f); yield return Capture("01-main-menu");
            Check(g.State==GameState.MAIN_MENU,"Boot reaches main menu"); g.StartAdventure(false); yield return null;
            Check(g.State==GameState.TUTORIAL && g.TutorialPage==0,"New adventure opens controls tutorial");
            Check(!g.IsPlaying && Time.timeScale==0,"Tutorial freezes gameplay");
            Check(Cursor.lockState==CursorLockMode.None && Cursor.visible,"Tutorial frees cursor for its buttons");
            Check(!g.Interactions.TryInteract(g.World.Find("route")),"Tutorial prevents mission interactions");
            for(int i=0;i<4;i++) Check(Resources.Load<Texture2D>("Tutorial/"+(RobotKind)i)!=null,"Tutorial portrait loads for "+(RobotKind)i);
            yield return Capture("tutorial-01-controls");
            g.NextTutorial(); Check(g.TutorialPage==1,"Tutorial advances to robot guide");
            g.Robots.Select(RobotKind.Tiko); yield return Capture("tutorial-02-funobotz");
            g.NextTutorial(); Check(g.TutorialPage==2,"Tutorial explains first mission and safe placement");
            yield return Capture("tutorial-03-first-mission");
            g.PreviousTutorial(); Check(g.TutorialPage==1,"Tutorial back navigation works");
            g.NextTutorial(); g.NextTutorial();
            Check(g.IsPlaying && Time.timeScale==1 && g.Robots.Selected==RobotKind.Petalo,"Finishing tutorial restores gameplay and original robot");
            Check(g.Missions.Completed.Count==0 && g.Missions.ExplorerTokens==0,"Tutorial leaves mission progress untouched");
            g.TogglePause(); g.OpenTutorial();
            Check(g.State==GameState.TUTORIAL && g.TutorialPage==0,"Pause menu can reopen tutorial");
            g.CloseTutorial(); Check(g.State==GameState.PAUSED && Time.timeScale==0,"Closing replayed tutorial returns to pause");
            g.TogglePause();
            g.StartAdventure(false); g.CloseTutorial();
            Check(g.IsPlaying && Time.timeScale==1 && g.Missions.Step==0,"Skipping tutorial begins a fresh adventure");
            g.Missions.SaveProgress(); g.StartAdventure(true); yield return null;
            Check(g.IsPlaying && g.State!=GameState.TUTORIAL,"Continue enters saved expedition without tutorial");
            Check(g.Missions.Region==0 && g.Missions.Step==0,"Fresh adventure begins at Grand Gateway");
            yield return Capture("Snap02-Player-Start");
            var start=g.Player.transform.position;
            for(float t=0;t<.6f;t+=Time.deltaTime) { g.Player.Tick(Vector2.up,false,Time.deltaTime); yield return null; }
            Check(Vector3.Distance(start,g.Player.transform.position)>.5f,"CharacterController moves explorer");
            for(float t=0;t<.3f;t+=Time.deltaTime) { g.Player.Tick(Vector2.zero,false,Time.deltaTime); yield return null; }
            float ground=g.Player.transform.position.y; g.Player.Tick(Vector2.zero,true,Time.deltaTime);
            for(float t=0;t<.22f;t+=Time.deltaTime) { g.Player.Tick(Vector2.zero,false,Time.deltaTime); yield return null; }
            Check(g.Player.transform.position.y>ground+.1f,"Space jump simulation leaves ground");
            for(float t=0;t<1;t+=Time.deltaTime) { g.Player.Tick(Vector2.zero,false,Time.deltaTime); yield return null; }
            g.Player.CameraRig.Yaw=25; g.Player.CameraRig.Snap(); Check(g.Camera.transform.forward.x>.2f,"Camera orbits explorer"); g.Player.CameraRig.Yaw=0;
            for(int i=0;i<4;i++) { g.Robots.Select((RobotKind)i); Check(g.Robots.Selected==(RobotKind)i && g.Robots.Members[i].SelectionRing.activeSelf,"Select "+(RobotKind)i); }
            Check(!g.Missions.CanRescue && !g.Missions.CompleteObjective("rescue"),"Final rescue rejected before four systems");
            g.TogglePause(); Check(g.State==GameState.PAUSED && Time.timeScale==0,"Pause stops simulation"); g.TogglePause();
            int previousRegion=0; int guard=0; bool checkpointTested=false;
            while(!g.Missions.Finished && guard++<60 && !failed)
            {
                string id=g.Missions.CurrentObjective.id; var item=g.World.Find(id); Check(item!=null,"Scene reference "+id); if(!item) break;
                if(g.Missions.Region!=previousRegion)
                {
                    // Cross every unlocked physical gate along the connected trail.
                    if(previousRegion==2) yield return Walk(new Vector3(0,0,2*44+12));
                    yield return Walk(new Vector3(0,0,g.Player.transform.position.z));
                    yield return Walk(g.World.Spawn(g.Missions.Region));
                    Check(g.Player.transform.position.z>g.Missions.Region*44-18,"Walk into region "+(g.Missions.Region+1)); previousRegion=g.Missions.Region;
                }
                if(id=="bridge_cross") { yield return Walk(new Vector3(0,0,2*44-7)); yield return Walk(new Vector3(0,0,2*44+8)); }
                // Keep the bridge supply approach on the near bank.
                if(id=="bridge_supply") { yield return Walk(new Vector3(0,0,2*44-10)); }
                var approach=item.transform.position+Vector3.back*2.2f; approach.y=.1f;
                yield return Walk(approach);
                g.Player.CameraRig.Yaw=0; g.Player.CameraRig.Pitch=26; g.Player.CameraRig.Snap(); g.Detector.Refresh();
                Check(Vector3.Distance(g.Player.transform.position+Vector3.up,item.transform.position)<=InteractionDetector.Range,"Reach interaction "+id);
                Check(g.Detector.Target!=null && g.Detector.Target.objectiveId==id,"Interaction detector targets the correct objective "+id);
                if(id=="branch" && !checkpointTested) {
                    g.NoticeUntil=0; g.CompletionUntil=0; yield return Capture("Snap03-Forest-Challenge");
                    structureText="Adventure Quest / "+g.State+"\n\n"+g.World.name+"\n    "+g.World.Interactables.Count+" authored interactables / "+g.World.Gates.Count+" progression gates\n\nExplorer & Funobotz\n    Explorer Engineer: CharacterController + ExplorerController\n    Expedition camera: Camera + ThirdPersonCamera\n    Petalo / Quacky / Tolly / Tiko: live RobotController instances\n\nMissionManager: "+g.Missions.Current.id+" / objective "+id+"\nInteractionDetector target: "+g.Detector.Target.DisplayName+"\nInteractionManager: "+(g.Interactions.Busy?"working":"ready")+" / carried object: "+(g.Interactions.Carried?g.Interactions.Carried.DisplayName:"none");
                    structureVisible=true; yield return Capture("Snap08-Live-Runtime-Structure"); structureVisible=false;
                }
                if(item.needsInspection) { g.Robots.Select(item.requiredRobot==RobotKind.None?RobotKind.Petalo:item.requiredRobot); Check(!g.Interactions.TryInteract(item),"Inspection required for "+id); item.Inspect(); }
                if(item.requiredRobot!=RobotKind.None) {
                    g.Robots.Select((RobotKind)(((int)item.requiredRobot+1)%4)); Check(!g.Interactions.TryInteract(item),"Wrong robot rejected for "+id); g.Robots.Select(item.requiredRobot);
                }
                if(id=="rescue") {
                    Check(g.Missions.CanRescue,"All four restoration flags required and present");
                    foreach(string system in new[]{"energy","mechanism","beacon","access"}) {
                        g.Missions.Completed.Remove(system);
                        Check(!g.Missions.CanRescue && !g.Interactions.TryInteract(item),"Rescue blocked when "+system+" is missing");
                        if(system=="energy") yield return Capture("Snap09-Rescue-Prerequisite-Test");
                        g.Missions.Completed.Add(system);
                    }
                    g.NoticeUntil=0; yield return Capture("09-four-systems-ready");
                }
                Check(g.Interactions.TryInteract(item),"Accept objective "+id);
                float deadline=Time.realtimeSinceStartup+12;
                while(g.Interactions.Busy && Time.realtimeSinceStartup<deadline) yield return null;
                Check(!g.Interactions.Busy && g.Missions.Completed.Contains(id),"Complete objective "+id);
                if(id=="branch" && !checkpointTested) yield return Capture("Snap04-Tiko-Lift");
                if(id=="branch" || id=="stone") {
                    var unsafeItem=g.World.Interactables.Find(x=>x.region==g.Missions.Region && x.action==ActionKind.UnsafePlace);
                    yield return Walk(unsafeItem.transform.position+Vector3.back*2.2f);
                    Check(!g.Interactions.TryInteract(unsafeItem) && g.Interactions.Carried==null,"Protected plants reject and reset "+id);
                    Check(Vector3.Distance(item.movable.position,item.originalPosition)<.01f,"Object resets to its original position");
                    if(id=="branch" && !checkpointTested) yield return Capture("Snap05-Protected-Plants-Feedback");
                    yield return Walk(item.transform.position+Vector3.back*2.2f); Check(g.Interactions.TryInteract(item),"Can lift reset "+id);
                    while(g.Interactions.Busy) yield return null;
                }
                if(id=="branch" && !checkpointTested) {
                    checkpointTested=true; g.StartAdventure(true); yield return null;
                    Check(g.Missions.Region==1 && g.Missions.Step==0 && g.Missions.ExplorerTokens==20,"Continue restores the unlocked region and checkpoint tokens");
                    Check(g.Interactions.Carried==null && !g.World.Gates[0].gameObject.activeSelf,"Partial puzzle reload clears carried object and restores earlier gateway");
                    previousRegion=1;
                }
                if(id=="gateway" || id=="forest_marker" || id=="bridge_cross" || id=="castle_light" || id=="cave_three" || id=="garden_marker") yield return Capture("region-"+g.Missions.Region);
                if(id=="forest_marker") yield return Capture("Snap06-Forest-Complete");
            }
            yield return new WaitForSeconds(4);
            Check(g.Missions.Finished && g.World.Restored,"Dragon rescue completes");
            Check(!g.World.FinalBarrier.gameObject.activeSelf && g.World.Portal.gameObject.activeSelf,"Final barrier opens and portal activates");
            Check(g.State==GameState.GAME_COMPLETE,"Victory screen appears"); yield return Capture("10-victory");
            int savedTokens=g.Missions.ExplorerTokens;
            g.StartAdventure(true); yield return new WaitForSeconds(.2f);
            Check(g.Missions.Finished && g.Missions.CanRescue && g.Missions.ExplorerTokens==savedTokens && g.State==GameState.GAME_COMPLETE,"PlayerPrefs Continue restores completed expedition and tokens");
            Check(errors==0,"No runtime errors or exceptions");
            g.StartAdventure(false); yield return null;
            Check(g.State==GameState.TUTORIAL,"Play Again offers tutorial again"); g.CloseTutorial();
            Check(g.Missions.Region==0 && g.Missions.Step==0 && !g.Missions.CanRescue && !g.World.Restored,"Play Again resets all objectives and final systems");
            g.ReturnToMenu(); Check(g.State==GameState.MAIN_MENU,"Return to menu works");
            checks.Add(failed?"RESULT: FAILED":"RESULT: PASSED"); File.WriteAllLines(Path.Combine(folder,"acceptance-results.txt"),checks); Debug.Log("QUEST_QA_FINISHED"); Application.Quit(failed?1:0);
        }
        IEnumerator Walk(Vector3 target)
        {
            target.y=g.Player.transform.position.y; float deadline=Time.realtimeSinceStartup+18; g.Player.CameraRig.Yaw=0;
            while(Time.realtimeSinceStartup<deadline)
            {
                var delta=target-g.Player.transform.position; delta.y=0; if(delta.magnitude<.24f) break;
                var input=new Vector2(delta.x,delta.z).normalized*Mathf.Clamp01(delta.magnitude*2);
                g.Player.Tick(input,false,Time.deltaTime); yield return null;
            }
            var remaining=target-g.Player.transform.position; remaining.y=0;
            Check(remaining.magnitude<.7f,"Walk path to "+target.ToString("F1"));
        }
        IEnumerator Capture(string name)
        {
            yield return new WaitForEndOfFrame(); ScreenCapture.CaptureScreenshot(Path.Combine(folder,name+".png")); yield return null;
        }
    }
}
