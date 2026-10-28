using System.Collections;
using UnityEngine;
namespace DragonValley
{
    public class InteractionManager : MonoBehaviour
    {
        public bool Busy { get; private set; }
        public InteractableBase Carried { get; private set; }
        void Update()
        {
            var g=AdventureGame.Instance;
            if(!g.IsPlaying || AdventureGame.Autopilot) return;
            var item=g.Detector.Target;
            if(item && Input.GetKeyDown(KeyCode.F)) item.Inspect();
            if(item && Input.GetKeyDown(KeyCode.E)) TryInteract(item);
        }
        public bool TryInteract(InteractableBase item)
        {
            var g=AdventureGame.Instance;
            if(!item || !g.IsPlaying || Busy || item.done) return false;
            if(Vector3.Distance(g.Player.transform.position+Vector3.up,item.transform.position)>InteractionDetector.Range) return false;
            if(item.region!=g.Missions.Region) return false;
            if(item.Mechanism) {
                if(Physics.Linecast(g.Player.transform.position+Vector3.up,item.transform.position,out var obstruction,1,QueryTriggerInteraction.Ignore)
                    && Vector3.Distance(obstruction.point,item.transform.position)>.8f) return false;
                return item.Mechanism.Operate(item.Operation);
            }
            if(item.action==ActionKind.UnsafePlace)
            {
                if(Carried==null) { g.Notify("Protected habitat","Keep heavy objects off the pink plant beds."); return false; }
                Carried.ResetObject(); g.Robots.Tiko.Release();
                // The completed pickup remains valid; the reset object must be lifted again.
                Carried.done=false; Carried=null;
                g.Missions.Fail("Plants need room to grow. Your object was reset; lift it again and use the cyan pad.");
                return false;
            }
            bool repick=item.action==ActionKind.Carry && g.Missions.Completed.Contains(item.objectiveId) && Carried==null
                && g.Missions.CurrentObjective.id==item.objectiveId+"_place";
            if(!g.Missions.IsCurrent(item.objectiveId) && !repick) { g.Notify("Observe, then solve",g.Missions.CurrentObjective.hint,true); return false; }
            if(item.action==ActionKind.Rescue && !g.Missions.CanRescue) { g.Notify("Four systems required","Delivery, repair, light and access must all be restored.",true); return false; }
            if(item.requiredRobot!=RobotKind.None && g.Robots.Selected!=item.requiredRobot) { g.Notify("Choose your Funobotz",item.requiredRobot+" is needed here. Select using keys 1–4.",true); return false; }
            if(item.needsInspection && !item.inspected) { g.Notify("Look a little closer","Press F to inspect this mechanism before using it.",true); return false; }
            if(item.action==ActionKind.Place && (Carried==null || Carried!=item.source)) { g.Notify("Something is missing","Use Tiko to lift the matching object first.",true); return false; }
            StartCoroutine(Perform(item,repick)); return true;
        }
        IEnumerator Perform(InteractableBase item,bool repick)
        {
            Busy=true; var g=AdventureGame.Instance;
            g.Audio.Play("interact");
            if(item.requiredRobot!=RobotKind.None)
            {
                var robot=g.Robots.Get(item.requiredRobot);
                var target=item.transform.position;
                yield return robot.MoveToTarget(target);
                if(item.requiredRobot==RobotKind.Petalo) g.Robots.Petalo.Illuminate();
                if(item.requiredRobot==RobotKind.Tolly) g.Robots.Tolly.OperateGate();
                if(item.requiredRobot==RobotKind.Quacky && item.destination)
                { yield return g.Robots.Quacky.Deliver(item.movable,item.destination.position); }
                if(item.action==ActionKind.Carry) { Carried=item; g.Robots.Tiko.Grab(item.movable); }
                if(item.action==ActionKind.Place)
                {
                    g.Robots.Tiko.Release();
                    if(Carried.movable) yield return Move(Carried.movable,item.destination?item.destination.position:item.transform.position,.65f);
                    Carried=null;
                }
                robot.Celebrate(); robot.ReturnToPlayer();
            }
            item.done=true;
            g.NoticeUntil=0;
            g.World.Apply(item.objectiveId);
            if(!repick) g.Missions.CompleteObjective(item.objectiveId);
            g.Audio.Play(item.action==ActionKind.Rescue?"dragon rescue":"objective complete");
            Busy=false;
            if(item.action==ActionKind.Rescue) g.FinishAdventure();
        }
        public static IEnumerator Move(Transform obj,Vector3 target,float duration)
        {
            if(!obj) yield break; var from=obj.position;
            for(float t=0;t<duration;t+=Time.deltaTime) { if(!obj) yield break; obj.position=Vector3.Lerp(from,target,Mathf.SmoothStep(0,1,t/duration)); yield return null; }
            if(obj) obj.position=target;
        }
        public void Clear() { StopAllCoroutines(); Busy=false; Carried=null; }
    }
}
