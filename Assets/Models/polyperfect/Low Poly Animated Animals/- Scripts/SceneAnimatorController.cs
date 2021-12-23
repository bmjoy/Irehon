using UnityEngine;

public class SceneAnimatorController : MonoBehaviour
{
    private Animator animator;

    private void Start()
    {
        this.animator = this.GetComponent<Animator>();
    }

    public void SetAnimatorString(string value)
    {
        for (int i = 0; i < this.animator.parameterCount; i++)
        {
            this.animator.SetBool(this.animator.parameters[i].name, false);
        }

        if (value != "Idle")
        {
            this.animator.SetBool(value, true);
        }
    }
}
