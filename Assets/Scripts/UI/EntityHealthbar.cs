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
    [SerializeField]
    private float showingDistance = 6f;

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

        entity.OnDeathEvent += () => SetActive(false);
        entity.OnRespawnEvent += () => SetActive(true);

        StartCoroutine(WaitCameraControllerIntialize());
        nickname.text = entity.NickName;
        entity.OnHealthChangeEvent += ChangeHealthOnBar;
        entity.OnPlayerLookingEvent += () => EnableForTime(5f);
    }

    private void Update()
    {
        if (cameraTransform == null)
            return;

        RepositionBar();
        if (Vector3.Distance(transform.position, cameraTransform.position) < showingDistance && entity.isAlive)
        {
            disablingDelay = 2f;
            SetActive(true);
        }
        disablingDelay -= Time.deltaTime;
        if (disablingDelay < 0)
            SetActive(false);
    }

    public void EnableForTime(float time)
    {
        if (entity.isAlive)
        {
            disablingDelay = time;
            SetActive(true);
        }
    }

    public void SetActive(bool isActive)
    {
        canvas.enabled = isActive;
        nickname.enabled = isActive;
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
        transform.LookAt(cameraTransform);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
    }
}