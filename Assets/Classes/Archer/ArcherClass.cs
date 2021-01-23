using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Archer", menuName = "PlayerClasses/Archer")]
public class ArcherClass : ClassData
{
    [SerializeField]
    private GameObject projectile;

    public override void LeftMouseButtonDown(Transform player)
    {
        Vector3 currentRotation = player.rotation.eulerAngles;
        Vector3 projectileSpawnOffset = new Vector3(0.6f, 1f, 0);
        currentRotation.x = CameraController.instance.GetPlayerYAxis();
        Quaternion temp = new Quaternion();
        temp.eulerAngles = currentRotation;
        GameObject flyingProjectile = Instantiate(projectile, player.position, temp);
        flyingProjectile.transform.Translate(projectileSpawnOffset);
    }
    public override void RightMouseButtonDown(Transform player)
    {
        player.GetComponent<PlayerController>().StartAiming();
    }
    public override void RightMouseButtonUp(Transform player)
    {
        player.GetComponent<PlayerController>().StopAiming();
    }
    public override void LeftMouseButtonUp(Transform player)
    {
        
    }
}
