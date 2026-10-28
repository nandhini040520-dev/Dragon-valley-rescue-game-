using UnityEngine;
namespace DragonValley
{
    public class FunobotzMotion : MonoBehaviour
    {
        public bool AnimateCrawling;
        public bool Working;
        public Transform CarrySocket;
        Transform gate,arm,left,right;
        Vector3 previousPosition;
        float actionUntil;
        void Start()
        {
            gate=transform.Find("Tollgate pivot"); arm=transform.Find("Segmented arm pivot");
            left=transform.Find("Flex arm L"); right=transform.Find("Flex arm R");
            previousPosition=transform.position;
        }
        public void Act() { actionUntil=Time.time+2; }
        void LateUpdate()
        {
            if(Time.deltaTime<=0) return;
            bool active=Time.time<actionUntil || Working;
            if(gate) gate.localRotation=Quaternion.Slerp(gate.localRotation,Quaternion.Euler(0,0,active?78:0),Time.deltaTime*5);
            if(arm) arm.localRotation=Quaternion.Slerp(arm.localRotation,Quaternion.Euler(0,0,active?35:0),Time.deltaTime*4);
            float speed=Vector3.Distance(transform.position,previousPosition)/Time.deltaTime; previousPosition=transform.position;
            float crawl=AnimateCrawling && speed>.2f?Mathf.Sin(Time.time*7)*9:0;
            if(left) left.localRotation=Quaternion.Euler(crawl,0,0);
            if(right) right.localRotation=Quaternion.Euler(-crawl,0,0);
        }
    }
}
