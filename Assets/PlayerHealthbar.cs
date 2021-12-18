using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Irehon.UI
{
    public class PlayerHealthbar : MonoBehaviour
    {
        public static PlayerHealthbar Instance { get; private set; }
        [SerializeField]
        private Slider healthBar;
        [SerializeField]
        private Slider postHealthBar;

        private float reducingAmount = 0.7f;
        private float healthBarUpdateDelay = 0.5f;
        private Coroutine healthBarCoroutine;

        private void Awake() => Instance = this;

        public void SetHealthBarValue(float value)
        {
            this.healthBar.value = value;

            float passedTime = 0f;

            if (this.healthBarCoroutine != null)
            {
                this.StopCoroutine(this.healthBarCoroutine);
            }

            if (this.postHealthBar.value > this.healthBar.value)
            {
                this.healthBarCoroutine = this.StartCoroutine(ChangeFillAmount());
            }
            else
            {
                this.postHealthBar.value = this.healthBar.value;
            }

            IEnumerator ChangeFillAmount()
            {
                while (this.postHealthBar.value > this.healthBar.value)
                {
                    passedTime += Time.deltaTime;
                    if (passedTime > this.healthBarUpdateDelay)
                    {
                        this.postHealthBar.value -= this.reducingAmount * Time.deltaTime;
                    }

                    yield return null;
                }
            }
        }
    }
}