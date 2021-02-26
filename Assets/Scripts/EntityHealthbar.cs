using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EntityHealthbar : MonoBehaviour
{
    [SerializeField]
    private Slider healthBarFiller;
    [SerializeField]
    private Slider healthBarPostFiller;
    [SerializeField]
    private Text nickname;
    [SerializeField]
    private float reducingPostBarAmmount = 3f;
    [SerializeField]
    private float updateDelay;
    private Transform cameraTransform;
    private Entity entity;
    private void Start()
    {
        entity = transform.parent.GetComponent<Entity>();
        if (entity.GetComponent<NetworkIdentity>().isLocalPlayer)
        {
            Destroy(gameObject);
            return;
        }
        cameraTransform = CameraController.instance.transform;
        nickname.text = entity.NickName;
        entity.OnHealthChanged.AddListener(ChangeHealthOnBar);
    }

    private void ChangeHealthOnBar(int maxHealth, int health)
    {
        float fill = 1.0f * health / maxHealth;

        healthBarFiller.value = fill;

        float passedTime = 0f;

        StopAllCoroutines();

        if (healthBarPostFiller.value > healthBarFiller.value)
            StartCoroutine(ChangeFillAmount());
        else
            healthBarPostFiller.value = healthBarFiller.value;

        IEnumerator ChangeFillAmount()
        {
            while (healthBarPostFiller.value > healthBarFiller.value)
            {
                passedTime += Time.deltaTime;
                if (passedTime > updateDelay)
                    healthBarPostFiller.value -= reducingPostBarAmmount * Time.deltaTime;
                yield return null;
            }
        }
    }

    private void Update()
    {
        RepositionBar();
    }

    private void RepositionBar()
    {
        transform.LookAt(cameraTransform);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
    }
}