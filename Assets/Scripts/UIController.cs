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
    private Image hitMarker;
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
        Color newColor = hitMarker.color;
        newColor.a = 1;
        hitMarker.color = newColor;
        StartCoroutine(DisappearHitMarker());
    }

    private IEnumerator DisappearHitMarker()
    {
        while (hitMarker.color.a > 0)
        {
            Color newColor = hitMarker.color;
            newColor.a -= .01f;
            hitMarker.color = newColor;
            yield return new WaitForSeconds(.01f);
        }
    }

    public void SetHealthBarValue(float value)
    {
        health.value = value;
    }
}
