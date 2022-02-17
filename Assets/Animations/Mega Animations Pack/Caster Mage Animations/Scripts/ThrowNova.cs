using UnityEngine;

namespace KevinIglesias
{

    public class ThrowNova : StateMachineBehaviour
    {
        private CastSpells cS;

        public float spawnDelay;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

            if (this.cS == null)
            {
                this.cS = animator.GetComponent<CastSpells>();
            }

            if (this.cS != null)
            {
                this.cS.ThrowNova(this.spawnDelay);
            }
        }
    }
}
