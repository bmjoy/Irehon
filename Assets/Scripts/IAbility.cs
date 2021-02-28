public interface IAbility
{
    AbilityCooldownEvent OnAbilityCooldown { get; set; }
    UnityEngine.Sprite AbilityIcon { get; }
    UnityEngine.KeyCode TriggerKey { get; }
    
    bool TriggerKeyDown(UnityEngine.Vector3 target);
    
    void TriggerKeyUp(UnityEngine.Vector3 target);

    void AnimationEvent();

    void AbilityInit(AbilitySystem abilitySystem);

    void Interrupt();
}
