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
            if (c1.ForwardKeyDown != c2.ForwardKeyDown || c1.BackKeyDown != c2.BackKeyDown
                || c1.RightKeyDown != c2.RightKeyDown || c1.LeftKeyDown != c2.LeftKeyDown || 
                c1.JumpKeyDown != c2.JumpKeyDown || c1.SprintKeyDown != c2.SprintKeyDown)
                return true;
            return false;
        }

        public static bool operator ==(InputState c1, InputState c2)
        {
            if (c1 != c2)
                return false;
            return true;
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

        public override int GetHashCode()
        {
            int hashCode = 2013472923;
            hashCode = hashCode * -1521134295 + ForwardKeyDown.GetHashCode();
            hashCode = hashCode * -1521134295 + BackKeyDown.GetHashCode();
            hashCode = hashCode * -1521134295 + RightKeyDown.GetHashCode();
            hashCode = hashCode * -1521134295 + LeftKeyDown.GetHashCode();
            hashCode = hashCode * -1521134295 + SprintKeyDown.GetHashCode();
            hashCode = hashCode * -1521134295 + JumpKeyDown.GetHashCode();
            hashCode = hashCode * -1521134295 + currentRotation.GetHashCode();
            hashCode = hashCode * -1521134295 + frame.GetHashCode();
            return hashCode;
        }
    };
}
