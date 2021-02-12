interface IAbility
{
    UnityEngine.KeyCode TriggerKey { get; }
    
    bool TriggerKeyDown(UnityEngine.Vector3 target);
    
    void TriggerKeyUp(UnityEngine.Vector3 target);

    void AnimationEvent();

    void Interrupt();
}
