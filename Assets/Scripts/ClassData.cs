using UnityEngine;

public abstract class ClassData : MonoBehaviour
{
    public AnimatorOverrideController classAnimator;

    private void Start()
    {
        IntializeClass();
    }

    public abstract void ResetClassState();

    public abstract void IntializeClass();

    public abstract void RightMouseButtonDown();
    public abstract void RightMouseButtonUp();
    public abstract void LeftMouseButtonDown();
    public abstract void LeftMouseButtonUp();

    public abstract void AttackEvent();
}
