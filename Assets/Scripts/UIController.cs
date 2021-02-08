using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    private float defaultTriangleSize = 150;
    private float minimumTriangleSize = 80f;
    [SerializeField]
    private Slider health;
    [SerializeField]
    private CanvasGroup hitMarker;
    [SerializeField]
    private AudioSource hitMarkerSound;
    [SerializeField]
    private RectTransform triangleAimingRectangle;
    [SerializeField]
    private RectTransform defaultAimingRectangle;

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

    public void ShowHitMarker()
    {
        StopAllCoroutines();
        hitMarker.alpha = 1;
        StartCoroutine(DisappearHitMarker());
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
        health.value = value;
    }
}
