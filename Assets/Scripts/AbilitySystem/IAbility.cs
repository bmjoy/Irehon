public interface IAbility
{
    int Id { get; }
    string Title { get; }
    string Describe { get; }
    AbilityCooldownEvent OnAbilityCooldown { get; set; }
    AbilityUnlockRequirment UnlockRequirment { get; }
    UnityEngine.Sprite AbilityIcon { get; }
    UnityEngine.KeyCode TriggerKey { get; }
    
    bool TriggerKeyDown(UnityEngine.Vector3 target);
    
    void TriggerKeyUp(UnityEngine.Vector3 target);

    void AnimationEvent();

    void Interrupt();
}
