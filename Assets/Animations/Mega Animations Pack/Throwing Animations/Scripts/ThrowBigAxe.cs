///////////////////////////////////////////////////////////////////////////
//  ThrowBigAxe                                                          //
//  Kevin Iglesias - https://www.keviniglesias.com/       			     //
//  Contact Support: support@keviniglesias.com                           //
///////////////////////////////////////////////////////////////////////////

using System.Collections;
using UnityEngine;

namespace KevinIglesias
{

    public class ThrowBigAxe : MonoBehaviour
    {

        //Retargeter
        public Transform retargeter;

        //Prop to move
        public Transform propToSpin;

        //Hand that holds the prop
        public Transform hand;

        //How far will the prop launched
        public float spinDistance;

        //Movement speed of the prop
        public float translationSpeed;

        //Rotation speed of the prop
        public float spinSpeed;

        //Needed for check if the trick is active
        public bool spinActive = false;

        //Offset for fitting the prop in end distance
        public Vector3 endPositionOffset;

        //Offset for fitting the prop in hand when returning
        public Vector3 returningPositionOffset;

        //Character root (for parenting when prop is thrown)
        private Transform characterRoot;
        //Needed for getting the prop back
        private Vector3 zeroPosition;
        private Quaternion zeroRotation;
        //Needed for calculate prop trajectory
        private Vector3 startPosition;
        private Quaternion startRotation;
        private Vector3 endPosition;
        private Quaternion endRotation;
        //Coroutine that will make the prop move
        private IEnumerator spinCO;

        public void Awake()
        {
            this.characterRoot = this.transform;

            this.zeroPosition = this.propToSpin.localPosition;
            this.zeroRotation = this.propToSpin.localRotation;
        }

        public void Update()
        {
            if (this.retargeter.localPosition.y > 0)
            {
                if (!this.spinActive)
                {
                    this.SpinProp();
                    this.spinActive = true;
                }
            }
            else
            {

                if (this.spinActive)
                {
                    if (this.spinCO != null)
                    {
                        this.StopCoroutine(this.spinCO);
                    }
                    this.propToSpin.SetParent(this.hand);
                    this.propToSpin.localPosition = this.zeroPosition;
                    this.propToSpin.localRotation = this.zeroRotation;
                }
                this.spinActive = false;
            }
        }

        //Function called when retargeter is active
        public void SpinProp()
        {
            if (this.spinCO != null)
            {
                this.StopCoroutine(this.spinCO);
            }
            this.spinCO = this.StartSpin();
            this.StartCoroutine(this.spinCO);
        }

        private IEnumerator StartSpin()
        {
            //Remove prop from hand
            this.propToSpin.SetParent(this.characterRoot);

            //Get initial position/rotation
            this.startPosition = this.propToSpin.position;
            this.startRotation = this.propToSpin.localRotation;

            //Set end position (farthest point the prop will get)
            this.endPosition = new Vector3(this.propToSpin.position.x - this.spinDistance, this.propToSpin.position.y, this.propToSpin.position.z);
            this.endPosition = this.endPosition + this.endPositionOffset;

            //Going away
            float i = 0;
            while (i < 1f)
            {

                i += Time.deltaTime * this.translationSpeed;

                this.propToSpin.position = Vector3.Lerp(this.startPosition, this.endPosition, Mathf.Sin(i * Mathf.PI * 0.5f));
                this.propToSpin.transform.Rotate(0.0f, -this.spinSpeed, 0.0f, Space.World);
                yield return 0;
            }

            //Coming back
            i = 0;
            while (i < 1f)
            {
                i += Time.deltaTime * this.translationSpeed;

                this.propToSpin.position = Vector3.Lerp(this.endPosition, this.startPosition + this.returningPositionOffset, 1f - Mathf.Cos(i * Mathf.PI * 0.5f));
                this.propToSpin.transform.Rotate(0f, -this.spinSpeed, 0.0f, Space.World);

                yield return 0;
            }
        }

    }
}