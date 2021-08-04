public interface IMovementBehaviour
{
    void ProcessMovementInput(PlayerController.InputState input);
    void Jump();

    bool IsCanJump();
}