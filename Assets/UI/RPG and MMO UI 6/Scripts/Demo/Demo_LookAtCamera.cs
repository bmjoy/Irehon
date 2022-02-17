using UnityEngine;

namespace DuloGames.UI
{
    public class Demo_LookAtCamera : MonoBehaviour
    {
        [SerializeField] private Camera m_Camera;

        protected void Awake()
        {
            if (this.m_Camera == null)
            {
                this.m_Camera = Camera.main;
            }
        }

        private void Update()
        {
            if (this.m_Camera)
            {
                this.transform.rotation = Quaternion.LookRotation(this.m_Camera.transform.forward);
            }
        }
    }
}
