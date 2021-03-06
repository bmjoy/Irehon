public interface IAbilitySystem
{
    void AbilityKeyDown(UnityEngine.KeyCode key, UnityEngine.Vector3 target);
    void AbilityKeyUp(UnityEngine.KeyCode key, UnityEngine.Vector3 target);
    void TrySetAbilityToSlot(int slot, int id);
    void TryUnlockAbility(int id);
    bool IsAbilityCasting();
}
