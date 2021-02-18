public interface IAbilitySystem
{
    void AbilityKeyDown(UnityEngine.KeyCode key, UnityEngine.Vector3 target);
    void AbilityKeyUp(UnityEngine.KeyCode key, UnityEngine.Vector3 target);

    bool IsAbilityCasting();
}
