namespace DragonValley
{
    public interface IInteractable
    {
        string DisplayName { get; }
        string Inspection { get; }
        void Inspect();
        bool Interact();
    }
}
