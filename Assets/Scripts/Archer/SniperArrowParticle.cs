using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperArrowParticle : MonoBehaviour
{
    private Vector3 minSize = new Vector3(4f, 4f, 4f);
    private Vector3 maxSize = new Vector3(12f, 12f, 5f);
    [SerializeField]
    private Transform particle;

    public void SetWaveSize(float sizeProcent)
    {
        Vector3 waveSize = minSize + (maxSize - minSize) * sizeProcent;
        particle.localScale = waveSize;
    }
}
