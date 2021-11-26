using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EntityHealthbar : MonoBehaviour
{
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private Image healthBarFiller;
    [SerializeField]
    private Image healthBarPostFiller;
    [SerializeField]
    private TextMeshPro nickname;
    [SerializeField]
    private float reducingPostBarAmmount = 3f;
    [SerializeField]
    private float updateDelay;
    private Transform cameraTransform;
    private Entity entity;
    private void Start()
    {
        entity = transform.parent.GetComponent<Entity>();
        if (entity.GetComponent<NetworkIdentity>().isLocalPlayer && !entity.GetComponent<NetworkIdentity>().isServer)
        {
            Destroy(gameObject);
            return;
        }

        entity.OnDeathEvent.AddListener(() => gameObject.SetActive(false));
        entity.OnRespawnEvent.AddListener(() => gameObject.SetActive(true));

        StartCoroutine(GetInstanceKostil());
        nickname.text = entity.NickName;
        entity.OnHealthChangeEvent.AddListener(ChangeHealthOnBar);
    }

    private IEnumerator GetInstanceKostil()
    {
        while (CameraController.i == null)
            yield return null;
        cameraTransform = CameraController.i.transform;
        canvas.worldCamera = CameraController.i.cameraComponent;
    }

    private void ChangeHealthOnBar(int maxHealth, int health)
    {
        if (health == 0)
            return;

        float fill = 1.0f * health / maxHealth;

        healthBarFiller.fillAmount = fill;

        float passedTime = 0f;

        StopAllCoroutines();

        if (healthBarPostFiller.fillAmount > healthBarFiller.fillAmount)
            StartCoroutine(ChangeFillAmount());
        else
            healthBarPostFiller.fillAmount = healthBarFiller.fillAmount;

        IEnumerator ChangeFillAmount()
        {
            while (healthBarPostFiller.fillAmount > healthBarFiller.fillAmount)
            {
                passedTime += Time.deltaTime;
                if (passedTime > updateDelay)
                    healthBarPostFiller.fillAmount -= reducingPostBarAmmount * Time.deltaTime;
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
        if (cameraTransform == null)
            return;
        transform.LookAt(cameraTransform);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
    }
}