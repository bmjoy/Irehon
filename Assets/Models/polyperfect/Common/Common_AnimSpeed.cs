using UnityEngine;

namespace PolyPerfect
{
    public class Common_AnimSpeed : MonoBehaviour
    {
        private Animator anim;
        private float Speed;

        // Use this for initialization
        private void Start()
        {
            this.Speed = Random.Range(0.85f, 1.25f);
            this.anim = this.GetComponent<Animator>();
        }

        // Update is called once per frame
        private void Update()
        {
            this.anim.speed = this.Speed;
        }
    }
}