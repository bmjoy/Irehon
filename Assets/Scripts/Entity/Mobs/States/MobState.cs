public abstract class MobState
{
    public MobState(Mob mob)
    {
        this.mob = mob;
    }

    protected Mob mob;

    public virtual bool CanAgro { get => true; }
    public abstract void Enter();
    public abstract void Exit();

    public abstract MobState Update(float timeInState);

}
