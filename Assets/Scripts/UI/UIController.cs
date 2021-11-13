    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{

    public static UIController i;


    [SerializeField]
    private Image cursorHint;

    private float defaultTriangleSize = 150;
    private float healthBarUpdateDelay = 0.5f;
    private float reducingAmount = 0.7f;
    private float minimumTriangleSize = 80f;
    [SerializeField]
    private Canvas statusBarCanvas;
    [SerializeField]
    private Canvas AbilityTree;
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
    private RectTransform defaultAimingRectangle;
    [SerializeField]
    private GameObject interactableHint;

    private Coroutine hitMarkerCoroutine;
    private Coroutine healthBarCoroutine;

    private void Awake()
    {
        if (i != null && i != this)
            Destroy(gameObject);
        else
            i = this;
    }

    private void Start()
    {
        CameraController.OnChangeCursorStateEvent.AddListener(ChangeCursorHintStatus);
    }

    public void ShowStatusCanvas() => statusBarCanvas.gameObject.SetActive(true);
    public void HideStatusCanvas() => statusBarCanvas.gameObject.SetActive(false);


    public void EnableDefaultCrosshair()
    {
        defaultAimingRectangle.gameObject.SetActive(true);
        triangleAimingRectangle.gameObject.SetActive(false);
    }

    public void EnableTriangleCrosshair()
    {
        defaultAimingRectangle.gameObject.SetActive(false);
        triangleAimingRectangle.gameObject.SetActive(true);
        ChangeTriangleAimSize(0);
    }

    public void ChangeTriangleAimSize(float newSize)
    {
        float sizeDelta = (defaultTriangleSize - minimumTriangleSize) * newSize;
        float size = defaultTriangleSize - sizeDelta;
        triangleAimingRectangle.sizeDelta = new Vector2(size, size);
    }

    public void ShowHint(string header, string content)
    {
        interactableHint.SetActive(true);
        interactableHint.GetComponent<HintUI>().UpdateHint(header, content);
    }

    public void HideHint()
    {
        interactableHint.SetActive(false);
    }

    public void ShowHitMarker()
    {
        if (hitMarkerCoroutine != null)
            StopCoroutine(hitMarkerCoroutine);
        hitMarker.alpha = 1;
        hitMarkerCoroutine = StartCoroutine(DisappearHitMarker());
        hitMarkerSound.Play();
    }

    private IEnumerator DisappearHitMarker()
    {
        while (hitMarker.alpha > 0)
        {
            hitMarker.alpha -= .1f;
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
        healthBar.value = value;

        float passedTime = 0f;

        if (healthBarCoroutine != null)
            StopCoroutine(healthBarCoroutine);

        if (postHealthBar.value > healthBar.value)
            healthBarCoroutine = StartCoroutine(ChangeFillAmount());
        else
            postHealthBar.value = healthBar.value;

        IEnumerator ChangeFillAmount()
        {
            while (postHealthBar.value > healthBar.value)
            {
                passedTime += Time.deltaTime;
                if (passedTime > healthBarUpdateDelay)
                    postHealthBar.value -= reducingAmount * Time.deltaTime;
                yield return null;
            }
        }
    }

    private void ChangeCursorHintStatus(bool isCursorEnabled)
    {
        float alpha = isCursorEnabled ? 0.9f : 0.3f;

        var color = Color.white;
        color.a = alpha;
        cursorHint.color = color;
    }

    public static void SetDefaultUI()
    {
        i.EnableDefaultCrosshair();
        CameraController.DisableAimCamera();
        CameraController.EnableCursor();
    }
}
