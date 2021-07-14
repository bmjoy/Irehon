using UnityEngine;

public partial class PlayerController
{
    public struct SendInputState
    {
        public InputState input;
        public int frame;
        public Vector3 position;
        public System.DateTime inputSendedTime;
    };
}
