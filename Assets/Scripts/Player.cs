using Mirror;

public class Player : Entity
{
    PlayerController controller;

    protected override void Awake()
    {
        base.Awake();
        controller = GetComponent<PlayerController>();
    }

    protected override void SetDefaultState()
    {
        base.SetDefaultState();
        controller.AllowControll();
    }

    protected override void Death()
    {
        base.Death();
        controller.BlockControll();
        if (isServer && !isLocalPlayer)
            DeathOnClient(connectionToClient);
    }

    protected override void Respawn()
    {
        base.Respawn();
        //controller.ResetControll();
        if (isServer && !isLocalPlayer)
            RespawnOnClient(connectionToClient);
    }

    [Server]
    protected override void SetHealth(int health)
    {
        base.SetHealth(health);
        SetHealthOnClient(connectionToClient, health);
    }

    [Server]
    public override void DoDamage(int damage)
    {
        base.DoDamage(damage);
        SetHealthOnClient(connectionToClient, health);
    }

    [TargetRpc]
    protected virtual void SetHealthOnClient(NetworkConnection con, int currentHealth)
    {
        health = currentHealth;
        UIController.instance.SetHealthBarValue(1f * health / maxHealth);
    }

    [TargetRpc]
    protected virtual void DeathOnClient(NetworkConnection con)
    {
        base.Death();
        controller.BlockControll();
    }

    [TargetRpc]
    protected virtual void RespawnOnClient(NetworkConnection con)
    {
        base.Respawn();
        //controller.ResetControll();
    }

    [Server]
    public void DoDamage(Entity target, int damage)
    {
        target.DoDamage(damage);
        controller.HitConfirmed(connectionToClient);
    }
}
