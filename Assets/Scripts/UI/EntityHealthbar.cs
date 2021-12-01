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

    private float disablingDelay;
    private Transform cameraTransform;
    private Entity entity;
    private void Start()
    {
        entity = transform.parent.GetComponent<Entity>();

        if (entity == null)
        {
            Destroy(gameObject);
            return;
        }

        if (entity.GetComponent<NetworkIdentity>().isLocalPlayer && !entity.GetComponent<NetworkIdentity>().isServer)
        {
            Destroy(gameObject);
            return;
        }

        entity.OnDeathEvent.AddListener(() => gameObject.SetActive(false));
        entity.OnRespawnEvent.AddListener(() => gameObject.SetActive(true));

        StartCoroutine(WaitCameraControllerIntialize());
        nickname.text = entity.NickName;
        entity.OnHealthChangeEvent.AddListener(ChangeHealthOnBar);
        entity.OnPlayerLookingEvent.AddListener(() =>
        {
            if (entity.isAlive)
            {
                disablingDelay = 5f;
                gameObject.SetActive(true);
            }
        });
    }

    private void Update()
    {
        RepositionBar();
        disablingDelay -= Time.deltaTime;
        if (disablingDelay < 0)
            gameObject.SetActive(false);
    }

    private IEnumerator WaitCameraControllerIntialize()
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

        if (isActiveAndEnabled && healthBarPostFiller.fillAmount > healthBarFiller.fillAmount)
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

    private void RepositionBar()
    {
        if (cameraTransform == null)
            return;
        transform.LookAt(cameraTransform);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
    }
}