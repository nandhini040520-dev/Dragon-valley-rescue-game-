using System.Collections;
using UnityEngine;
namespace DragonValley
{
    public class QuackyController : RobotController
    {
        public IEnumerator Deliver(Transform parcel,Vector3 destination)
        {
            Transform previous=null;
            if(parcel) { previous=parcel.parent; parcel.SetParent(transform,true); parcel.localPosition=Vector3.up*.7f; }
            yield return MoveToTarget(destination);
            if(parcel) { parcel.SetParent(previous,true); parcel.position=destination; }
            Celebrate();
        }
    }
}
