using UnityEngine;
namespace DragonValley
{
    public class PetaloController : RobotController
    {
        public Light Lamp;
        public void SelectPetalo() { AdventureGame.Instance.Robots.Select(RobotKind.Petalo); }
        public void Illuminate() { if(Lamp) { Lamp.enabled=true; Lamp.intensity=1.3f; Lamp.range=13; } }
        public void ActivateLightSwitch() { Illuminate(); Celebrate(); }
    }
}
