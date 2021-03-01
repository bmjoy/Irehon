public interface IAbility
{
    int Id { get; }
    AbilityCooldownEvent OnAbilityCooldown { get; set; }
    UnityEngine.Sprite AbilityIcon { get; }
    UnityEngine.KeyCode TriggerKey { get; }
    
    bool TriggerKeyDown(UnityEngine.Vector3 target);
    
    void TriggerKeyUp(UnityEngine.Vector3 target);

    void AnimationEvent();

    void Interrupt();
}
