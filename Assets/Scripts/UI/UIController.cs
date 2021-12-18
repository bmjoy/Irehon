using Irehon;
using Irehon.Entitys;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController i;

    private float defaultTriangleSize = 150;
    private float healthBarUpdateDelay = 0.5f;
    private float reducingAmount = 0.7f;
    private float minimumTriangleSize = 80f;

    [SerializeField]
    private Image cursorHint;
    [SerializeField]
    private Canvas statusBarCanvas;
    [SerializeField]
    private Slider healthBar;
    [SerializeField]
    private Slider postHealthBar;
    [SerializeField]
    private CanvasGroup hitMarker;
    [SerializeField]
    private AudioSource hitMarkerSound;
    [SerializeField]
    private RectTransform triangleAimingRectangle;
    [SerializeField]
    private RectTransform defaultAimPoint;
    [SerializeField]
    private GameObject interactableHint;

    private Image defaultAimPointImage;
    private Coroutine hitMarkerCoroutine;
    private Coroutine healthBarCoroutine;

    private void Awake()
    {
        if (i != null && i != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            i = this;
        }
    }

    private void Start()
    {
        CameraController.OnChangeCursorStateEvent += this.ChangeCursorHintStatus;
        CameraController.i.OnLookingOnEntityEvent += this.UpdateCursorColor;

        this.defaultAimPointImage = this.defaultAimPoint.GetComponent<Image>();
    }

    private void UpdateCursorColor(Entity target, Player player)
    {
        if (target == null || !target.isAlive)
        {
            this.defaultAimPointImage.color = Color.white;
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
                        this.defaultAimPointImage.color = Color.green;
                        break;
                    case FractionBehaviour.Neutral:
                        this.defaultAimPointImage.color = Color.white;
                        break;
                    case FractionBehaviour.Agressive:
                        this.defaultAimPointImage.color = Color.red;
                        break;
                }
            }
            else
            {
                this.defaultAimPointImage.color = Color.white;
            }
        }
    }

    public void ShowStatusCanvas()
    {
        this.statusBarCanvas.gameObject.SetActive(true);
    }

    public void HideStatusCanvas()
    {
        this.statusBarCanvas.gameObject.SetActive(false);
    }

    public void DisableDefaultCrosshair()
    {
        this.defaultAimPoint.gameObject.SetActive(false);
    }

    public void EnableDefaultCrosshair()
    {
        this.defaultAimPoint.gameObject.SetActive(true);
        this.triangleAimingRectangle.gameObject.SetActive(false);
    }

    public void EnableTriangleCrosshair()
    {
        this.DisableDefaultCrosshair();
        this.triangleAimingRectangle.gameObject.SetActive(true);
        this.ChangeTriangleAimSize(0);
    }

    public void ChangeTriangleAimSize(float newSize)
    {
        float sizeDelta = (this.defaultTriangleSize - this.minimumTriangleSize) * newSize;
        float size = this.defaultTriangleSize - sizeDelta;
        this.triangleAimingRectangle.sizeDelta = new Vector2(size, size);
    }

    public void ShowHint(string header, string content)
    {
        this.interactableHint.SetActive(true);
        this.interactableHint.GetComponent<HintUI>().UpdateHint(header, content);
    }

    public void HideHint()
    {
        this.interactableHint.SetActive(false);
    }

    public void ShowHitMarker()
    {
        if (this.hitMarkerCoroutine != null)
        {
            this.StopCoroutine(this.hitMarkerCoroutine);
        }

        this.hitMarker.alpha = 1;
        this.hitMarkerCoroutine = this.StartCoroutine(this.DisappearHitMarker());
        this.hitMarkerSound.Play();
    }

    private IEnumerator DisappearHitMarker()
    {
        while (this.hitMarker.alpha > 0)
        {
            this.hitMarker.alpha -= .1f;
            yield return new WaitForSeconds(.1f);
        }
    }

    public static bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

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

    private void ChangeCursorHintStatus(bool isCursorEnabled)
    {
        float alpha = isCursorEnabled ? 0.9f : 0.3f;

        Color color = Color.white;
        color.a = alpha;
        this.cursorHint.color = color;
    }

    public static void SetDefaultUI()
    {
        i.EnableDefaultCrosshair();
        ContainerWindowManager.i.GetDragger().gameObject.SetActive(false);
        CameraController.DisableAimCamera();
        CameraController.EnableCursor();
    }
}
