using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerCombatMath
{
    public static Vector3 GetAngleVector3(Vector3 original, float angle, float distance)
    {
        float x = original.x + distance * Mathf.Sin(angle * Mathf.Deg2Rad);
        float z = original.z + distance * Mathf.Cos(angle * Mathf.Deg2Rad);
        return new Vector3(x, original.y, z);
    }

    public static bool IsTargetOnFOV(Vector3 source, Vector3 target, float fieldOfView, float yAngle)
    {
        float distance = Vector3.Distance(target, source);
        Vector3 rightVector = GetAngleVector3(source, yAngle + fieldOfView / 2, distance);
        Vector3 leftVector = GetAngleVector3(source, yAngle - fieldOfView / 2, distance);

        float angle = Vector3.Angle(rightVector - target, leftVector - target);

        return angle + 5 > fieldOfView;
    }

    public static void BlockDamageProcess(ref DamageMessage damageMessage)
    {
        Vector3 blockerPosition = damageMessage.source.transform.position;
        Vector3 attackerPosition = damageMessage.target.transform.position;
        float blockerAngle = damageMessage.source.transform.rotation.eulerAngles.y;

        bool isBlocking = IsTargetOnFOV(blockerPosition, attackerPosition, 120, blockerAngle);
        if (isBlocking)
        {
            float damage = damageMessage.damage;
            damage *= .1f;

            damageMessage.damage = Mathf.RoundToInt(damage);
        }
    }
}
