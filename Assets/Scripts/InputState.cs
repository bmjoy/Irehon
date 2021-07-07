using UnityEngine;

public partial class PlayerController
{
    public struct InputState
    {
        public bool ForwardKeyDown;
        public bool BackKeyDown;
        public bool RightKeyDown;
        public bool LeftKeyDown;
        public bool SprintKeyDown;
        public bool JumpKeyDown;

        public float currentRotation;

        public int frame;

        public static bool operator !=(InputState c1, InputState c2)
        {
            return !(c1.Equals(c2));
                
        }

        public static bool operator ==(InputState c1, InputState c2)
        {
            return !(c1.Equals(c2));
        }

        public Vector2 GetMoveVector()
        {
            Vector2 moveVector = Vector2.zero;
            moveVector.x += RightKeyDown ? 1 : 0;
            moveVector.x += LeftKeyDown ? -1 : 0;
            moveVector.y += ForwardKeyDown ? 1 : 0;
            moveVector.y += BackKeyDown ? -1 : 0;
            return moveVector;
        }

        public override bool Equals(object obj)
        {
            return obj is InputState state &&
                   ForwardKeyDown == state.ForwardKeyDown &&
                   BackKeyDown == state.BackKeyDown &&
                   RightKeyDown == state.RightKeyDown &&
                   LeftKeyDown == state.LeftKeyDown &&
                   SprintKeyDown == state.SprintKeyDown &&
                   JumpKeyDown == state.JumpKeyDown &&
                   currentRotation == state.currentRotation &&
                   frame == state.frame;
        }
    };
}
