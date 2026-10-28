using UnityEngine;
namespace DragonValley
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        public Transform Target;
        [SerializeField] float distance=7, sensitivity=2.2f;
        public float Yaw=0, Pitch=26;
        Vector3 focusPoint;
        float focusUntil;
        public void Focus(Vector3 point) { focusPoint=point; focusUntil=Time.unscaledTime+3; }
        void LateUpdate()
        {
            if(!Target) return;
            if(AdventureGame.Instance.IsPlaying && !AdventureGame.Autopilot) {
                Yaw+=Input.GetAxis("Mouse X")*sensitivity;
                Pitch=Mathf.Clamp(Pitch-Input.GetAxis("Mouse Y")*sensitivity,10,62);
            }
            Snap();
        }
        public void Snap()
        {
            if(!Target) return;
            var focus=Target.position+Vector3.up*1.6f;
            if(Time.unscaledTime<focusUntil) focus=Vector3.Lerp(focus,focusPoint,.45f);
            var rotation=Quaternion.Euler(Pitch,Yaw,0);
            var offset=rotation*Vector3.back*distance;
            var length=distance;
            if(Physics.SphereCast(focus,.22f,offset.normalized,out var hit,distance,1<<0,QueryTriggerInteraction.Ignore)) length=Mathf.Max(1.3f,hit.distance-.15f);
            transform.position=focus+offset.normalized*length;
            transform.rotation=rotation;
        }
    }
}
