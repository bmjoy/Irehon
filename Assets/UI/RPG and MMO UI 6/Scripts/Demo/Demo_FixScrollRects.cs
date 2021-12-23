using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Demo_FixScrollRects : MonoBehaviour
{

    protected void OnEnable()
    {
        SceneManager.sceneLoaded += this.OnLoaded;
    }

    protected void OnDisable()
    {
        SceneManager.sceneLoaded -= this.OnLoaded;
    }

    private void OnLoaded(Scene scene, LoadSceneMode mode)
    {
        ScrollRect[] rects = Component.FindObjectsOfType<ScrollRect>();

        foreach (ScrollRect rect in rects)
        {
            LayoutRebuilder.MarkLayoutForRebuild(rect.viewport);
        }
    }
}
