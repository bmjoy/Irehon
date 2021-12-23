///////////////////////////////////////////////////////////////////////////
//  ChangeSpear                                                          //
//  Kevin Iglesias - https://www.keviniglesias.com/       			     //
//  Contact Support: support@keviniglesias.com                           //
///////////////////////////////////////////////////////////////////////////

using System.Collections;
using UnityEngine;

namespace KevinIglesias
{

    public class ChangeSpear : MonoBehaviour
    {

        //Retargeter
        public Transform retargeter;

        //Prop to move
        public Transform spear;

        //Hand that holds the prop
        public Transform hand;

        //Needed for check if the change is active
        public bool changeActive = false;

        //Needed for check if the change is even or not
        public bool secondTime = false;

        //Character root (for parenting when prop is thrown)
        private Transform characterRoot;
        //Needed for getting the prop back
        private Vector3 zeroPosition;
        private Vector3 zeroRotation;
        //Needed for calculate prop trajectory
        private Vector3 startPosition;
        private Quaternion startRotation;
        private Vector3 endPosition;
        private Quaternion endRotation;
        //Coroutine that will make the prop move
        private IEnumerator changeCO;

        public void Start()
        {
            this.characterRoot = this.transform;

            this.zeroPosition = this.spear.localPosition;
            this.zeroRotation = this.spear.localEulerAngles;
        }

        public void Update()
        {
            if (this.retargeter.localPosition.y > 0)
            {
                if (!this.changeActive)
                {
                    this.DoChangeSpear();
                    this.changeActive = true;
                }
            }
            else
            {
                this.changeActive = false;
            }
        }

        //Function called when retargeter is active
        public void DoChangeSpear()
        {
            if (this.changeCO != null)
            {
                this.StopCoroutine(this.changeCO);
            }
            this.changeCO = this.StartChange();
            this.StartCoroutine(this.changeCO);
        }

        private IEnumerator StartChange()
        {
            //Remove prop from hand
            this.spear.SetParent(this.characterRoot);

            //Get initial position/rotation
            this.startPosition = this.spear.position;
            this.startRotation = this.spear.localRotation;

            //Set end position (highest point the prop will get)
            this.endPosition = new Vector3(this.spear.position.x, this.spear.position.y + 0.2f, this.spear.position.z);

            //Rotation is different each time
            float yRotation = 0.70f;
            if (this.secondTime)
            {
                yRotation = -0.25f;
            }

            //Going up
            float i = 0;
            while (i < 1)
            {
                i += Time.deltaTime * 5f;
                this.spear.position = Vector3.Lerp(this.startPosition, this.endPosition, Mathf.Sin(i * Mathf.PI * 0.5f));
                this.spear.transform.Rotate(0.70f, yRotation, 0f, Space.World);
                yield return 0;
            }

            this.startPosition = new Vector3(this.startPosition.x, this.startPosition.y, this.startPosition.z);

            //Going down
            i = 0;
            while (i < 1)
            {
                i += Time.deltaTime * 5f;
                this.spear.position = Vector3.Lerp(this.endPosition, this.startPosition, 1f - Mathf.Cos(i * Mathf.PI * 0.5f));
                this.spear.transform.Rotate(0.70f, yRotation, 0f, Space.World);
                yield return 0;
            }

            //Back to the hand
            this.spear.SetParent(this.hand);
            this.spear.localPosition = this.zeroPosition;
            this.spear.localEulerAngles = this.zeroRotation;

            //Get the correct rotation of the spearhead
            if (!this.secondTime)
            {
                this.spear.localEulerAngles = new Vector3(this.spear.localEulerAngles.x + 180f, this.spear.localEulerAngles.y, this.spear.localEulerAngles.z);
                this.secondTime = true;
            }
            else
            {
                this.secondTime = false;
            }
        }
    }
}