using UnityEngine;
namespace DragonValley
{
    public class AdventureHUD : MonoBehaviour
    {
        GUIStyle body,title,small,button;
        GUIStyle cardTitle,cardBody,eyebrow,infoBody;
        readonly Color paper=new Color(.96f,.94f,.86f), panel=new Color(.025f,.075f,.10f,.96f);
        void Init()
        {
            var font=Resources.Load<Font>("ExpeditionSans");
            body=new GUIStyle(GUI.skin.label){font=font,fontSize=21,wordWrap=true}; body.normal.textColor=paper;
            title=new GUIStyle(body){fontSize=36}; small=new GUIStyle(body){fontSize=17};
            button=new GUIStyle(GUI.skin.button){font=font,fontSize=22,wordWrap=true};
            cardTitle=new GUIStyle(body){font=Resources.Load<Font>("ExpeditionSerif")??font,fontSize=28,padding=new RectOffset()};
            cardBody=new GUIStyle(body){fontSize=18,padding=new RectOffset()};
            infoBody=new GUIStyle(body){fontSize=16,padding=new RectOffset()};
            eyebrow=new GUIStyle(small){fontSize=14,padding=new RectOffset()};
            eyebrow.normal.textColor=new Color(.64f,.75f,.77f);
        }
        void Fill(float x,float y,float w,float h,Color color) { var old=GUI.color; GUI.color=color; GUI.DrawTexture(new Rect(x,y,w,h),Texture2D.whiteTexture); GUI.color=old; }
        void Card(float x,float y,float w,float h,Color accent) { Fill(x,y,w,h,new Color(.035f,.095f,.14f,.94f)); Fill(x,y,4,h,accent); }
        void TaskCard(AdventureGame g,MechanismController m)
        {
            Card(28,26,600,216,WorldArt.Mint);
            Text(50,40,550,25,"CURRENT TASK  /  "+g.Missions.Current.zone.ToUpperInvariant(),eyebrow);
            Text(50,72,550,67,m.Title,cardTitle);
            Text(50,142,550,21,"WHAT TO DO NEXT",eyebrow);
            Text(50,170,550,62,m.Completed?"System restored. Continue along the discovery trail.":m.Hint,cardBody);
            Card(1270,26,302,150,WorldArt.Mint);
            Text(1292,41,260,25,"EXPEDITION  "+(g.Missions.Region+1)+" / 7",cardBody);
            Text(1292,76,260,23,"SYSTEM: "+m.State,eyebrow);
            Text(1292,104,260,24,"Travel "+Mathf.RoundToInt(m.Progress*100)+"%   /   "+Mathf.CeilToInt(m.Elapsed)+"s",cardBody);
            for(int i=0;i<7;i++) Fill(1294+i*36,151,28,3,i<=g.Missions.Region?WorldArt.Mint:new Color(.25f,.35f,.38f));
            Card(1270,188,302,46,WorldArt.Gold);
            Text(1292,201,262,25,g.ShowJournal?"Q  CLOSE INFORMATION":"Q  INFORMATION / FIELD NOTES",eyebrow);
            if(m.Temporary) { Card(1270,246,302,57,WorldArt.Gold); Text(1292,263,262,30,"BYPASS: "+Mathf.CeilToInt(m.TemporaryRemaining)+" seconds",cardBody); }
        }
        void InformationCard(AdventureGame g,MechanismController m)
        {
            Card(866,252,706,480,WorldArt.Mint);
            Text(890,269,658,23,"INFORMATION  /  CONNECTED SYSTEM",eyebrow);
            Text(890,304,658,42,"Your discovery notes",cardTitle);
            Fill(890,354,658,1,new Color(.22f,.35f,.39f));
            Text(890,371,658,278,m.StatusText(),infoBody);
            Text(890,669,658,24,"Restored "+g.Missions.Discoveries+" / 7   |   Clues "+g.Missions.Clues+"   |   Actions "+m.Actions+"   |   Errors "+m.Incorrect,eyebrow);
            Text(890,701,658,23,g.ShowJournal?"Q  Close information. You remain in control of the machine.":"F  Inspect nearby parts   /   Q  Keep information open",eyebrow);
        }
        void Panel(float x,float y,float w,float h) { GUI.color=panel; GUI.DrawTexture(new Rect(x,y,w,h),Texture2D.whiteTexture); GUI.color=Color.white; }
        void Text(float x,float y,float w,float h,string value,GUIStyle style=null) { GUI.Label(new Rect(x,y,w,h),value,style??body); }
        bool Button(float x,float y,float w,string value) => GUI.Button(new Rect(x,y,w,54),value,button);
        void OnGUI()
        {
            if(body==null) Init(); var g=AdventureGame.Instance; if(!g || !g.Missions) return;
            GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(Screen.width/1600f,Screen.height/900f,1));
            if(g.State==GameState.MAIN_MENU) { Menu(g); return; }
            if(g.State==GameState.TUTORIAL) { Tutorial(g); return; }
            if(g.State==GameState.GAME_COMPLETE) { Victory(g); return; }
            var m=g.World.Machines[g.Missions.Region];
            TaskCard(g,m);
            if(Time.unscaledTime<g.NoticeUntil) { Card(28,260,600,140,g.NoticeError?WorldArt.Pink:WorldArt.Gold); Text(50,275,550,48,g.NoticeTitle,cardBody); Text(50,328,550,62,g.NoticeText,small); }
            var target=g.Detector.Target;
            Card(390,779,820,90,WorldArt.Mint);
            Text(415,791,765,31,target?target.DisplayName:"Explore the discovery trail",cardBody);
            Text(415,832,765,27,"F  Inspect   |   E  Operate   |   Q  Information   |   1-4  Companions",eyebrow);
            Card(28,797,346,72,WorldArt.Gold);
            Text(50,807,314,54,"WASD  Move   |   Mouse  Look\nSpace  Jump   |   Esc  Pause",eyebrow);
            if(g.IsPlaying) { GUI.color=WorldArt.Mint; GUI.DrawTexture(new Rect(797,447,6,6),Texture2D.whiteTexture); GUI.color=Color.white; }
            if(g.ShowJournal || (g.InspectedMechanism && Time.unscaledTime<g.InspectionUntil)) {
                var inspected=g.ShowJournal?m:g.InspectedMechanism?g.InspectedMechanism:m;
                InformationCard(g,inspected);
            }
            if(g.State==GameState.PAUSED) {
                Panel(430,120,740,680); Text(470,155,660,70,"Expedition paused",title);
                Text(470,227,660,85,"Progress saves at completed machines. Unfinished machines restart safely at the region checkpoint.");
                if(Button(470,340,660,"RESUME")) g.TogglePause();
                if(Button(470,408,660,"FIELD GUIDE")) g.OpenTutorial();
                if(Button(470,476,660,"REPLAY BRIDGE CHALLENGE")) g.ReplayChallenge();
                if(Button(470,544,660,"RETURN TO MENU")) g.ReturnToMenu();
                if(Button(470,612,660,"QUIT")) { g.Missions.SaveProgress(); Application.Quit(); }
            }
        }
        void Menu(AdventureGame g)
        {
            Panel(40,40,750,820); Text(80,80,660,45,"DISCOVERY WORLD / PHASE 2",small);
            Text(80,145,660,110,"Adventure Quest\nThe Dragon Valley Rescue",title);
            Text(80,285,660,150,"Discover how ancient machines work.\nRestore their power, motion and signals.\nRescue Aster with four companion friends.");
            if(Button(80,475,660,"START ADVENTURE")) g.StartAdventure(false);
            GUI.enabled=g.Missions.HasSave; if(Button(80,545,660,"CONTINUE")) g.StartAdventure(true); GUI.enabled=true;
            if(Button(80,615,660,"REPLAY BRIDGE CHALLENGE")) g.ReplayChallenge();
            if(Button(80,685,660,"QUIT")) Application.Quit();
            Text(80,785,660,35,"CRUST INNOVATORS / AVS Engineering College",small);
        }
        void Tutorial(AdventureGame g)
        {
            Panel(120,65,1360,770); Text(165,100,1270,60,"Explorer field guide / "+(g.TutorialPage+1)+" of 3",title);
            string[] headings={"You are the mechanism operator","Four companions, one explorer","Your first machine: Ancient Gate Control"};
            string[] content={
                "WASD moves. Mouse looks. Space jumps.\n\nF inspects nearby parts, focuses the camera and reveals the connected system status.\nE operates the nearby control. Controls can be adjusted in any order.\nQ opens the diagnosis journal. Esc pauses.\n\nFollow the visible chain: source > switch > cable > transmission > actuator.\nIf a link is inactive, downstream motion stops. Inspect and correct it.",
                "Petalo illuminates the surroundings.\nQuacky accompanies your discoveries.\nTolly provides access-themed feedback.\nTiko reacts to successful repairs.\n\nKeys 1-4 focus a companion. Robots never solve a mechanism for you.\n\nLook for service inscriptions and matching marks. Optional clues reward exploration.",
                "Inspect the machine with F. Find the loose crystal and install it in the socket.\nToggle the power switch and inspect which links become active.\nMatch the gear to its service marks and fit the coupling pin.\nChoose a route, release the brake, and operate the drive.\nWatch the gears transmit motion and the selected route open.\nReach the checkpoint to verify the result.\n\nLater: the bridge offers a 25-second bypass or a permanent gear repair.\nPermanent bridges must be locked before you cross; lifts can be ridden."};
            Text(165,187,1250,60,headings[g.TutorialPage],title); Text(165,275,1235,395,content[g.TutorialPage]);
            if(Button(165,733,230,"BACK")) g.PreviousTutorial();
            if(Button(420,733,230,"CLOSE GUIDE")) g.CloseTutorial();
            if(Button(1035,733,400,g.TutorialPage==2?"BEGIN EXPEDITION":"NEXT")) g.NextTutorial();
        }
        void Victory(AdventureGame g)
        {
            Panel(100,90,1100,730); Text(145,130,1010,110,g.Missions.Replay?"Bridge challenge complete":"Dragon Valley restored",title);
            Text(145,260,990,140,"You diagnosed the systems, configured their inputs,\nand restored the world through visible cause and effect.\n\n"+g.Missions.LastResult);
            Text(145,445,990,130,"Restored: "+g.Missions.Discoveries+"   |   Clues: "+g.Missions.Clues+"   |   Hidden areas: "+g.Missions.HiddenAreas+"\nTotal time: "+g.Missions.TotalSeconds.ToString("0.0")+"s   |   Operations: "+g.Missions.TotalActions+"\nIncorrect: "+g.Missions.TotalIncorrect+"   |   Redundant: "+g.Missions.TotalUnnecessary);
            if(Button(145,630,990,"REPLAY BRIDGE - TRY THE OTHER SOLUTION")) g.ReplayChallenge();
            if(Button(145,703,470,"NEW EXPEDITION")) g.StartAdventure(false);
            if(Button(640,703,495,"MAIN MENU")) g.ReturnToMenu();
        }
    }
}
