using UnityEngine;
namespace DragonValley
{
    public class InteractionDetector : MonoBehaviour
    {
        public InteractableBase Target { get; private set; }
        public const float Range=4.5f;
        public void Refresh()
        {
            var g=AdventureGame.Instance; if(!g || !g.IsPlaying) { Target=null; return; }
            float best=float.MaxValue; Target=null;
            var origin=g.Player.transform.position+Vector3.up;
            foreach(var item in g.World.Interactables)
            {
                if(!item || !item.Relevant) continue;
                float d=Vector3.Distance(origin,item.transform.position);
                if(d>Range) continue;
                var view=g.Camera.WorldToViewportPoint(item.transform.position);
                if(view.z<0 || view.x<-.1f || view.x>1.1f) continue;
                // Prefer the active task, while allowing intentional environmental mistakes.
                float score=d+(g.Missions.IsCurrent(item.objectiveId)?0:1.5f);
                if(Physics.Linecast(origin,item.transform.position,out var hit,1<<0,QueryTriggerInteraction.Ignore)
                    && hit.transform!=item.transform && !hit.transform.IsChildOf(item.transform)
                    && Vector3.Distance(hit.point,item.transform.position)>.8f) continue;
                if(score<best) { best=score; Target=item; }
            }
        }
        void Update() { Refresh(); }
    }
}
