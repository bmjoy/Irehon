///////////////////////////////////////////////////////////////////////////
//  Archer Animations - BowLoadScript                                    //
//  Kevin Iglesias - https://www.keviniglesias.com/     			     //
//  Contact Support: support@keviniglesias.com                           //
///////////////////////////////////////////////////////////////////////////

// This script makes the bow animate when pulling arrow, also it makes thes
// arrow look ready to shot when drawing it from the quivers.

// To do the pulling animation, the bow mesh needs a blenshape named 'Load' 
// and the character needs an empty Gameobject in your Unity scene named 
// 'ArrowLoad' as a child, see character dummies hierarchy from the demo 
// scene as example. More information at Documentation PDF file.

using UnityEngine;

namespace KevinIglesias
{
    public class BowLoadScript : MonoBehaviour
    {

        public Transform bow;
        public Transform arrowLoad;

        //Bow Blendshape
        private SkinnedMeshRenderer bowSkinnedMeshRenderer;

        //Arrow draw & rotation
        public bool arrowOnHand;
        public Transform arrowToDraw;
        public Transform arrowToShoot;

        private void Awake()
        {

            if (this.bow != null)
            {
                this.bowSkinnedMeshRenderer = this.bow.GetComponent<SkinnedMeshRenderer>();
            }

            if (this.arrowToDraw != null)
            {
                this.arrowToDraw.gameObject.SetActive(false);
            }
            if (this.arrowToShoot != null)
            {
                this.arrowToShoot.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            //Bow blendshape animation
            if (this.bowSkinnedMeshRenderer != null && this.bow != null && this.arrowLoad != null)
            {
                float bowWeight = Mathf.InverseLerp(0, -0.7f, this.arrowLoad.localPosition.z);
                this.bowSkinnedMeshRenderer.SetBlendShapeWeight(0, bowWeight * 100);
            }

            //Draw arrow from quiver and rotate it
            if (this.arrowToDraw != null && this.arrowToShoot != null && this.arrowLoad != null)
            {
                if (this.arrowLoad.localPosition.y == 0.5f)
                {
                    if (this.arrowToDraw != null)
                    {
                        this.arrowOnHand = true;
                        this.arrowToDraw.gameObject.SetActive(true);
                    }
                }

                if (this.arrowLoad.localPosition.y > 0.5f)
                {
                    if (this.arrowToDraw != null && this.arrowToShoot != null)
                    {
                        this.arrowToDraw.gameObject.SetActive(false);
                        this.arrowToShoot.gameObject.SetActive(true);
                    }
                }

                if (this.arrowLoad.localScale.z < 1f)
                {
                    if (this.arrowToShoot != null)
                    {
                        this.arrowToShoot.gameObject.SetActive(false);
                        this.arrowOnHand = false;
                    }
                }
            }
        }
    }
}
