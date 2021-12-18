namespace DuloGames.UI
{
    public interface IUISlotHasCooldown
    {
        UISpellInfo GetSpellInfo();
        UISlotCooldown cooldownComponent { get; }
        void SetCooldownComponent(UISlotCooldown cooldown);
    }
}
