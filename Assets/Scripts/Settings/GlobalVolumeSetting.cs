using UnityEngine;
using UnityEngine.UI;

public class GlobalVolumeSetting : MonoBehaviour
{
    private void Awake()
    {
        this.GetComponent<Slider>().value = AudioListener.volume;
    }
    public void SetGlobalVolume(float newValue)
    {
        AudioListener.volume = newValue;
    }
}
