using System;
using UnityEngine;

namespace DragonValley
{
    public enum RobotKind { None = -1, Petalo, Quacky, Tolly, Tiko }
    public enum GameState { BOOT, MAIN_MENU, PLAYING, MISSION_ACTIVE, MISSION_COMPLETE, PAUSED, GAME_COMPLETE, TUTORIAL }
    public enum ActionKind { Activate, Carry, Place, UnsafePlace, Rescue }
    public static class GameplayEvents
    {
        public static event Action<string> Raised;
        public static void Emit(string id) { Raised?.Invoke(id); }
    }
    [Serializable]
    public class Objective
    {
        public string id, text, hint;
        public RobotKind robot;
        public bool inspect;
        public Objective(string id, string text, string hint, RobotKind robot = RobotKind.None, bool inspect = false)
        { this.id = id; this.text = text; this.hint = hint; this.robot = robot; this.inspect = inspect; }
    }
    [Serializable]
    public class Mission
    {
        public string id, title, zone, skill, completionEvent;
        public Objective[] objectives;
        public Mission(string id, string title, string zone, string skill, string completionEvent, params Objective[] objectives)
        { this.id=id; this.title=title; this.zone=zone; this.skill=skill; this.completionEvent=completionEvent; this.objectives=objectives; }
    }
}
