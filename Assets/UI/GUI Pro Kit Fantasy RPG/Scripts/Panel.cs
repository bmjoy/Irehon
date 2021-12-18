using UnityEngine;

namespace LayerLab
{
    public class Panel : MonoBehaviour
    {
        [SerializeField] private GameObject[] otherPanels;

        public void OnEnable()
        {
            for (int i = 0; i < this.otherPanels.Length; i++)
            {
                this.otherPanels[i].SetActive(true);
            }
        }

        public void OnDisable()
        {
            for (int i = 0; i < this.otherPanels.Length; i++)
            {
                this.otherPanels[i].SetActive(false);
            }
        }
    }
}