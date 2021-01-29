using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    private float defaultTriangleSize = 150;
    private float minimumTriangleSize = 80f;
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
}
