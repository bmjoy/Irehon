using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ClassData : ScriptableObject
{
    public string ClassName;
    public AnimatorOverrideController classAnimator;
    public abstract void RightMouseButtonDown(Transform player);
    public abstract void RightMouseButtonUp(Transform player);
    public abstract void LeftMouseButtonDown(Transform player);
    public abstract void LeftMouseButtonUp(Transform player);
}
