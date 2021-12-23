using System.Collections;
using UnityEngine;

public class FrontAttack : MonoBehaviour
{
    public Transform pivot;
    public Vector3 startRotation;
    public float speed = 15f;
    public float drug = 1f;
    public GameObject craterPrefab;
    public ParticleSystem ps;
    public bool playPS = false;
    public float spawnRate = 1f;
    public float spawnDuration = 1f;
    public float positionOffset = 0f;
    public bool changeScale = false;
    private float randomTimer = 0f;
    private float attackingTimer = 0f;
    private float startSpeed = 0f;
    private Vector3 stepPosition;

    [Space]
    [Header("Effect with Mesh animation")]
    public bool effectWithAnimation = false;
    public Animator[] anim;
    public float delay = 0f;
    public bool playMeshEffect;

    private void Update()
    {
        if (this.playMeshEffect == true)
        {
            this.StartCoroutine(this.MeshEffect());
            this.playMeshEffect = false;
        }
    }

    public void PrepeareAttack(Vector3 targetPoint)
    {
        if (this.effectWithAnimation)
        {
            this.StartCoroutine(this.MeshEffect());
        }
        else
        {
            if (this.playPS)
            {
                this.ps.Play();
            }

            this.startSpeed = this.speed;
            this.transform.parent = null;
            this.transform.position = this.pivot.position;
            Vector3 lookPos = targetPoint - this.transform.position;
            lookPos.y = 0;
            if (!this.playPS)
            {
                this.transform.rotation = Quaternion.LookRotation(lookPos);
            }
            else
            {
                this.transform.rotation = Quaternion.LookRotation(lookPos) * Quaternion.Euler(this.startRotation);
            }
            this.stepPosition = this.pivot.position;
            this.randomTimer = 0;
            this.StartCoroutine(this.StartMove());
        }
    }

    public IEnumerator MeshEffect()
    {
        if (this.playPS)
        {
            this.ps.Play();
        }

        yield return new WaitForSeconds(this.delay);
        foreach (Animator animS in this.anim)
        {
            animS.SetTrigger("Attack");
        }
        yield break;
    }

    public IEnumerator StartMove()
    {

        this.attackingTimer += Time.deltaTime;
        while (true)
        {
            this.randomTimer += Time.deltaTime;
            this.startSpeed = this.startSpeed * this.drug;
            this.transform.position += this.transform.forward * (this.startSpeed * Time.deltaTime);

            Vector3 heading = this.transform.position - this.stepPosition;
            float distance = heading.magnitude;

            if (distance > this.spawnRate)
            {
                if (this.craterPrefab != null)
                {
                    Vector3 randomPosition = new Vector3(Random.Range(-this.positionOffset, this.positionOffset), 0, Random.Range(-this.positionOffset, this.positionOffset));
                    Vector3 pos = this.transform.position + (randomPosition * this.randomTimer * 2);

                    //to create effects on terrain
                    if (Terrain.activeTerrain != null)
                    {
                        pos.y = Terrain.activeTerrain.SampleHeight(this.transform.position);
                    }

                    GameObject craterInstance = Instantiate(this.craterPrefab, pos, Quaternion.identity);
                    if (this.changeScale == true) { craterInstance.transform.localScale += new Vector3(this.randomTimer * 2, this.randomTimer * 2, this.randomTimer * 2); }
                    ParticleSystem craterPs = craterInstance.GetComponent<ParticleSystem>();
                    if (craterPs != null)
                    {
                        Destroy(craterInstance, craterPs.main.duration);
                    }
                    else
                    {
                        ParticleSystem flashPsParts = craterInstance.transform.GetChild(0).GetComponent<ParticleSystem>();
                        Destroy(craterInstance, flashPsParts.main.duration);
                    }
                }
                //distance = 0;
                this.stepPosition = this.transform.position;
            }
            if (this.randomTimer > this.spawnDuration)
            {
                this.transform.parent = this.pivot;
                this.transform.position = this.pivot.position;
                this.transform.rotation = Quaternion.Euler(this.startRotation);
                yield break;
            }
            yield return null;
        }
    }
}
