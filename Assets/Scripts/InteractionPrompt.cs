namespace DragonValley
{
    public static class InteractionPrompt
    {
        public static string Text(InteractableBase target)
        {
            if(!target) return "Follow the path to your next discovery";
            return target.action==ActionKind.Rescue ? "E  —  RESTORE DRAGON VALLEY" : "E  —  INTERACT    /    F  —  INSPECT";
        }
    }
}
