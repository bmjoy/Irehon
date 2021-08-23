public interface IMovementBehaviour
{
    void ProcessMovementInput(InputState input);
    void Jump();

    bool IsCanJump();
}