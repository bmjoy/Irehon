using UnityEngine;

public static class CharacterControllerExtension
{
    public static void SetRotation(this CharacterController controller, Vector3 rotation)
    {
        controller.enabled = false;
        controller.transform.rotation = Quaternion.Euler(rotation);
        controller.enabled = true;
    }

    public static void SetRotation(this CharacterController controller, Quaternion rotation)
    {
        controller.enabled = false;
        controller.transform.rotation = rotation;
        controller.enabled = true;
    }

    public static void Rotate(this CharacterController controller, Vector3 eulers)
    {
        controller.enabled = false;
        controller.transform.Rotate(eulers);
        controller.enabled = true;
    }

    public static void SetPosition(this CharacterController controller, Vector3 position)
    {
        controller.enabled = false;
        controller.transform.position = position;
        controller.enabled = true;
    }
}