using System.Collections;
using UnityEngine;
namespace DragonValley
{
    public class RobotController : MonoBehaviour
    {
        public RobotKind Kind;
        public Vector3 FollowOffset;
        public Transform Visual;
        public GameObject SelectionRing;
        public bool Working { get; protected set; }
        float cheer;
        FunobotzMotion kitMotion;
        Vector3 followVelocity;
        float stuck;
        void Start() { if(Visual) kitMotion=Visual.GetComponent<FunobotzMotion>(); }
        void Update()
        {
            var g=AdventureGame.Instance; if(!g || !g.IsPlaying) return;
            if(kitMotion) kitMotion.Working=Working;
            if(!Working) {
                var target=g.Player.transform.position+FollowOffset+Vector3.up*.65f;
                var delta=target-transform.position; delta.y=0;
                foreach(var other in g.Robots.Members) if(other && other!=this) {
                    var away=transform.position-other.transform.position; away.y=0;
                    if(away.sqrMagnitude<1.1f) target+=away.normalized*.7f;
                }
                var direction=delta.normalized;
                if(delta.magnitude>.5f && Physics.SphereCast(transform.position+Vector3.up*.4f,.3f,direction,out var hit,1.2f,1,QueryTriggerInteraction.Ignore)) {
                    var tangent=Vector3.Cross(Vector3.up,hit.normal).normalized*((int)Kind%2==0?1:-1);
                    target=transform.position+tangent*2; stuck+=Time.deltaTime;
                } else stuck=0;
                if(delta.magnitude>.5f) transform.position=Vector3.SmoothDamp(transform.position,target,ref followVelocity,.3f,7,Time.deltaTime);
                if((delta.magnitude>35 || stuck>8) && !Physics.CheckSphere(target,.3f,1,QueryTriggerInteraction.Ignore)) { transform.position=target; stuck=0; }
            }
            if(Visual) Visual.localPosition=Vector3.up*(Mathf.Sin(Time.time*3+(int)Kind)*.07f+Mathf.Max(0,cheer)*.2f);
            cheer=Mathf.MoveTowards(cheer,0,Time.deltaTime*2);
        }
        public virtual IEnumerator MoveToTarget(Vector3 point)
        {
            Working=true; point.y=Mathf.Max(.65f,point.y);
            float timeout=0;
            while(Vector3.Distance(transform.position,point)>.12f && timeout<4) {
                transform.position=Vector3.MoveTowards(transform.position,point,Time.deltaTime*12);
                timeout+=Time.deltaTime; yield return null;
            }
        }
        public virtual void ReturnToPlayer() { Working=false; }
        public void Celebrate() { cheer=1; if(Visual) { var motion=Visual.GetComponent<FunobotzMotion>(); if(motion) motion.Act(); } }
    }
}
