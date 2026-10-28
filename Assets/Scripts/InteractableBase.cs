using UnityEngine;
namespace DragonValley
{
    public class InteractableBase : MonoBehaviour, IInteractable
    {
        public string objectiveId, displayName, inspection;
        public int region;
        public RobotKind requiredRobot=RobotKind.None;
        public ActionKind action;
        public bool needsInspection, inspected, done;
        public Transform movable, destination;
        public InteractableBase source;
        public Vector3 originalPosition;
        public MechanismController Mechanism;
        public MechanismAction Operation;
        public string DisplayName=>displayName;
        public string Inspection=>inspection;
        public bool Relevant => !done && AdventureGame.Instance && region==AdventureGame.Instance.Missions.Region;
        public void Inspect()
        {
            inspected=true;
            if(Mechanism) { Mechanism.Inspect(Operation==MechanismAction.Clue); return; }
            AdventureGame.Instance.Notify(displayName,inspection);
            AdventureGame.Instance.Audio.Play("interact");
        }
        public bool Interact() => AdventureGame.Instance.Interactions.TryInteract(this);
        public void ResetObject()
        {
            if(movable) movable.position=originalPosition;
        }
    }
}
