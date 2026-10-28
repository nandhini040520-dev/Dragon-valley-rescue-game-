using UnityEngine;
namespace DragonValley
{
    public class RobotTeam : MonoBehaviour
    {
        public RobotController[] Members=new RobotController[4];
        public RobotKind Selected { get; private set; } = RobotKind.Petalo;
        public PetaloController Petalo => (PetaloController)Members[0];
        public QuackyController Quacky => (QuackyController)Members[1];
        public TollyController Tolly => (TollyController)Members[2];
        public TikoController Tiko => (TikoController)Members[3];
        public RobotController Get(RobotKind kind) => Members[(int)kind];
        public void Select(RobotKind kind)
        {
            if(kind==RobotKind.None) return; Selected=kind;
            for(int i=0;i<4;i++) if(Members[i] && Members[i].SelectionRing) Members[i].SelectionRing.SetActive(i==(int)kind);
            if(kind==RobotKind.Petalo) Petalo.Illuminate();
        }
        void Update()
        {
            if(!AdventureGame.Instance.IsPlaying || AdventureGame.Autopilot) return;
            for(int i=0;i<4;i++) if(Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1+i))) { Select((RobotKind)i); AdventureGame.Instance.Audio.Play("interact"); }
        }
    }
}
