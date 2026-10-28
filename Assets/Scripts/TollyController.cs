namespace DragonValley
{
    public class TollyController : RobotController
    {
        public void OperateGate() { AdventureGame.Instance.Audio.Play("gate open"); Celebrate(); }
    }
}
