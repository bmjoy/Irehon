using Cinemachine;
using System.Collections;
using UnityEngine;

namespace Irehon
{
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }
        public CinemachineBasicMultiChannelPerlin ShakeHandler;

        private float shakeTimer;
        private float shakeTimerTotal;
        private float currentIntensity;


        private void Awake() => Instance = this;


        private void FixedUpdate()
        {
            if (ShakeHandler == null)
            {
                return;
            }

            if (this.shakeTimer >= 0)
            {
                this.shakeTimer -= Time.fixedDeltaTime;
                this.ShakeHandler.m_AmplitudeGain = Mathf.Lerp(this.currentIntensity, 0, 1 - this.shakeTimer / this.shakeTimerTotal);
            }
            else
            {
                this.ShakeHandler.m_AmplitudeGain = 0;
            }
        }

        public void CreateShake(float power, float time)
        {
            if (ShakeHandler == null)
            {
                Debug.LogError("Shake handler null");
                return;
            }
            shakeTimer = time;
            shakeTimerTotal = time;
            currentIntensity = power;
            ShakeHandler.m_AmplitudeGain = Instance.currentIntensity;
        }
    }
}