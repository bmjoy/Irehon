using Mirror;

public class Player : Entity
{
    private void Start()
    {
        UIController.instance.SetHealthBarValue(1f * (health / maxHealth));
    }

    [Server]
    public override void TakeDamageOnServer(int damage)
    {
        base.TakeDamageOnServer(damage);
        if (!isClient)
            TakeDamageOnClient(connectionToClient, damage, health);
    }

    [TargetRpc]
    public virtual void TakeDamageOnClient(NetworkConnection con, int damage, int currentHealth)
    {
        health -= currentHealth;
        if (health < 0)
            health = 0;
        if (health != currentHealth)
            health = currentHealth;
        UIController.instance.SetHealthBarValue(1f * health / maxHealth);
    }
}
