///////////////////////////////////////////////////////////////////////////
//  ThrowMultipleProps                                                   //
//  Kevin Iglesias - https://www.keviniglesias.com/       			     //
//  Contact Support: support@keviniglesias.com                           //
///////////////////////////////////////////////////////////////////////////

using UnityEngine;

namespace KevinIglesias
{

    public class ThrowMultipleProps : MonoBehaviour
    {

        //Retargeters
        public Transform retargeter1;
        public Transform retargeter2;

        //Props to move
        public Transform propToThrow1;
        public Transform propToThrow2;

        //Hands that holds the props
        public Transform hand1;
        public Transform hand2;

        //Target to throw the prop
        public Transform targetPos;

        //Speed of the prop
        public float speed = 10;

        //Maximum arc the prop will make
        public float arcHeight = 1;

        //Needed for checking if prop was thrown or not
        public bool launched1 = false;
        public bool launched2 = false;
        public bool recoverProp1 = false;
        public bool recoverProp2 = false;

        //Needed for checking if prop landed
        public bool propLanded1 = false;
        public bool propLanded2 = false;

        //Character root (for parenting when prop is thrown)
        private Transform characterRoot;

        //Needed for calculate prop trajectory
        private Vector3 startPos1;
        private Vector3 startPos2;
        private Vector3 zeroPosition1;
        private Quaternion zeroRotation1;
        private Vector3 zeroPosition2;
        private Quaternion zeroRotation2;
        private Vector3 nextPos;

        private void Start()
        {
            this.characterRoot = this.transform;

            this.zeroPosition1 = this.propToThrow1.localPosition;
            this.zeroRotation1 = this.propToThrow1.localRotation;
            this.zeroPosition2 = this.propToThrow2.localPosition;
            this.zeroRotation2 = this.propToThrow2.localRotation;
        }

        //This will make the prop move when launched
        private void Update()
        {
            //Arc throw prop 1
            if (this.launched1 && !this.propLanded1)
            {
                float x0 = this.startPos1.x;
                float x1 = this.targetPos.position.x;
                float dist = x1 - x0;
                float nextX = Mathf.MoveTowards(this.propToThrow1.position.x, x1, this.speed * Time.deltaTime);
                float baseY = Mathf.Lerp(this.startPos1.y, this.targetPos.position.y, (nextX - x0) / dist);
                float arc = this.arcHeight * (nextX - x0) * (nextX - x1) / (-0.25f * dist * dist);
                Vector3 nextPos = new Vector3(nextX, baseY + arc, this.propToThrow1.position.z);

                this.propToThrow1.rotation = LookAt2D(nextPos - this.propToThrow1.position);
                this.propToThrow1.position = nextPos;

                float currentDistance = Mathf.Abs(this.targetPos.position.x - this.propToThrow1.position.x);
                if (currentDistance < 0.5f)
                {
                    this.propLanded1 = true;
                }

            }

            //Arc throw prop 2
            if (this.launched2 && !this.propLanded2)
            {
                float x0 = this.startPos2.x;
                float x1 = this.targetPos.position.x;
                float dist = x1 - x0;
                float nextX = Mathf.MoveTowards(this.propToThrow2.position.x, x1, this.speed * Time.deltaTime);
                float baseY = Mathf.Lerp(this.startPos2.y, this.targetPos.position.y, (nextX - x0) / dist);
                float arc = this.arcHeight * (nextX - x0) * (nextX - x1) / (-0.25f * dist * dist);
                Vector3 nextPos = new Vector3(nextX, baseY + arc, this.propToThrow2.position.z);

                this.propToThrow2.rotation = LookAt2D(nextPos - this.propToThrow2.position);
                this.propToThrow2.position = nextPos;

                float currentDistance = Mathf.Abs(this.targetPos.position.x - this.propToThrow2.position.x);
                if (currentDistance < 0.5f)
                {
                    this.propLanded2 = true;
                }

            }

            if (this.retargeter1.localPosition.y > 0)
            {
                if (!this.launched1 && !this.recoverProp1)
                {
                    this.Throw1();
                }

                if (this.launched1 && this.recoverProp1)
                {
                    this.RecoverProp1();
                }
            }
            else
            {

                if (!this.recoverProp1 && this.launched1)
                {
                    this.recoverProp1 = true;
                }

                if (this.recoverProp1 && !this.launched1)
                {
                    this.recoverProp1 = false;
                }
            }

            if (this.retargeter2.localPosition.y > 0)
            {
                if (!this.launched2 && !this.recoverProp2)
                {
                    this.Throw2();
                }

                if (this.launched2 && this.recoverProp2)
                {
                    this.RecoverProp2();
                }
            }
            else
            {

                if (!this.recoverProp2 && this.launched2)
                {
                    this.recoverProp2 = true;
                }

                if (this.recoverProp2 && !this.launched2)
                {
                    this.recoverProp2 = false;
                }
            }

        }

        private static Quaternion LookAt2D(Vector3 forward)
        {
            return Quaternion.Euler(0, 0, (Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg) - 90);
        }

        //Function called when retargeter1 is active
        public void Throw1()
        {
            this.startPos1 = this.propToThrow1.position;
            this.propToThrow1.SetParent(this.characterRoot);
            this.launched1 = true;
        }

        //Function called when retargeter2 is active
        public void Throw2()
        {
            this.startPos2 = this.propToThrow2.position;
            this.propToThrow2.SetParent(this.characterRoot);
            this.launched2 = true;
        }

        //Function called when retargeter1 is active
        public void RecoverProp1()
        {
            this.propLanded1 = false;
            this.launched1 = false;
            this.propToThrow1.SetParent(this.hand1);
            this.propToThrow1.localPosition = this.zeroPosition1;
            this.propToThrow1.localRotation = this.zeroRotation1;
        }

        //Function called when retargeter2 is active
        public void RecoverProp2()
        {
            this.propLanded2 = false;
            this.launched2 = false;
            this.propToThrow2.SetParent(this.hand2);
            this.propToThrow2.localPosition = this.zeroPosition2;
            this.propToThrow2.localRotation = this.zeroRotation2;
        }
    }

}