///////////////////////////////////////////////////////////////////////////
//  VillagerHandsIK - MonoBehaviour Script				                 //
//  Kevin Iglesias - https://www.keviniglesias.com/     			     //
//  Contact Support: support@keviniglesias.com                           //
///////////////////////////////////////////////////////////////////////////

using UnityEngine;

namespace KevinIglesias
{

    public enum VillagerIKGoal { LeftHand = 0, RightHand = 1 };

    public class VillagerHandsIK : MonoBehaviour
    {
        public Transform retargeter;
        public Transform handEffector;

        public VillagerIKGoal hand;

        private Animator animator;
        private float weight;

        private void Awake()
        {
            this.animator = this.GetComponent<Animator>();
            this.weight = 0f;
        }

        private void Update()
        {

            this.weight = Mathf.Lerp(0, 1, 1f - Mathf.Cos(this.retargeter.localPosition.y * Mathf.PI * 0.5f));

        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (this.hand == VillagerIKGoal.LeftHand)
            {
                this.animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, this.weight);
                this.animator.SetIKPosition(AvatarIKGoal.LeftHand, this.handEffector.position);

                this.animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, this.weight);
                this.animator.SetIKRotation(AvatarIKGoal.LeftHand, this.handEffector.rotation);
            }
            else
            {
                this.animator.SetIKPositionWeight(AvatarIKGoal.RightHand, this.weight);
                this.animator.SetIKPosition(AvatarIKGoal.RightHand, this.handEffector.position);

                this.animator.SetIKRotationWeight(AvatarIKGoal.RightHand, this.weight);
                this.animator.SetIKRotation(AvatarIKGoal.RightHand, this.handEffector.rotation);
            }
        }
    }
}
