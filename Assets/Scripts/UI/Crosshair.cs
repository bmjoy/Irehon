using Irehon.Camera;
using Irehon.Entitys;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Irehon
{
    public class Crosshair : MonoBehaviour
    {
        public static Crosshair Instance { get; private set; }

        [SerializeField]
        private RectTransform triangleCrosshairTransform;
        [SerializeField]
        private RectTransform defaultCrosshairTransform;

        private RectTransform currentCrosshair;

        private float defaultTriangleSize = 150;
        private float minimumTriangleSize = 80f;


        private void Awake() => Instance = this;

        private void Start()
        {
            CameraController.Instance.OnLookingOnEntityEvent += this.SetEntityCrosshairColor;
        }

        private void SetEntityCrosshairColor(Entity target, Player player)
        {
            if (target == null || !target.isAlive)
            {
                Instance.SetCrosshairColor(Color.white);
            }
            else
            {
                Fraction entityFraction = target.fraction;
                if (player.FractionBehaviourData.Behaviours.ContainsKey(entityFraction))
                {
                    FractionBehaviour behaviour = player.FractionBehaviourData.Behaviours[entityFraction];
                    switch (behaviour)
                    {
                        case FractionBehaviour.Friendly:
                            SetCrosshairColor(Color.green);
                            break;
                        case FractionBehaviour.Neutral:
                            SetCrosshairColor(Color.white);
                            break;
                        case FractionBehaviour.Agressive:
                            SetCrosshairColor(Color.red);
                            break;
                    }
                }
                else
                {
                    SetCrosshairColor(Color.white);
                }
            }
        }


        public void DisableCrosshair()
        {
            this.currentCrosshair.gameObject.SetActive(false);
        }

        public void EnableDefaultCrosshair()
        {
            this.DisableCrosshair();
            this.defaultCrosshairTransform.gameObject.SetActive(true);
            this.currentCrosshair = this.defaultCrosshairTransform;
        }

        public void EnableTriangleCrosshair()
        {
            this.DisableCrosshair();
            this.triangleCrosshairTransform.gameObject.SetActive(true);
            this.currentCrosshair = this.triangleCrosshairTransform;
            this.ChangeTriangleAimSize(0);
        }

        public void ChangeTriangleAimSize(float newSize)
        {
            float sizeDelta = (this.defaultTriangleSize - this.minimumTriangleSize) * newSize;
            float size = this.defaultTriangleSize - sizeDelta;
            this.triangleCrosshairTransform.sizeDelta = new Vector2(size, size);
        }

        public void SetCrosshairColor(Color color)
        {
            foreach (Image image in currentCrosshair.GetComponentsInChildren<Image>())
                image.color = color;
        }
    }
}