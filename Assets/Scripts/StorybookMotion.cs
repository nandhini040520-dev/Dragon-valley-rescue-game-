using UnityEngine;
namespace DragonValley
{
    public class StorybookMotion : MonoBehaviour
    {
        Transform leftLeg,rightLeg,leftArm,rightArm,leftWing,rightWing;
        Vector3 last;
        void Start() { leftLeg=transform.Find("Leg L"); rightLeg=transform.Find("Leg R"); leftArm=transform.Find("Arm L"); rightArm=transform.Find("Arm R"); leftWing=transform.Find("Wing L"); rightWing=transform.Find("Wing R"); last=transform.position; }
        void LateUpdate()
        {
            if(Time.deltaTime<=0) return;
            float speed=(transform.position-last).magnitude/Time.deltaTime; last=transform.position;
            float swing=Mathf.Sin(Time.time*9)*Mathf.Clamp(speed/6,0,1)*24;
            if(leftLeg) leftLeg.localRotation=Quaternion.Euler(swing,0,0); if(rightLeg) rightLeg.localRotation=Quaternion.Euler(-swing,0,0);
            if(leftArm) leftArm.localRotation=Quaternion.Euler(-swing*.7f,0,0); if(rightArm) rightArm.localRotation=Quaternion.Euler(swing*.7f,0,0);
            if(leftWing) leftWing.localRotation=Quaternion.Euler(0,Mathf.Sin(Time.time*1.3f)*4,Mathf.Sin(Time.time*1.3f)*3);
            if(rightWing) rightWing.localRotation=Quaternion.Euler(0,-Mathf.Sin(Time.time*1.3f)*4,-Mathf.Sin(Time.time*1.3f)*3);
        }
    }
}
