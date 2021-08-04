    using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    private float defaultTriangleSize = 150;
    private float healthBarUpdateDelay = 0.5f;
    private float reducingAmount = 0.7f;
    private float minimumTriangleSize = 80f;
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
        instance = this;
    }

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

    public void ShowInteractableHint()
    {
        interactableHint.SetActive(true);
    }

    public void HideInteractableHint()
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
}
