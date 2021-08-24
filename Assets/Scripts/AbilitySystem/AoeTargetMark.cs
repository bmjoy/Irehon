using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AoeTargetMark : NetworkBehaviour
{
    private Vector3 targetOffset = new Vector3(0, 0.2f, 0);
    private ParticleSystem particle;
    private Transform parent;
    [SerializeField]
    private Transform targetTransform;
    private new bool enabled;
    private float currentMaxRange;

    private void Start()
    {
        enabled = false;
        //if (isLocalPlayer)
            //CameraController.i.OnChangingTarget.AddListener(MoveTargetCircle);
        parent = targetTransform.parent;
        particle = targetTransform.GetComponent<ParticleSystem>();
    }

    public Vector3 GetMarkPositionOffset()
    {
        return targetOffset;
    }

    public bool IsTargetable() => targetTransform.gameObject.activeSelf;

    public void EnableTarget(float size, float range)
    {
        targetTransform.parent = null;
        targetTransform.localScale = new Vector3(size, size, size);
        currentMaxRange = range;
        enabled = true;
        MoveTargetCircle(CameraController.i.GetLookingTargetPosition()); 
    }

    public void DisableTarget()
    {
        enabled = false;
        particle.Stop();
        targetTransform.parent = parent;
        targetTransform.localPosition = Vector3.zero;
        targetTransform.gameObject.SetActive(false);
    }

    private void MoveTargetCircle(Vector3 newTarget)
    {
        if (!enabled)
            return;
        if (CameraController.i.IsTargetOnFloor())
        {
            targetTransform.gameObject.SetActive(true);
            if (!particle.isPlaying)
                particle.Play();
            targetTransform.position = newTarget + targetOffset;
        }
        else
        {
            targetTransform.gameObject.SetActive(false);
        }
    }
}
