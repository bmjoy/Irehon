namespace DuloGames.UI
{
    public interface IUISpellSlot
    {
        UISpellInfo GetSpellInfo();
        bool Assign(UISpellInfo spellInfo);
        void Unassign();
    }
}
