using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DischargeAoeArrows : AoeAbilityAreaBase
{
    private const int HIT_EFFECT_POOL_AMMOUNT = 7;

    private DischargeAoeArrows aoe;
    [SerializeField]
    private GameObject hitEffectPrefab;
    [SerializeField]
    private int damageByArrow;
    private List<ParticleSystem> hitEffectPool = new List<ParticleSystem>();
    private new ParticleSystem particleSystem;
    private new AudioSource audio;
    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
    private int currentHitId;

    protected override void Effect(Entity entity)
    {
        entity.DoDamage(damageByArrow);
    }

    protected override void OnAddToArea(Entity entity)
    {
        print("entered " + entity.name);
    }

    protected override void OnRemoveFromArea(Entity entity)
    {
        print("left " + entity.name);
    }


    private void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
        aoe = GetComponent<DischargeAoeArrows>();
        audio = GetComponent<AudioSource>();
        for (int i = 0; i < HIT_EFFECT_POOL_AMMOUNT; i++)
        {
            GameObject hitEffectObj = Instantiate(hitEffectPrefab, transform);
            hitEffectPool.Add(hitEffectObj.GetComponent<ParticleSystem>());
        }
    }

    public void PlayArrowRain()
    {
        float duration = particleSystem.main.duration + 0.1f;
        Transform parent = transform.parent;
        transform.parent = null;
        audio.Play();
        particleSystem.Play();
        currentHitId = 0;
        StartCoroutine(ReturnToPlayer());
        IEnumerator ReturnToPlayer()
        {
            yield return new WaitForSeconds(duration);
            transform.parent = parent;
        }
    }

    private void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("Walkable") || other.CompareTag("Floor"))
            return;
        int numCollisionEvents = particleSystem.GetCollisionEvents(other, collisionEvents);
        print(numCollisionEvents + " name = " + other.name);
        for (int i = 0; i < numCollisionEvents; i++)
        {
            currentHitId++;
            if (currentHitId >= hitEffectPool.Count)
                currentHitId = 0;
            ShowHitEffect(hitEffectPool[currentHitId],
                collisionEvents[i].intersection,
                hitEffectPool[currentHitId].main.duration);
        }
        aoe.TriggerEffectToAllInArea();
    }

    private void ShowHitEffect(ParticleSystem particle, Vector3 pos, float duration)
    {
        particle.transform.position = pos;
        particle.Play();
    }
}
