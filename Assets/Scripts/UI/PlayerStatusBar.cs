using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Irehon.UI
{
    public class PlayerStatusBar : MonoBehaviour
    {
        [SerializeField]
        private Slider bar;
        [SerializeField]
        private Slider postBar;

        private float reducingAmount = 0.7f;
        private float healthBarUpdateDelay = 0.5f;
        private Coroutine barCoroutine;

        public void SetBarValue(float value)
        {
            this.bar.value = value;

            float passedTime = 0f;

            if (this.barCoroutine != null)
            {
                this.StopCoroutine(this.barCoroutine);
            }

            if (this.postBar.value > this.bar.value)
            {
                this.barCoroutine = this.StartCoroutine(ChangeFillAmount());
            }
            else
            {
                this.postBar.value = this.bar.value;
            }

            IEnumerator ChangeFillAmount()
            {
                while (this.postBar.value > this.bar.value)
                {
                    passedTime += Time.deltaTime;
                    if (passedTime > this.healthBarUpdateDelay)
                    {
                        this.postBar.value -= this.reducingAmount * Time.deltaTime;
                    }

                    yield return null;
                }
            }
        }
    }
}