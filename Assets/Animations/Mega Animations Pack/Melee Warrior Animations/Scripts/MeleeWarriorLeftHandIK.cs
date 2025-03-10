﻿///////////////////////////////////////////////////////////////////////////
//  Melee Warrior Left Hand IK - MonoBehaviour Script				     //
//  Kevin Iglesias - https://www.keviniglesias.com/     			     //
//  Contact Support: support@keviniglesias.com                           //
///////////////////////////////////////////////////////////////////////////

// This script will enable IK of the left hand when needed in the animation
// used on your custom 3D model to make animations that holds weapons with
// both hands look good. It requires the 'Retargeters' empty gameobjects 
// and a child called 'LeftHandIK'. 
// More information at Documentation PDF file.

using UnityEngine;

namespace KevinIglesias
{
    public class MeleeWarriorLeftHandIK : MonoBehaviour
    {
        public Transform retargeter;
        public Transform leftHandEffector;

        private Animator animator;

        private float weight;

        private void Awake()
        {
            this.animator = this.GetComponent<Animator>();
            this.weight = 0f;
        }

        private void Update()
        {
            this.weight = this.retargeter.localPosition.y;
        }

        private void OnAnimatorIK(int layerIndex)
        {
            this.animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, this.weight);
            this.animator.SetIKPosition(AvatarIKGoal.LeftHand, this.leftHandEffector.position);
        }
    }
}
