using UnityEngine;

namespace DuloGames.UI
{
    public class Test_UIItemSlot_Assign : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private UIItemSlot slot;
        [SerializeField] private int assignItem;
#pragma warning restore 0649

        private void Awake()
        {
            if (this.slot == null)
            {
                this.slot = this.GetComponent<UIItemSlot>();
            }
        }

        private void Start()
        {
            if (this.slot == null || UIItemDatabase.Instance == null)
            {
                this.Destruct();
                return;
            }

            this.slot.Assign(UIItemDatabase.Instance.GetByID(this.assignItem));
            this.Destruct();
        }

        private void Destruct()
        {
            DestroyImmediate(this);
        }
    }
}
