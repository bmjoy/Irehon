using UnityEngine;

public class ProjectorMaterialProperties : MonoBehaviour
{
    private float value = 0.01f;
    private float TimeRate = 0;
    public float Timer = 2f;
    private bool Undo;
    public bool opacity = false;
    private Material mat;

    private void Start()
    {
        this.Undo = false;
        Projector proj = this.GetComponent<Projector>();
        if (!proj.material.name.EndsWith("(Instance)"))
        {
            proj.material = new Material(proj.material) { name = proj.material.name + " (Instance)" };
        }

        this.mat = proj.material;
    }

    private void Update()
    {
        if (this.opacity == true && this.TimeRate <= this.Timer && this.Undo == false)
        {
            this.TimeRate += Time.deltaTime;
            this.value = Mathf.Lerp(1f, 0f, this.TimeRate / this.Timer);
            this.mat.SetFloat("_Opacity", this.value);
        }

        if (this.opacity == false)
        {
            if (this.TimeRate <= this.Timer && this.Undo == false)
            {
                this.TimeRate += Time.deltaTime;
                this.value = Mathf.Lerp(0.01f, 4f, this.TimeRate / this.Timer);
                this.mat.SetFloat("_MoveCirle", this.value);
            }

            if (this.TimeRate >= this.Timer && this.Undo == false)
            {
                this.check();
            }

            if (this.Undo == true && this.TimeRate <= this.Timer)
            {
                this.TimeRate += Time.deltaTime;
                this.value = Mathf.Lerp(4f, 0.01f, this.TimeRate / this.Timer);
                this.mat.SetFloat("_MoveCirle", this.value);
            }
        }
    }

    private void check()
    {
        this.Undo = true;
        this.TimeRate = 0;
    }
}
