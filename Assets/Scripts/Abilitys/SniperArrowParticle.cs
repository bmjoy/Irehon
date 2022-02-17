using UnityEngine;

namespace Irehon.Abilitys
{
    public class SniperArrowParticle : MonoBehaviour
    {
        private Vector3 minSize = new Vector3(2f, 2f, 1f);
        private Vector3 maxSize = new Vector3(5f, 5f, 5f);
        [SerializeField]
        private Transform particle;

        public void SetWaveSize(float sizeProcent)
        {
            Vector3 waveSize = this.minSize + (this.maxSize - this.minSize) * sizeProcent;
            this.particle.localScale = waveSize;
        }
    }
}