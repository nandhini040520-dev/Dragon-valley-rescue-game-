using UnityEngine;
namespace DragonValley
{
    public class TikoController : RobotController
    {
        Transform held,previous;
        public void Grab(Transform item)
        {
            if(!item) return; held=item; previous=item.parent;
            var motion=Visual?Visual.GetComponent<FunobotzMotion>():null;
            var socket=motion?motion.CarrySocket:null;
            item.SetParent(socket?socket:transform,true); item.localPosition=socket?Vector3.zero:Vector3.up*1.2f;
            if(motion) motion.Act();
        }
        public void Release() { if(held) held.SetParent(previous,true); held=null; }
    }
}
