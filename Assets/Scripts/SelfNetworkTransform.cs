using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SelfNetworkTransform : NetworkTransform
{
    public override void ApplyPositionRotationScale(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        if (isLocalPlayer)
            return;
        base.ApplyPositionRotationScale(position, rotation, scale);
    }
}
