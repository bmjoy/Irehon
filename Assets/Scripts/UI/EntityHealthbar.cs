using Irehon.Camera;
using Irehon.Entitys;
using Mirror;
using System.Collections;
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
        this.entity = this.transform.parent.GetComponent<Entity>();

        if (this.entity == null)
        {
            Destroy(this.gameObject);
            return;
        }

        if (this.entity.GetComponent<NetworkIdentity>().isLocalPlayer && !this.entity.GetComponent<NetworkIdentity>().isServer)
        {
            Destroy(this.gameObject);
            return;
        }

        this.entity.OnDeathEvent += () => this.SetActive(false);
        this.entity.OnRespawnEvent += () => this.SetActive(true);

        this.StartCoroutine(this.WaitCameraControllerIntialize());
        this.nickname.text = this.entity.NickName;
        this.entity.OnHealthChangeEvent += this.ChangeHealthOnBar;
        this.entity.OnPlayerLookingEvent += () => this.EnableForTime(5f);
    }

    private void Update()
    {
        if (this.cameraTransform == null)
        {
            return;
        }

        this.RepositionBar();
        if (Vector3.Distance(this.transform.position, this.cameraTransform.position) < this.showingDistance && this.entity.isAlive)
        {
            this.disablingDelay = 2f;
            this.SetActive(true);
        }
        this.disablingDelay -= Time.deltaTime;
        if (this.disablingDelay < 0)
        {
            this.SetActive(false);
        }
    }

    public void EnableForTime(float time)
    {
        if (this.entity.isAlive)
        {
            this.disablingDelay = time;
            this.SetActive(true);
        }
    }

    public void SetActive(bool isActive)
    {
        this.canvas.enabled = isActive;
        this.nickname.enabled = isActive;
    }

    private IEnumerator WaitCameraControllerIntialize()
    {
        while (CameraController.Instance == null)
        {
            yield return null;
        }

        this.cameraTransform = CameraController.Instance.transform;
        this.canvas.worldCamera = PlayerCamera.Instance.Camera;
    }

    private void ChangeHealthOnBar(int maxHealth, int health)
    {
        if (health == 0)
        {
            return;
        }

        float fill = 1.0f * health / maxHealth;

        this.healthBarFiller.fillAmount = fill;

        float passedTime = 0f;

        this.StopAllCoroutines();

        if (this.isActiveAndEnabled && this.healthBarPostFiller.fillAmount > this.healthBarFiller.fillAmount)
        {
            this.StartCoroutine(ChangeFillAmount());
        }
        else
        {
            this.healthBarPostFiller.fillAmount = this.healthBarFiller.fillAmount;
        }

        IEnumerator ChangeFillAmount()
        {
            while (this.healthBarPostFiller.fillAmount > this.healthBarFiller.fillAmount)
            {
                passedTime += Time.deltaTime;
                if (passedTime > this.updateDelay)
                {
                    this.healthBarPostFiller.fillAmount -= this.reducingPostBarAmmount * Time.deltaTime;
                }

                yield return null;
            }
        }
    }

    private void RepositionBar()
    {
        this.transform.LookAt(this.cameraTransform);
        this.transform.rotation = Quaternion.Euler(this.transform.rotation.eulerAngles.x, this.transform.rotation.eulerAngles.y, 0);
    }
}