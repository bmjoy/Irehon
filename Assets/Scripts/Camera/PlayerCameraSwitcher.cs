using Cinemachine;
using System.Collections;
using UnityEngine;

namespace Irehon.Camera
{
    public class PlayerCameraSwitcher : MonoBehaviour
    {
        public static PlayerCameraSwitcher Instance { get; private set; }

        [SerializeField]
        private CinemachineVirtualCamera aimCamera;
        [SerializeField]
        private CinemachineVirtualCamera mainCamera;


        private void Awake() => Instance = this;

        private void Start()
        {
            CameraShake.Instance.ShakeHandler = this.mainCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }

        public void EnableAimCamera()
        {
            mainCamera.gameObject.SetActive(false);
            Instance.aimCamera.gameObject.SetActive(true);

            float currentAmplitude = CameraShake.Instance.ShakeHandler.m_AmplitudeGain;
            CameraShake.Instance.ShakeHandler = Instance.aimCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            CameraShake.Instance.ShakeHandler.m_AmplitudeGain = currentAmplitude;
        }

        public void DisableAimCamera()
        {
            Instance.mainCamera.gameObject.SetActive(true);
            Instance.aimCamera.gameObject.SetActive(false);

            float currentAmplitude = CameraShake.Instance.ShakeHandler.m_AmplitudeGain;
            CameraShake.Instance.ShakeHandler = Instance.mainCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            CameraShake.Instance.ShakeHandler.m_AmplitudeGain = currentAmplitude;
        }
    }
}