using Mirror;
using System.Collections.Generic;
using UnityEngine;

public struct InputInfo
{
    public List<KeyCode> PressedKeys;
    public PlayerStateType PlayerStateType;
    public Vector2 CameraRotation;
    public Vector3 TargetPoint;
    public Vector3 Position;
    public NetworkIdentity interactionTarget;

    public Vector2 GetMoveVector()
    {
        Vector2 moveVector = Vector2.zero;
        moveVector.x += this.PressedKeys.Contains(KeyCode.D) ? 1 : 0;
        moveVector.x += this.PressedKeys.Contains(KeyCode.A) ? -1 : 0;
        moveVector.y += this.PressedKeys.Contains(KeyCode.W) ? 1 : 0;
        moveVector.y += this.PressedKeys.Contains(KeyCode.S) ? -1 : 0;
        return moveVector;
    }

    public bool IsKeyPressed(KeyCode key)
    {
        return this.PressedKeys.Contains(key);
    }
}