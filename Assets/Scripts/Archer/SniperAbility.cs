using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperAbility : AbilityBase
{
    [SerializeField]
    private GameObject arrow;

    private Quiver quiver;
    private Player player;


    protected override void Start()
    {
        base.Start();
        player = GetComponent<Player>();
        quiver = new Quiver(player, 5, arrow);
    }

    protected override void Ability(Vector3 target)
    {
        
    }

    public override void TriggerKeyUp(Vector3 target)
    {
        
    }

    public override void Interrupt()
    {

    }
}
