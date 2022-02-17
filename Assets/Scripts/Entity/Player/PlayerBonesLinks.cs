using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerBonesLinks : SerializedMonoBehaviour
{
    [SerializeField]
    public Transform Model { get; private set; }
    [SerializeField]
    public Transform Shoulder { get; private set; }
    [SerializeField]
    public Transform ShoulderForwardPoint { get; private set; }
    [SerializeField]
    public Transform RightHand { get; private set; }
    [SerializeField]
    public Transform LeftHand { get; private set; }
    [SerializeField]
    public GameObject ParticlesPool { get; private set; }
}
