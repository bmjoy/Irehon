using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LayerLab
{
    public class PanelControl : MonoBehaviour
    {
        private int page = 0;
        private bool isReady = false;
        [SerializeField] private List<GameObject> panels = new List<GameObject>();
        private TextMeshProUGUI textTitle;
        [SerializeField] private Transform panelTransform;
        [SerializeField] private Button buttonPrev;
        [SerializeField] private Button buttonNext;

        private void Start()
        {
            this.textTitle = this.transform.GetComponentInChildren<TextMeshProUGUI>();
            this.buttonPrev.onClick.AddListener(this.Click_Prev);
            this.buttonNext.onClick.AddListener(this.Click_Next);

            foreach (Transform t in this.panelTransform)
            {
                this.panels.Add(t.gameObject);
                t.gameObject.SetActive(false);
            }

            this.panels[this.page].SetActive(true);
            this.isReady = true;

            this.CheckControl();
        }

        private void Update()
        {
            if (this.panels.Count <= 0 || !this.isReady)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                this.Click_Prev();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                this.Click_Next();
            }
        }

        //Click_Prev
        public void Click_Prev()
        {
            if (this.page <= 0 || !this.isReady)
            {
                return;
            }

            this.panels[this.page].SetActive(false);
            this.panels[this.page -= 1].SetActive(true);
            this.textTitle.text = this.panels[this.page].name;
            this.CheckControl();
        }

        //Click_Next
        public void Click_Next()
        {
            if (this.page >= this.panels.Count - 1)
            {
                return;
            }

            this.panels[this.page].SetActive(false);
            this.panels[this.page += 1].SetActive(true);
            this.CheckControl();
        }

        private void SetArrowActive()
        {
            this.buttonPrev.gameObject.SetActive(this.page > 0);
            this.buttonNext.gameObject.SetActive(this.page < this.panels.Count - 1);
        }

        //SetTitle, SetArrow Active
        private void CheckControl()
        {
            this.textTitle.text = this.panels[this.page].name.Replace("_", " ");
            this.SetArrowActive();
        }
    }
}
