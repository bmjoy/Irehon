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
        DeathOnClient(connectionToClient);
    }

    protected override void Respawn()
    {
        base.Respawn();
        //controller.ResetControll();
        RespawnOnClient(connectionToClient);
    }

    [Server]
    protected override void SetHealth(int health)
    {
        base.SetHealth(health);
        SetHealthOnClient(connectionToClient, health);
    }

    [Server]
    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
        SetHealthOnClient(connectionToClient, health);
    }

    [TargetRpc]
    public virtual void SetHealthOnClient(NetworkConnection con, int currentHealth)
    {
        health = currentHealth;
        UIController.instance.SetHealthBarValue(1f * health / maxHealth);
    }

    [TargetRpc]
    public virtual void DeathOnClient(NetworkConnection con)
    {
        base.Death();
        controller.BlockControll();
    }

    [TargetRpc]
    public virtual void RespawnOnClient(NetworkConnection con)
    {
        base.Respawn();
        //controller.ResetControll();
    }

    [Server]
    public void DoDamage(Entity target, int damage)
    {
        target.TakeDamage(damage);
        controller.HitConfirmed(connectionToClient);
    }
}
