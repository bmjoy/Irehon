///////////////////////////////////////////////////////////////////////////
//  IdleThrowTrick                                                       //
//  Kevin Iglesias - https://www.keviniglesias.com/       			     //
//  Contact Support: support@keviniglesias.com                           //
///////////////////////////////////////////////////////////////////////////

using System.Collections;
using UnityEngine;

namespace KevinIglesias
{

    public class IdleThrowTrick : MonoBehaviour
    {

        //Retargeter
        public Transform retargeter;

        //Prop to move
        public Transform propToThrow;

        //Hand that holds the prop
        public Transform hand;

        //How far will the prop launched
        public float trickDistance;

        //Movement speed of the prop
        public float trickTranslationSpeed;

        //Rotation speed of the prop
        public float trickRotationSpeed;

        //Needed for check if the trick is active
        public bool trickActive = false;

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

        public void Start()
        {
            this.characterRoot = this.transform;

            this.zeroPosition = this.propToThrow.localPosition;
            this.zeroRotation = this.propToThrow.localRotation;
        }


        public void Update()
        {
            if (this.retargeter.localPosition.y > 0)
            {
                if (!this.trickActive)
                {
                    this.SpinProp();
                    this.trickActive = true;
                }
            }
            else
            {
                if (this.trickActive)
                {
                    if (this.spinCO != null)
                    {
                        this.StopCoroutine(this.spinCO);
                    }
                    this.propToThrow.SetParent(this.hand);
                    this.propToThrow.localPosition = this.zeroPosition;
                    this.propToThrow.localRotation = this.zeroRotation;
                }
                this.trickActive = false;
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


            //Get initial position/rotation
            this.startPosition = this.propToThrow.position;
            this.startRotation = this.propToThrow.localRotation;

            //Set end position (highest point the prop will get)
            this.endPosition = new Vector3(this.propToThrow.position.x, this.propToThrow.position.y + this.trickDistance, this.propToThrow.position.z);

            //Remove prop from hand
            this.propToThrow.SetParent(this.characterRoot);

            //Going up
            float i = 0;
            while (i < 1)
            {
                i += Time.deltaTime * this.trickTranslationSpeed;
                this.propToThrow.position = Vector3.Lerp(this.startPosition, this.endPosition, Mathf.Sin(i * Mathf.PI * 0.5f));
                this.propToThrow.transform.Rotate(0.0f, 0.0f, -this.trickRotationSpeed, Space.World);
                yield return 0;
            }

            this.startPosition = new Vector3(this.startPosition.x, this.startPosition.y - 0.11f, this.startPosition.z);

            //Going down
            i = 0;
            while (i < 1f)
            {
                i += Time.deltaTime * this.trickTranslationSpeed;
                this.propToThrow.position = Vector3.Lerp(this.endPosition, this.startPosition, 1f - Mathf.Cos(i * Mathf.PI * 0.5f));
                this.propToThrow.transform.Rotate(0f, 0.0f, -this.trickRotationSpeed, Space.World);
                yield return 0;
            }

        }
    }
}