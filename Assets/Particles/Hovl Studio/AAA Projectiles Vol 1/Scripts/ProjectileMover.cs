using UnityEngine;

public class ProjectileMover : MonoBehaviour
{
    public float speed = 15f;
    public float hitOffset = 0f;
    public bool UseFirePointRotation;
    public Vector3 rotationOffset = new Vector3(0, 0, 0);
    public GameObject hit;
    public GameObject flash;
    private Rigidbody rb;
    public GameObject[] Detached;

    private void Start()
    {
        this.rb = this.GetComponent<Rigidbody>();
        if (this.flash != null)
        {
            GameObject flashInstance = Instantiate(this.flash, this.transform.position, Quaternion.identity);
            flashInstance.transform.forward = this.gameObject.transform.forward;
            ParticleSystem flashPs = flashInstance.GetComponent<ParticleSystem>();
            if (flashPs != null)
            {
                Destroy(flashInstance, flashPs.main.duration);
            }
            else
            {
                ParticleSystem flashPsParts = flashInstance.transform.GetChild(0).GetComponent<ParticleSystem>();
                Destroy(flashInstance, flashPsParts.main.duration);
            }
        }
        Destroy(this.gameObject, 5);
    }

    private void FixedUpdate()
    {
        if (this.speed != 0)
        {
            this.rb.velocity = this.transform.forward * this.speed;
            //transform.position += transform.forward * (speed * Time.deltaTime);         
        }
    }

    //https ://docs.unity3d.com/ScriptReference/Rigidbody.OnCollisionEnter.html
    private void OnCollisionEnter(Collision collision)
    {
        //Lock all axes movement and rotation
        this.rb.constraints = RigidbodyConstraints.FreezeAll;
        this.speed = 0;

        ContactPoint contact = collision.contacts[0];
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
        Vector3 pos = contact.point + contact.normal * this.hitOffset;

        if (this.hit != null)
        {
            GameObject hitInstance = Instantiate(this.hit, pos, rot);
            if (this.UseFirePointRotation) { hitInstance.transform.rotation = this.gameObject.transform.rotation * Quaternion.Euler(0, 180f, 0); }
            else if (this.rotationOffset != Vector3.zero) { hitInstance.transform.rotation = Quaternion.Euler(this.rotationOffset); }
            else { hitInstance.transform.LookAt(contact.point + contact.normal); }

            ParticleSystem hitPs = hitInstance.GetComponent<ParticleSystem>();
            if (hitPs != null)
            {
                Destroy(hitInstance, hitPs.main.duration);
            }
            else
            {
                ParticleSystem hitPsParts = hitInstance.transform.GetChild(0).GetComponent<ParticleSystem>();
                Destroy(hitInstance, hitPsParts.main.duration);
            }
        }
        foreach (GameObject detachedPrefab in this.Detached)
        {
            if (detachedPrefab != null)
            {
                detachedPrefab.transform.parent = null;
            }
        }
        Destroy(this.gameObject);
    }
}
