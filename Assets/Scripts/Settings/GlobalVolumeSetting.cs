using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class GlobalVolumeSetting : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Slider>().value = AudioListener.volume;
    }
    public void SetGlobalVolume(float newValue)
    {
        AudioListener.volume = newValue;
    }
}
