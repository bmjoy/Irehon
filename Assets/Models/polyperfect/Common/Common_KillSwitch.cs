using UnityEngine;

namespace PolyPerfect
{
    public class Common_KillSwitch : MonoBehaviour
    {
        private Animator anim;

        // Use this for initialization
        private void Start()
        {

            this.anim = this.GetComponent<Animator>();

        }

        // Update is called once per frame
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                this.anim.SetBool("isDead", true);
            }
        }
    }
}
