public class PeacefulMob : Mob
{
    protected override void Start()
    {
        base.Start();
        if (this.isServer)
        {
            GotDamage += (dmg =>
            {
                if (this.isAlive)
                {
                    this.stateMachine.SetNewState(new MobFearState(this));
                }
            });
        }
    }
}
