using UnityEngine;

namespace KevinIglesias
{

    public class ThrowHealing : StateMachineBehaviour
    {
        private CastSpells cS;

        public CastHand castHand;

        public float spawnDelay;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

            if (this.cS == null)
            {
                this.cS = animator.GetComponent<CastSpells>();
            }

            if (this.cS != null)
            {
                this.cS.ThrowHealing(this.castHand, this.spawnDelay);
            }
        }
    }
}
