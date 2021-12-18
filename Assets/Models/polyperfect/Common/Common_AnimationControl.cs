using UnityEngine;

namespace PolyPerfect
{
    public class Common_AnimationControl : MonoBehaviour
    {
        private string currentAnimation = "";

        // Use this for initialization
        private void Start()
        {

        }

        // Update is called once per frame
        private void Update()
        {


        }
        public void SetAnimation(string animationName)
        {

            if (this.currentAnimation != "")
            {
                this.GetComponent<Animator>().SetBool(this.currentAnimation, false);
            }
            this.GetComponent<Animator>().SetBool(animationName, true);
            this.currentAnimation = animationName;
        }

        public void SetAnimationIdle()
        {

            if (this.currentAnimation != "")
            {
                this.GetComponent<Animator>().SetBool(this.currentAnimation, false);
            }


        }
        public void SetDeathAnimation(int numOfClips)
        {

            int clipIndex = Random.Range(0, numOfClips);
            string animationName = "Death";
            Debug.Log(clipIndex);

            this.GetComponent<Animator>().SetInteger(animationName, clipIndex);
        }
    }
}