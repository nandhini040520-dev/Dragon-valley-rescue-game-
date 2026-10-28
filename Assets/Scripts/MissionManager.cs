using System;
using System.Collections.Generic;
using UnityEngine;
namespace DragonValley
{
    public class MissionManager : MonoBehaviour
    {
        public readonly List<Mission> Missions=new List<Mission>();
        public readonly HashSet<string> Completed=new HashSet<string>();
        public int Region { get; private set; }
        public int Step => 0;
        public int ExplorerTokens { get; private set; }
        public bool Finished { get; private set; }
        public bool Replay;
        public int TotalActions, TotalIncorrect, TotalUnnecessary, Discoveries, Clues;
        public int HiddenAreas;
        public float TotalSeconds;
        public string LastResult="";
        public Mission Current => Missions[Mathf.Clamp(Region,0,6)];
        public Objective CurrentObjective => Finished?null:Current.objectives[0];
        public bool ENERGY_DELIVERED => CurrentMachine && CurrentMachine.Components[2].Output;
        public bool MECHANISM_REPAIRED => CurrentMachine && CurrentMachine.Components[3].Output;
        public bool BEACON_ACTIVE => CurrentMachine && CurrentMachine.Components[4].Output;
        public bool BARRIER_UNLOCKED => CurrentMachine && CurrentMachine.Progress>=1;
        public bool CanRescue => Region==6 && CurrentMachine && CurrentMachine.SafeOutput;
        MechanismController CurrentMachine => AdventureGame.Instance && AdventureGame.Instance.World ? AdventureGame.Instance.World.Machines[Region]:null;
        public event Action Changed;
        public event Action<Mission> MissionCompleted;
        static string SaveKey => AdventureGame.TestMode?"DragonValley.Phase2.QA.":"DragonValley.Phase2.v1.";
        public bool HasSave => PlayerPrefs.HasKey(SaveKey+"region");
        void Awake()
        {
            string[] zones={"Grand Gateway","Hidden Forest","Rainbow Bridge","Castle Place","Mystery Cave","Fairy Garden","Dragon Valley"};
            string[] skills={"Power and route selection","Flow and obstruction","Torque, alignment and stability","Direction and counterweight","Connected signal paths","Protecting living systems","Integrated restoration"};
            for(int i=0;i<7;i++) Missions.Add(new Mission("PHASE2_"+i,MechanismWorld.Titles[i],zones[i],skills[i],"RESTORED_"+i,
                new Objective("machine_"+i,"Restore the "+MechanismWorld.Titles[i],"Inspect the machine, diagnose the inactive links, configure the controls, then verify its physical output.")));
        }
        public bool IsCurrent(string id) => !Finished && CurrentObjective.id==id;
        // No legacy objective flag can bypass the actual safe output condition.
        public bool CompleteObjective(string id) => false;
        public void CompleteMechanism(MechanismController machine)
        {
            if(Finished || machine.Region!=Region || !machine.SafeOutput || !machine.Completed) return;
            Completed.Add(machine.Prefix); TotalActions+=machine.Actions; TotalIncorrect+=machine.Incorrect;
            TotalUnnecessary+=machine.Unnecessary; TotalSeconds+=machine.Elapsed; Discoveries++; if(machine.OptionalClue) Clues++;
            if(machine.HiddenFound) HiddenAreas++;
            ExplorerTokens+=100+(machine.OptionalClue?20:0)+(machine.HiddenFound?30:0);
            LastResult=string.Format("{0:0.0}s | {1} actions | {2} errors | {3} redundant",machine.Elapsed,machine.Actions,machine.Incorrect,machine.Unnecessary);
            string bestKey=SaveKey+"best."+Region+(machine.Temporary?".bypass":".permanent");
            if(machine.Elapsed<PlayerPrefs.GetFloat(bestKey,float.MaxValue)) { PlayerPrefs.SetFloat(bestKey,machine.Elapsed); LastResult+=" | NEW TIME BEST"; }
            if(machine.Actions<PlayerPrefs.GetInt(bestKey+".actions",int.MaxValue)) PlayerPrefs.SetInt(bestKey+".actions",machine.Actions);
            if(machine.Incorrect==0) LastResult+=" | PERFECT CONFIGURATION";
            PlayerPrefs.Save(); var done=Current; MissionCompleted?.Invoke(done); GameplayEvents.Emit(done.completionEvent);
            if(Region==6 || Replay) Finished=true; else Region++;
            SaveProgress(); Changed?.Invoke(); if(Replay) AdventureGame.Instance.SetState(GameState.GAME_COMPLETE);
        }
        public void Fail(string message) { AdventureGame.Instance.Notify("Machine warning",message,true); }
        public void SaveProgress()
        {
            if(Replay) return;
            PlayerPrefs.SetInt(SaveKey+"region",Region); PlayerPrefs.SetInt(SaveKey+"finished",Finished?1:0);
            PlayerPrefs.SetInt(SaveKey+"tokens",ExplorerTokens); PlayerPrefs.SetInt(SaveKey+"actions",TotalActions);
            PlayerPrefs.SetInt(SaveKey+"errors",TotalIncorrect); PlayerPrefs.SetInt(SaveKey+"redundant",TotalUnnecessary);
            PlayerPrefs.SetInt(SaveKey+"discoveries",Discoveries); PlayerPrefs.SetInt(SaveKey+"clues",Clues);
            PlayerPrefs.SetInt(SaveKey+"hidden",HiddenAreas);
            PlayerPrefs.SetFloat(SaveKey+"seconds",TotalSeconds); PlayerPrefs.Save();
        }
        public void LoadProgress()
        {
            Replay=false; Region=Mathf.Clamp(PlayerPrefs.GetInt(SaveKey+"region",0),0,6);
            Finished=PlayerPrefs.GetInt(SaveKey+"finished",0)==1; ExplorerTokens=PlayerPrefs.GetInt(SaveKey+"tokens",0);
            TotalActions=PlayerPrefs.GetInt(SaveKey+"actions",0); TotalIncorrect=PlayerPrefs.GetInt(SaveKey+"errors",0);
            TotalUnnecessary=PlayerPrefs.GetInt(SaveKey+"redundant",0); Discoveries=PlayerPrefs.GetInt(SaveKey+"discoveries",0); Clues=PlayerPrefs.GetInt(SaveKey+"clues",0);
            TotalSeconds=PlayerPrefs.GetFloat(SaveKey+"seconds",0); Completed.Clear();
            HiddenAreas=PlayerPrefs.GetInt(SaveKey+"hidden",0);
            for(int i=0;i<(Finished?7:Region);i++) Completed.Add("machine_"+i); Changed?.Invoke();
        }
        public void ResetProgress()
        {
            if(!Replay) { foreach(string suffix in new[]{"region","finished","tokens","actions","errors","redundant","discoveries","clues","hidden","seconds"}) PlayerPrefs.DeleteKey(SaveKey+suffix); PlayerPrefs.Save(); }
            Region=0; Finished=false; ExplorerTokens=TotalActions=TotalIncorrect=TotalUnnecessary=Discoveries=Clues=HiddenAreas=0; TotalSeconds=0; LastResult=""; Completed.Clear(); Changed?.Invoke();
        }
        public void StartReplay(int region) { Replay=true; Region=region; Finished=false; }
    }
}
