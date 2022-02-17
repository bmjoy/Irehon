using UnityEngine;

public class BitMaskAttribute : PropertyAttribute
{
    public System.Type propType;

    public BitMaskAttribute(System.Type aType)
    {
        this.propType = aType;
    }
}
