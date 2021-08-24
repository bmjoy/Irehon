using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public struct InputInfo
{
    public List<KeyCode> PressedKeys;
    public Vector2 CameraRotation;
    public Vector3 TargetPoint;
    public Vector3 Position;
}