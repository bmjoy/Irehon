using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockTest : MonoBehaviour
{
    [SerializeField]
    private GameObject target;

    public float FOV;

    private void Update()
    {
        print(IsTargetOnFOV(transform.position, target.transform.position, FOV, transform.rotation.eulerAngles.y));
    }

    private void OnDrawGizmos()
    {
        float distance = 40f;
        Vector3 rightVector = GetAngleVector3(transform.position, transform.rotation.eulerAngles.y + FOV / 2, distance);
        Vector3 leftVector = GetAngleVector3(transform.position, transform.rotation.eulerAngles.y - FOV / 2, distance);

        Debug.DrawLine(transform.position, leftVector);
        Debug.DrawLine(transform.position, rightVector);
    }

    Vector3 GetAngleVector3(Vector3 original, float angle, float distance)
    {
        float x = original.x + distance * Mathf.Sin(angle * Mathf.Deg2Rad);
        float z = original.z + distance * Mathf.Cos(angle * Mathf.Deg2Rad);
        return new Vector3(x, original.y, z);
    }

    bool IsTargetOnFOV(Vector3 source, Vector3 target, float fieldOfView, float yAngle)
    {
        float distance = Vector3.Distance(target, source);
        Vector3 rightVector = GetAngleVector3(source, yAngle + fieldOfView / 2, distance);
        Vector3 leftVector = GetAngleVector3(source, yAngle - fieldOfView / 2, distance);

        float angle = Vector3.Angle(rightVector - target, leftVector - target);

        return angle + 5 > fieldOfView;
    }
}
