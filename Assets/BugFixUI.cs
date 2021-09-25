using UnityEditor;
using UnityEngine;

public class BugFixUI : ScriptableObject
{
    [MenuItem("Assets/Unhide Fix")]
    private static void unhide()
    {
        UnityEditor.Animations.AnimatorController ac = Selection.activeObject as UnityEditor.Animations.AnimatorController;

        foreach (UnityEditor.Animations.AnimatorControllerLayer layer in ac.layers)
        {

            foreach (UnityEditor.Animations.ChildAnimatorState curState in layer.stateMachine.states)
            {
                if (curState.state.hideFlags != 0) curState.state.hideFlags = (HideFlags)1;
                if (curState.state.motion != null)
                {
                    if (curState.state.motion.hideFlags != 0) curState.state.motion.hideFlags = (HideFlags)1;
                }
            }

            foreach (UnityEditor.Animations.ChildAnimatorStateMachine curStateMachine in layer.stateMachine.stateMachines)
            {
                foreach (UnityEditor.Animations.ChildAnimatorState curState in curStateMachine.stateMachine.states)
                {
                    if (curState.state.hideFlags != 0) curState.state.hideFlags = (HideFlags)1;
                    if (curState.state.motion != null)
                    {
                        if (curState.state.motion.hideFlags != 0) curState.state.motion.hideFlags = (HideFlags)1;
                    }
                }
            }
        }
        EditorUtility.SetDirty(ac);
    }

    [MenuItem("Tools/Animator States Unhide Fix")]
    private static void Fix()
    {
        if (Selection.objects.Length < 1) throw new UnityException("Select animator controller(s) before try fix it!");
        int scnt = 0;
        foreach (Object o in Selection.objects)
        {
            UnityEditor.Animations.AnimatorController ac = o as UnityEditor.Animations.AnimatorController;
            if (ac == null) continue;
            foreach (UnityEditor.Animations.AnimatorControllerLayer layer in ac.layers)
            {
                foreach (UnityEditor.Animations.ChildAnimatorState curState in layer.stateMachine.states)
                {
                    if (curState.state.hideFlags != 0)
                    {
                        curState.state.hideFlags = (HideFlags)1;
                        scnt++;
                    }
                    if (curState.state.motion != null)
                    {
                        if (curState.state.motion.hideFlags != 0) curState.state.motion.hideFlags = (HideFlags)1;
                    }
                }
                foreach (UnityEditor.Animations.ChildAnimatorStateMachine curStateMachine in layer.stateMachine.stateMachines)
                {
                    foreach (UnityEditor.Animations.ChildAnimatorState curState in curStateMachine.stateMachine.states)
                    {
                        if (curState.state.hideFlags != 0)
                        {
                            curState.state.hideFlags = (HideFlags)1;
                            scnt++;
                        }
                        if (curState.state.motion != null)
                        {
                            if (curState.state.motion.hideFlags != 0) curState.state.motion.hideFlags = (HideFlags)1;
                        }
                    }
                }
            }
            EditorUtility.SetDirty(ac);
        }
        Debug.Log("Fixing " + scnt + " states done!");
    }
}