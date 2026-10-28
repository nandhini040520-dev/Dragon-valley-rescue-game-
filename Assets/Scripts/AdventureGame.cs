using System;
using System.Collections;
using UnityEngine;
namespace DragonValley
{
    public class AdventureGame : MonoBehaviour
    {
        public static AdventureGame Instance { get; private set; }
        public static bool TestMode { get; private set; }
        public static bool Autopilot { get; set; }
        public GameState State { get; private set; }=GameState.BOOT;
        public bool IsPlaying => State==GameState.PLAYING || State==GameState.MISSION_ACTIVE || State==GameState.MISSION_COMPLETE;
        public MissionManager Missions { get; private set; }
        public AdventureWorld World { get; private set; }
        public ExplorerController Player { get; private set; }
        public Camera Camera { get; private set; }
        public RobotTeam Robots { get; private set; }
        public InteractionManager Interactions { get; private set; }
        public InteractionDetector Detector { get; private set; }
        public AudioManager Audio { get; private set; }
        public AdventureHUD HUD { get; private set; }
        Transform actors;
        public string NoticeTitle="",NoticeText="";
        public bool NoticeError;
        public float NoticeUntil;
        public string CompletionTitle="",CompletionSkill="";
        public float CompletionUntil;
        public bool ShowJournal;
        public MechanismController InspectedMechanism;
        public float InspectionUntil;
        public int TutorialPage { get; private set; }
        public const int TutorialPageCount=3;
        GameState tutorialReturnState;
        RobotKind tutorialPreviousRobot;
        bool finishing;
        void Awake()
        {
            Instance=this;
            var args=Environment.GetCommandLineArgs(); bool automatedQA=Array.IndexOf(args,"-questQA")>=0;
            bool assetTour=Array.IndexOf(args,"-assetTour")>=0;
            TestMode=automatedQA || assetTour || Array.IndexOf(args,"-questSmoke")>=0; Autopilot=automatedQA || assetTour;
            Application.targetFrameRate=60;
            Missions=gameObject.AddComponent<MissionManager>(); Audio=gameObject.AddComponent<AudioManager>();
            Interactions=gameObject.AddComponent<InteractionManager>(); Detector=gameObject.AddComponent<InteractionDetector>();
            HUD=gameObject.AddComponent<AdventureHUD>();
            Missions.MissionCompleted+=OnMissionCompleted;
            CreateWorld(); SetState(GameState.MAIN_MENU);
            if(automatedQA) gameObject.AddComponent<MechanismQA>();
            if(assetTour) gameObject.AddComponent<AssetTour>();
        }
        void CreateWorld()
        {
            var root=new GameObject("AdventureWorld • connected expedition"); World=root.AddComponent<AdventureWorld>(); World.Build();
            actors=new GameObject("Explorer & Funobotz").transform;
            var explorer=new GameObject("Explorer Engineer"); explorer.transform.SetParent(actors); explorer.layer=2;
            explorer.AddComponent<CharacterController>(); Player=explorer.AddComponent<ExplorerController>();
            Player.Visual=StorybookAssets.Explorer(explorer.transform);
            var camGo=new GameObject("Expedition camera"); camGo.transform.SetParent(actors); Camera=camGo.AddComponent<Camera>(); Camera.tag="MainCamera"; Camera.fieldOfView=58; Camera.nearClipPlane=.1f; Camera.farClipPlane=230; Camera.backgroundColor=new Color(.62f,.78f,.8f); Camera.clearFlags=CameraClearFlags.SolidColor; Camera.allowHDR=true; camGo.AddComponent<AudioListener>();
            var rig=camGo.AddComponent<ThirdPersonCamera>(); rig.Target=Player.transform; Player.CameraRig=rig;
            Robots=actors.gameObject.AddComponent<RobotTeam>();
            for(int i=0;i<4;i++) {
                var go=new GameObject(((RobotKind)i).ToString()); go.transform.SetParent(actors);
                RobotController r=i==0?(RobotController)go.AddComponent<PetaloController>():i==1?go.AddComponent<QuackyController>():i==2?go.AddComponent<TollyController>():go.AddComponent<TikoController>();
                r.Kind=(RobotKind)i; r.FollowOffset=new Vector3((i-1.5f)*1.9f,0,-3.2f-(i%2)*1.3f); r.Visual=StorybookAssets.Robot(go.transform,(RobotKind)i).transform;
                r.SelectionRing=WorldArt.Cylinder("Selected Funobotz",go.transform,new Vector3(0,-.5f,0),new Vector3(1.2f,.035f,1.2f),WorldArt.Mint,false);
                WorldArt.Label(go.transform,((RobotKind)i).ToString(),new Vector3(0,i==0?1.44f:1.15f,0),.1f);
                if(i==0) { var lamp=go.AddComponent<Light>(); lamp.type=LightType.Point; lamp.color=new Color(1,.8f,.74f); lamp.range=9; lamp.intensity=2; ((PetaloController)r).Lamp=lamp; }
                Robots.Members[i]=r;
            }
            Robots.Select(RobotKind.Petalo); Player.Teleport(World.Spawn(0));
            foreach(var r in Robots.Members) r.transform.position=Player.transform.position+r.FollowOffset+Vector3.up*.65f;
            rig.Snap();
        }
        public void StartAdventure(bool resume,bool replay=false)
        {
            Missions.Replay=replay;
            StopAllCoroutines(); Interactions.Clear(); finishing=false; Time.timeScale=1;
            Robots.Tiko.Release();
            if(World) { World.gameObject.SetActive(false); Destroy(World.gameObject); }
            if(actors) { actors.gameObject.SetActive(false); Destroy(actors.gameObject); }
            if(resume) Missions.LoadProgress(); else Missions.ResetProgress();
            CreateWorld(); World.RestoreCheckpoint(); Player.Teleport(World.Spawn(Missions.Region)); Player.CameraRig.Yaw=0; Player.CameraRig.Snap();
            foreach(var r in Robots.Members) r.transform.position=Player.transform.position+r.FollowOffset+Vector3.up*.65f;
            ShowJournal=false; CompletionUntil=0; InspectedMechanism=null; SetState(GameState.MISSION_ACTIVE);
            Notify("Welcome, Explorer Engineer","F inspects the connected machine. E operates its controls. Diagnose inactive links and restore the output yourself.");
            if(Missions.Finished) { finishing=true; SetState(GameState.GAME_COMPLETE); }
            else if(!resume) OpenTutorial();
        }
        public void OpenTutorial()
        {
            if(State!=GameState.PAUSED && !IsPlaying) return;
            tutorialReturnState=State==GameState.PAUSED?GameState.PAUSED:GameState.MISSION_ACTIVE;
            tutorialPreviousRobot=Robots.Selected;
            TutorialPage=0; ShowJournal=false; NoticeUntil=0; CompletionUntil=0;
            SetState(GameState.TUTORIAL);
        }
        public void NextTutorial()
        {
            if(State!=GameState.TUTORIAL) return;
            if(TutorialPage<TutorialPageCount-1) TutorialPage++;
            else CloseTutorial();
        }
        public void PreviousTutorial()
        { if(State==GameState.TUTORIAL) TutorialPage=Mathf.Max(0,TutorialPage-1); }
        public void CloseTutorial()
        {
            if(State!=GameState.TUTORIAL) return;
            Robots.Select(tutorialPreviousRobot);
            SetState(tutorialReturnState);
        }
        void Update()
        {
            if(Autopilot) return;
            if(State==GameState.TUTORIAL) {
                if(Input.GetKeyDown(KeyCode.Escape)) CloseTutorial();
                else if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) NextTutorial();
                else if(Input.GetKeyDown(KeyCode.Backspace)) PreviousTutorial();
                else if(TutorialPage==1) for(int i=0;i<4;i++)
                    if(Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1+i))) Robots.Select((RobotKind)i);
                return;
            }
            if(Input.GetKeyDown(KeyCode.Escape)) TogglePause();
            if(IsPlaying && Input.GetKeyDown(KeyCode.Q)) ShowJournal=!ShowJournal;
            if(State==GameState.MISSION_COMPLETE && Time.unscaledTime>CompletionUntil) SetState(GameState.MISSION_ACTIVE);
        }
        void OnMissionCompleted(Mission mission)
        {
            int index=Missions.Missions.IndexOf(mission); World.OpenRegion(index);
            CompletionTitle=mission.zone+" Restored"; CompletionSkill=mission.skill; CompletionUntil=Time.unscaledTime+5;
            SetState(GameState.MISSION_COMPLETE);
        }
        public void Notify(string title,string body,bool error=false)
        { NoticeTitle=title; NoticeText=body; NoticeError=error; NoticeUntil=Time.unscaledTime+7; if(error) Audio.Play("error"); }
        public void TogglePause()
        {
            if(finishing) return;
            if(State==GameState.PAUSED) SetState(GameState.MISSION_ACTIVE); else if(IsPlaying) SetState(GameState.PAUSED);
        }
        public void ReturnToMenu() { Interactions.Clear(); SetState(GameState.MAIN_MENU); }
        public void SetState(GameState state)
        {
            State=state; Time.timeScale=state==GameState.PAUSED || state==GameState.TUTORIAL?0:1;
            bool capture=IsPlaying && !Autopilot; Cursor.lockState=capture?CursorLockMode.Locked:CursorLockMode.None; Cursor.visible=!capture;
        }
        public void FinishAdventure() { if(finishing) return; finishing=true; StartCoroutine(Victory()); }
        public void ReplayChallenge()
        {
            StartAdventure(false,true); CloseTutorial(); Missions.StartReplay(2);
            World.RestoreCheckpoint(); Player.Teleport(World.Spawn(2));
            foreach(var r in Robots.Members) r.transform.position=Player.transform.position+r.FollowOffset+Vector3.up*.65f;
            Notify("Rainbow Bridge replay","Compare permanent repair and the timed bypass. Time and operations are scored separately from your campaign.");
        }
        IEnumerator Victory() { yield return new WaitForSeconds(3.5f); Player.CameraRig.Yaw=-22; Player.CameraRig.Pitch=17; SetState(GameState.GAME_COMPLETE); }
        void OnDestroy() { if(Missions) Missions.MissionCompleted-=OnMissionCompleted; if(Instance==this) Instance=null; Time.timeScale=1; }
    }
}
