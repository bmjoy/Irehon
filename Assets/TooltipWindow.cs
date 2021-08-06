using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 

public class TooltipWindow : MonoBehaviour
{
    public static TooltipWindow i;

    private void Awake()
    {
        if (i != null && i != this)
            Destroy(gameObject);
        else
            i = this;
    }

    public static ShowTooltip()
}
