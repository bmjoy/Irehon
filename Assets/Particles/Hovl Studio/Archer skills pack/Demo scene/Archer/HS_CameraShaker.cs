using System.Collections;
using UnityEngine;

public class HS_CameraShaker : MonoBehaviour
{
    public Transform cameraObject;
    public float amplitude;
    public float frequency;
    public float duration;
    public float timeRemaining;
    private Vector3 noiseOffset;
    private Vector3 noise;
    private AnimationCurve smoothCurve = new AnimationCurve(new Keyframe(0.0f, 0.0f, Mathf.Deg2Rad * 0.0f, Mathf.Deg2Rad * 720.0f), new Keyframe(0.2f, 1.0f), new Keyframe(1.0f, 0.0f));

    private void Start()
    {
        float rand = 32.0f;
        this.noiseOffset.x = Random.Range(0.0f, rand);
        this.noiseOffset.y = Random.Range(0.0f, rand);
        this.noiseOffset.z = Random.Range(0.0f, rand);
    }

    public IEnumerator Shake(float amp, float freq, float dur, float wait)
    {
        yield return new WaitForSeconds(wait);
        float rand = 32.0f;
        this.noiseOffset.x = Random.Range(0.0f, rand);
        this.noiseOffset.y = Random.Range(0.0f, rand);
        this.noiseOffset.z = Random.Range(0.0f, rand);
        this.amplitude = amp;
        this.frequency = freq;
        this.duration = dur;
        this.timeRemaining += dur;
        if (this.timeRemaining > dur)
        {
            this.timeRemaining = dur;
        }
    }

    private void Update()
    {
        if (this.timeRemaining <= 0)
        {
            return;
        }

        float deltaTime = Time.deltaTime;
        this.timeRemaining -= deltaTime;
        float noiseOffsetDelta = deltaTime * this.frequency;

        this.noiseOffset.x += noiseOffsetDelta;
        this.noiseOffset.y += noiseOffsetDelta;
        this.noiseOffset.z += noiseOffsetDelta;

        this.noise.x = Mathf.PerlinNoise(this.noiseOffset.x, 0.0f);
        this.noise.y = Mathf.PerlinNoise(this.noiseOffset.y, 1.0f);
        this.noise.z = Mathf.PerlinNoise(this.noiseOffset.z, 2.0f);

        this.noise -= Vector3.one * 0.5f;
        this.noise *= this.amplitude;

        float agePercent = 1.0f - (this.timeRemaining / this.duration);
        this.noise *= this.smoothCurve.Evaluate(agePercent);
    }

    private void LateUpdate()
    {
        if (this.timeRemaining <= 0)
        {
            return;
        }

        Vector3 positionOffset = Vector3.zero;
        Vector3 rotationOffset = Vector3.zero;
        positionOffset += this.noise;
        rotationOffset += this.noise;
        this.cameraObject.transform.localPosition = positionOffset;
        this.cameraObject.transform.localEulerAngles = rotationOffset;
    }
}
