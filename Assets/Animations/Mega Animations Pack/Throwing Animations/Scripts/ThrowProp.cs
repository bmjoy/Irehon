///////////////////////////////////////////////////////////////////////////
//  ThrowProp                                                            //
//  Kevin Iglesias - https://www.keviniglesias.com/       			     //
//  Contact Support: support@keviniglesias.com                           //
///////////////////////////////////////////////////////////////////////////

using UnityEngine;

namespace KevinIglesias
{

    public enum PropType
    {
        Spear,
        Knife,
        Tomahawk,
    }

    public class ThrowProp : MonoBehaviour
    {

        //Retargeter
        public Transform retargeter;

        //Different movements for different prop types
        public PropType propType;

        //Prop to move
        public Transform propToThrow;
        //Hand that holds the prop
        public Transform hand;

        //Target to throw the prop
        public Transform targetPos;

        //Speed of the prop
        public float speed = 10;

        //Maximum arc the prop will make
        public float arcHeight = 1;

        //Needed for checking if prop was thrown or not
        public bool launched = false;
        public bool recoverProp = false;

        //Needed for checking if prop landed
        public bool propLanded = false;

        //Character root (for parenting when prop is thrown)
        private Transform characterRoot;
        //Needed for calculate prop trajectory
        private Vector3 startPos;
        private Vector3 zeroPosition;
        private Quaternion zeroRotation;
        private Vector3 nextPos;

        private void Start()
        {
            this.characterRoot = this.transform;

            this.zeroPosition = this.propToThrow.localPosition;
            this.zeroRotation = this.propToThrow.localRotation;
        }

        //This will make the prop move when launched
        private void Update()
        {
            //Arc throw facing the target
            if (this.launched && (this.propType == PropType.Spear || this.propType == PropType.Knife) && !this.propLanded)
            {
                float x0 = this.startPos.x;
                float x1 = this.targetPos.position.x;
                float dist = x1 - x0;
                float nextX = Mathf.MoveTowards(this.propToThrow.position.x, x1, this.speed * Time.deltaTime);
                float baseY = Mathf.Lerp(this.startPos.y, this.targetPos.position.y, (nextX - x0) / dist);
                float arc = this.arcHeight * (nextX - x0) * (nextX - x1) / (-0.25f * dist * dist);
                Vector3 nextPos = new Vector3(nextX, baseY + arc, this.propToThrow.position.z);

                this.propToThrow.rotation = LookAt2D(nextPos - this.propToThrow.position);
                this.propToThrow.position = nextPos;

                float currentDistance = Mathf.Abs(this.targetPos.position.x - this.propToThrow.position.x);
                if (currentDistance < 0.5f)
                {
                    this.propLanded = true;
                }

            }

            //Arc throw rotating forwards
            if (this.launched && this.propType == PropType.Tomahawk && !this.propLanded)
            {
                float x0 = this.startPos.x;
                float x1 = this.targetPos.position.x;
                float dist = x1 - x0;
                float nextX = Mathf.MoveTowards(this.propToThrow.position.x, x1, this.speed * Time.deltaTime);
                float baseY = Mathf.Lerp(this.startPos.y, this.targetPos.position.y, (nextX - x0) / dist);
                float arc = this.arcHeight * (nextX - x0) * (nextX - x1) / (-0.25f * dist * dist);
                Vector3 nextPos = new Vector3(nextX, baseY + arc, this.propToThrow.position.z);

                this.propToThrow.transform.Rotate(19f, 0.0f, 0.0f, Space.Self);
                this.propToThrow.position = nextPos;

                float currentDistance = Mathf.Abs(this.targetPos.position.x - this.propToThrow.position.x);
                if (currentDistance < 0.5f)
                {
                    this.propLanded = true;
                }
            }

            if (this.retargeter.localPosition.y > 0)
            {
                if (!this.launched && !this.recoverProp)
                {
                    this.Throw();
                }

                if (this.launched && this.recoverProp)
                {
                    this.RecoverProp();
                }
            }
            else
            {

                if (!this.recoverProp && this.launched)
                {
                    this.recoverProp = true;
                }

                if (this.recoverProp && !this.launched)
                {
                    this.recoverProp = false;
                }
            }
        }

        private static Quaternion LookAt2D(Vector3 forward)
        {
            return Quaternion.Euler(0, 0, (Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg) - 90f);
        }

        //Function called when retargeter is active
        public void Throw()
        {
            this.launched = true;
            this.startPos = this.propToThrow.position;
            this.propToThrow.SetParent(this.characterRoot);
        }

        //Function called when retargeter is active
        public void RecoverProp()
        {
            this.propLanded = false;
            this.launched = false;
            this.propToThrow.SetParent(this.hand);
            this.propToThrow.localPosition = this.zeroPosition;
            this.propToThrow.localRotation = this.zeroRotation;
        }
    }

}