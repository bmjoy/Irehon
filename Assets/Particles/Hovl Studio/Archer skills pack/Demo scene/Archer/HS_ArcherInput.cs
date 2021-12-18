using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//This script requires you to have setup your animator with 3 parameters, "InputMagnitude", "InputX", "InputZ"
//With a blend tree to control the inputmagnitude and allow blending between animations.
//Also you need to shoose Firepoint, targets > 1, Aim image from canvas and 2 target markers and camera.
[RequireComponent(typeof(CharacterController))]
public class HS_ArcherInput : MonoBehaviour
{
    public float velocity = 9;
    [Space]

    public float InputX;
    public float InputZ;
    public Vector3 desiredMoveDirection;
    public bool blockRotationPlayer;
    public float desiredRotationSpeed = 0.1f;
    public Animator anim;
    public float Speed;
    public float allowPlayerRotation = 0.1f;
    public Camera cam;
    public CharacterController controller;
    public bool isGrounded;
    private float secondLayerWeight = 0;

    [Space]
    [Header("Animation Smoothing")]
    [Range(0, 1f)]
    public float HorizontalAnimSmoothTime = 0.2f;
    [Range(0, 1f)]
    public float VerticalAnimTime = 0.2f;
    [Range(0, 1f)]
    public float StartAnimTime = 0.3f;
    [Range(0, 1f)]
    public float StopAnimTime = 0.15f;

    private float verticalVel;
    private Vector3 moveVector;
    public bool aimMoving = false;
    private float aimTimer = 0;
    public bool canMove;

    [Space]
    [Header("Effects")]
    public GameObject TargetMarker;
    public GameObject TargetMarker2;
    public GameObject[] Prefabs;
    public GameObject[] PrefabsCast;
    public float[] castingTime; //If 0 - can loop, if > 0 - one shot time
    private bool casting = false;
    public LayerMask collidingLayer = ~0; //Target marker can only collide with scene layer
    private Transform parentObject;

    [Space]
    [Header("Canvas")]
    public Image aim;
    public Vector2 uiOffset;
    public List<Transform> screenTargets = new List<Transform>();
    private Transform target;
    private bool activeTarger = false;
    public Transform FirePoint;
    public float fireRate = 0.1f;
    private float fireCountdown = 0f;
    private bool rotateState = false;

    private AudioSource soundComponent; //Play audio from Prefabs
    private AudioClip clip;
    private AudioSource soundComponentCast; //Play audio from PrefabsCast

    [Space]
    [Header("Camera Shaker script")]
    public HS_CameraShaker cameraShaker;

    private void Start()
    {
        this.anim = this.GetComponent<Animator>();
        this.cam = Camera.main;
        this.controller = this.GetComponent<CharacterController>();
        this.target = this.screenTargets[this.targetIndex()];
    }

    private void Update()
    {
        this.UserInterface();
        //Disable moving and skills if alrerady using one
        if (this.aimTimer > 0)
        {
            this.aimTimer -= Time.deltaTime;
            this.aimMoving = true;
        }
        else
        {
            if (this.aimMoving)
            {
                this.InputMagnitude();
            }

            this.aimMoving = false;
        }

        //Need second layer in the Animator
        if (this.anim.layerCount > 1) { this.anim.SetLayerWeight(1, this.secondLayerWeight); }

        if (!this.canMove)
        {
            return;
        }

        this.target = this.screenTargets[this.targetIndex()];

        if (Input.GetMouseButton(0) && this.aim.enabled == true)
        {
            this.aimTimer = 2;
            if (this.activeTarger)
            {
                if (this.fireCountdown <= 0f)
                {
                    if (this.rotateState == false)
                    {
                        this.StartCoroutine(this.RotateToTarget(this.fireRate, this.target.position));
                        //enable turn animation if the turn deviation to the target is more than 20 degrees
                        Vector3 lookPos = this.target.position - this.transform.position;
                        lookPos.y = 0;
                        Quaternion rotation = Quaternion.LookRotation(lookPos);
                        float angle = Quaternion.Angle(this.transform.rotation, rotation);
                        if (angle > 20)
                        {
                            //turn animation
                            this.anim.SetFloat("InputX", 0.3f);
                        }
                    }

                    this.anim.SetTrigger("MaskAttack1");
                    this.secondLayerWeight = Mathf.Lerp(this.secondLayerWeight, 0.5f, Time.deltaTime * 60);
                    this.PrefabsCast[8].GetComponent<ParticleSystem>().Play();
                    GameObject projectile = Instantiate(this.Prefabs[5], this.FirePoint.position, this.FirePoint.rotation);
                    projectile.GetComponent<TargetProjectile>().UpdateTarget(this.target, this.uiOffset);
                    this.StartCoroutine(this.cameraShaker.Shake(0.1f, 2, 0.2f, 0));
                    this.fireCountdown = 0;
                    this.fireCountdown += this.fireRate;
                }
            }
        }
        else if (this.aimTimer < 1)
        {
            this.secondLayerWeight = Mathf.Lerp(this.secondLayerWeight, 0, Time.deltaTime * 2);
        }
        this.fireCountdown -= Time.deltaTime;

        if (Input.GetMouseButtonDown(1) && this.aim.enabled == true && this.activeTarger)
        {
            if (this.rotateState == false)
            {
                this.StartCoroutine(this.RotateToTarget(this.fireRate, this.target.position));
            }
            this.PrefabsCast[7].GetComponent<ParticleSystem>().Play();
            this.StartCoroutine(this.Attack(4));
            if (this.PrefabsCast[7].GetComponent<AudioSource>())
            {
                this.soundComponentCast = this.PrefabsCast[7].GetComponent<AudioSource>();
                this.clip = this.soundComponentCast.clip;
                this.soundComponentCast.PlayOneShot(this.clip);
            }
        }

        if (Input.GetKeyDown("1") && this.aim.enabled == true && this.activeTarger)
        {
            if (this.rotateState == false)
            {
                this.StartCoroutine(this.RotateToTarget(this.fireRate, this.target.position));
            }
            for (int i = 0; i < 3; i++)
            {
                this.PrefabsCast[i].GetComponent<ParticleSystem>().Play();
            }
            if (this.PrefabsCast[0].GetComponent<AudioSource>())
            {
                this.soundComponentCast = this.PrefabsCast[0].GetComponent<AudioSource>();
                this.clip = this.soundComponentCast.clip;
                this.soundComponentCast.PlayOneShot(this.clip);
            }
            this.StartCoroutine(this.Attack(0));
        }

        if (Input.GetMouseButtonDown(1) && this.casting == true)
        {
            this.casting = false;
        }

        if (Input.GetKeyDown("2"))
        {
            this.StartCoroutine(this.Attack(1));
            this.PrefabsCast[3].GetComponent<ParticleSystem>().Play();
            if (this.PrefabsCast[3].GetComponent<AudioSource>())
            {
                this.soundComponentCast = this.PrefabsCast[3].GetComponent<AudioSource>();
                this.clip = this.soundComponentCast.clip;
                this.soundComponentCast.PlayOneShot(this.clip);
            }
        }

        if (Input.GetKeyDown("3"))
        {
            this.StartCoroutine(this.FrontAttack(2));
        }

        if (Input.GetKeyDown("4"))
        {
            this.StartCoroutine(this.PreCast(3));
        }

        if (Input.GetKeyDown("z") && this.aim.enabled == true)
        {
            this.aimTimer = 2;
            if (this.activeTarger)
            {
                if (this.fireCountdown <= 0f)
                {
                    if (this.rotateState == false)
                    {
                        this.StartCoroutine(this.RotateToTarget(this.fireRate, this.target.position));
                        //enable turn animation if the turn deviation to the target is more than 20 degrees
                        Vector3 lookPos = this.target.position - this.transform.position;
                        lookPos.y = 0;
                        Quaternion rotation = Quaternion.LookRotation(lookPos);
                        float angle = Quaternion.Angle(this.transform.rotation, rotation);
                        if (angle > 20)
                        {
                            //turn animation
                            this.anim.SetFloat("InputX", 0.3f);
                        }
                    }
                    this.StartCoroutine(this.cameraShaker.Shake(0.4f, 3, 0.3f, 0.9f));
                    this.fireCountdown = 0;
                    this.fireCountdown += this.fireRate;
                }
                this.PrefabsCast[9].GetComponent<ParticleSystem>().Play();
                this.PrefabsCast[10].GetComponent<ParticleSystem>().Play();
                if (this.PrefabsCast[10].GetComponent<AudioSource>())
                {
                    this.soundComponentCast = this.PrefabsCast[10].GetComponent<AudioSource>();
                    this.clip = this.soundComponentCast.clip;
                    this.soundComponentCast.PlayOneShot(this.clip);
                }
                this.StartCoroutine(this.Attack(6));
            }
        }

        if (Input.GetKeyDown("x") && this.aim.enabled == true)
        {
            this.StartCoroutine(this.PreCast(7));
        }

        if (Input.GetKeyDown("c") && this.casting == false)
        {
            this.StartCoroutine(this.FrontAttack(8));
        }

        if (Input.GetKeyDown("v") && this.casting == false)
        {
            this.StartCoroutine(this.Attack(9));
        }

        this.InputMagnitude();

        //If you don't need the character grounded then get rid of this part.
        this.isGrounded = this.controller.isGrounded;
        if (this.isGrounded)
        {
            this.verticalVel = 0;
        }
        else
        {
            this.verticalVel -= 1f * Time.deltaTime;
        }
        this.moveVector = new Vector3(0, this.verticalVel, 0);
        this.controller.Move(this.moveVector);
    }

    public IEnumerator Attack(int EffectNumber)
    {
        //Block moving after using the skill
        if (EffectNumber != 6)
        {
            this.canMove = false;
        }

        this.SetAnimZero();
        while (true)
        {
            if (EffectNumber == 0)
            {
                this.anim.SetTrigger("Attack1");
                yield return new WaitForSeconds(this.castingTime[EffectNumber]);
                this.StartCoroutine(this.cameraShaker.Shake(0.4f, 7, 0.45f, 0));
                GameObject projectile = Instantiate(this.Prefabs[0], this.FirePoint.position, this.FirePoint.rotation);
                projectile.GetComponent<TargetProjectile>().UpdateTarget(this.target, this.uiOffset);
                yield return new WaitForSeconds(0.2f);
            }
            if (EffectNumber == 1)
            {
                this.anim.SetTrigger("AoE");
                yield return new WaitForSeconds(this.castingTime[EffectNumber]);
                this.parentObject = this.Prefabs[EffectNumber].transform.parent;
                this.Prefabs[EffectNumber].transform.parent = null;
                this.Prefabs[EffectNumber].GetComponent<ParticleSystem>().Play();
                this.StartCoroutine(this.cameraShaker.Shake(0.4f, 7, 0.6f, 0));
                yield return new WaitForSeconds(this.castingTime[EffectNumber]);
            }
            if (EffectNumber == 4)
            {
                this.anim.SetTrigger("Attack2");
                if (this.PrefabsCast[7].GetComponent<AudioSource>())
                {
                    this.soundComponentCast = this.PrefabsCast[7].GetComponent<AudioSource>();
                    this.clip = this.soundComponentCast.clip;
                    this.soundComponentCast.PlayOneShot(this.clip);
                }
                yield return new WaitForSeconds(this.castingTime[EffectNumber]);
                this.StartCoroutine(this.cameraShaker.Shake(0.4f, 8, 0.45f, 0));
                GameObject projectile = Instantiate(this.Prefabs[4], this.FirePoint.position, this.FirePoint.rotation);
                projectile.GetComponent<TargetProjectile>().UpdateTarget(this.target, this.uiOffset);
                yield return new WaitForSeconds(0.3f);
            }
            if (EffectNumber == 6)
            {
                this.anim.SetTrigger("MaskAttack2");
                this.secondLayerWeight = Mathf.Lerp(this.secondLayerWeight, 1f, Time.deltaTime * 60);
                yield return new WaitForSeconds(this.castingTime[EffectNumber]);
                this.parentObject = this.Prefabs[EffectNumber].transform.parent;
                this.Prefabs[EffectNumber].transform.parent = null;
                this.Prefabs[EffectNumber].transform.position = this.target.position;
                this.Prefabs[EffectNumber].GetComponent<ParticleSystem>().Play();
                if (this.Prefabs[EffectNumber].GetComponent<AudioSource>())
                {
                    this.soundComponent = this.Prefabs[EffectNumber].GetComponent<AudioSource>();
                    this.clip = this.soundComponent.clip;
                    this.soundComponent.PlayOneShot(this.clip);
                }
                this.StartCoroutine(this.cameraShaker.Shake(0.3f, 8, 1.1f, 0.2f));
                yield return new WaitForSeconds(1.5f);
            }
            this.canMove = true;
            if (EffectNumber == 1 || EffectNumber == 6)
            {
                yield return new WaitForSeconds(0.5f);
                this.Prefabs[EffectNumber].transform.parent = this.parentObject;
                this.Prefabs[EffectNumber].transform.localPosition = new Vector3(0, 0, 0);
                this.Prefabs[EffectNumber].transform.localRotation = Quaternion.identity;
            }
            if (EffectNumber == 9)
            {
                this.Prefabs[EffectNumber].GetComponent<ParticleSystem>().Play();
                if (this.Prefabs[EffectNumber].GetComponent<AudioSource>())
                {
                    this.soundComponent = this.Prefabs[EffectNumber].GetComponent<AudioSource>();
                    this.clip = this.soundComponent.clip;
                    this.soundComponent.PlayOneShot(this.clip);
                }
            }
            yield break;
        }
    }

    public IEnumerator FrontAttack(int EffectNumber)
    {
        if (this.TargetMarker2 && this.casting == false)
        {
            this.aim.enabled = false;
            this.TargetMarker2.SetActive(true);
            //Waiting for confirm or deny
            while (true)
            {
                Vector3 forwardCamera = Camera.main.transform.forward;
                forwardCamera.y = 0.0f;
                this.TargetMarker2.transform.rotation = Quaternion.LookRotation(forwardCamera);
                Vector3 vecPos = this.transform.position + forwardCamera * 4;

                if (Input.GetMouseButtonDown(0) && this.casting == false)
                {
                    this.casting = true;
                    this.canMove = false;
                    this.SetAnimZero();
                    this.TargetMarker2.SetActive(false);
                    if (this.rotateState == false)
                    {
                        this.StartCoroutine(this.RotateToTarget(0.5f, vecPos));
                    }
                    this.anim.SetTrigger("Attack2");
                    if (EffectNumber == 2)
                    {
                        this.PrefabsCast[4].GetComponent<ParticleSystem>().Play();
                        this.soundComponentCast = this.PrefabsCast[4].GetComponent<AudioSource>();
                        this.clip = this.soundComponentCast.clip;
                        this.soundComponentCast.PlayOneShot(this.clip);
                    }
                    if (EffectNumber == 8)
                    {
                        if (this.PrefabsCast[13].GetComponent<AudioSource>())
                        {
                            this.soundComponentCast = this.PrefabsCast[13].GetComponent<AudioSource>();
                            this.clip = this.soundComponentCast.clip;
                            this.soundComponentCast.PlayOneShot(this.clip);
                        }
                        this.PrefabsCast[13].GetComponent<ParticleSystem>().Play();
                        yield return new WaitForSeconds(this.castingTime[EffectNumber]);
                    }

                    //Use FrontAttack script if exist (it has own settings)
                    if (this.Prefabs[EffectNumber].GetComponent<FrontAttack>() != null)
                    {
                        this.StartCoroutine(this.cameraShaker.Shake(0.5f, 6, 1.3f, 0.0f));
                        this.parentObject = this.Prefabs[EffectNumber].transform.parent;
                        this.Prefabs[EffectNumber].transform.parent = null;
                        foreach (FrontAttack component in this.Prefabs[EffectNumber].GetComponentsInChildren<FrontAttack>())
                        {
                            component.playMeshEffect = true;
                        }
                        yield return new WaitForSeconds(0.2f);
                        this.aim.enabled = true;
                        this.canMove = true;
                        yield return new WaitForSeconds(0.8f);
                        this.Prefabs[EffectNumber].transform.parent = this.parentObject;
                        this.Prefabs[EffectNumber].transform.localPosition = new Vector3(0, 0, 0);
                        this.Prefabs[EffectNumber].transform.localRotation = Quaternion.identity;
                    }
                    else
                    {
                        this.parentObject = this.Prefabs[EffectNumber].transform.parent;
                        this.Prefabs[EffectNumber].transform.parent = null;
                        this.Prefabs[EffectNumber].transform.rotation = Quaternion.LookRotation(forwardCamera);
                        ParticleSystem effect = this.Prefabs[EffectNumber].GetComponent<ParticleSystem>();
                        effect.Play();
                        this.StartCoroutine(this.cameraShaker.Shake(0.5f, 7, 0.6f, 0.26f));
                        yield return new WaitForSeconds(this.castingTime[EffectNumber]);
                        this.aim.enabled = true;
                        this.canMove = true;
                        yield return new WaitForSeconds(0.5f);
                        this.Prefabs[EffectNumber].transform.parent = this.parentObject;
                        this.Prefabs[EffectNumber].transform.localPosition = new Vector3(0, 1, 0);
                        this.Prefabs[EffectNumber].transform.localRotation = Quaternion.identity;
                    }
                    this.casting = false;
                    yield break;
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    this.TargetMarker2.SetActive(false);
                    this.aim.enabled = true;
                    yield break;
                }
                yield return null;
            }
        }
    }

    public IEnumerator PreCast(int EffectNumber)
    {
        if (this.PrefabsCast[EffectNumber] && this.TargetMarker && this.casting == false)
        {
            this.aim.enabled = false;
            this.TargetMarker.SetActive(true);
            //Waiting for confirm or deny
            while (true)
            {
                Vector3 forwardCamera = Camera.main.transform.forward;
                forwardCamera.y = 0.0f;
                RaycastHit hit;
                Ray ray = new Ray(Camera.main.transform.position + new Vector3(0, 2, 0), Camera.main.transform.forward);
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, this.collidingLayer))
                {
                    this.TargetMarker.transform.position = hit.point;
                    this.TargetMarker.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * Quaternion.LookRotation(forwardCamera);
                }
                else
                {
                    this.aim.enabled = true;
                    this.TargetMarker.SetActive(false);
                }

                if (Input.GetMouseButtonDown(0) && this.casting == false)
                {
                    this.canMove = false;
                    this.casting = true;
                    this.aim.enabled = true;
                    this.TargetMarker.SetActive(false);
                    if (this.rotateState == false)
                    {
                        this.StartCoroutine(this.RotateToTarget(0.5f, hit.point));
                    }

                    if (EffectNumber == 3)
                    {
                        this.anim.SetTrigger("UpAttack");
                        if (this.PrefabsCast[5].GetComponent<AudioSource>())
                        {
                            this.soundComponentCast = this.PrefabsCast[5].GetComponent<AudioSource>();
                            this.clip = this.soundComponentCast.clip;
                            this.soundComponentCast.PlayOneShot(this.clip);
                        }
                        this.StartCoroutine(this.cameraShaker.Shake(0.4f, 9, 0.4f, 0.2f));
                        for (int i = 5; i <= 6; i++)
                        {
                            this.PrefabsCast[i].GetComponent<ParticleSystem>().Play();
                        }
                        yield return new WaitForSeconds(this.castingTime[EffectNumber]);
                        this.StartCoroutine(this.cameraShaker.Shake(0.5f, 7, 1.4f, 0));
                        this.parentObject = this.Prefabs[3].transform.parent;
                        this.Prefabs[3].transform.position = hit.point;
                        this.Prefabs[3].transform.rotation = Quaternion.LookRotation(forwardCamera);
                        this.Prefabs[3].transform.parent = null;
                        this.Prefabs[3].GetComponent<ParticleSystem>().Play();
                        if (this.PrefabsCast[3].GetComponent<AudioSource>())
                        {
                            this.soundComponent = this.Prefabs[3].GetComponent<AudioSource>();
                            this.clip = this.soundComponent.clip;
                            this.soundComponent.PlayOneShot(this.clip);
                        }
                    }

                    if (EffectNumber == 7)
                    {
                        this.anim.SetTrigger("UpAttack2");
                        this.StartCoroutine(this.cameraShaker.Shake(0.4f, 8, 0.4f, 0.2f));
                        this.PrefabsCast[11].GetComponent<ParticleSystem>().Play();
                        if (this.PrefabsCast[11].GetComponent<AudioSource>())
                        {
                            this.soundComponentCast = this.PrefabsCast[11].GetComponent<AudioSource>();
                            this.clip = this.soundComponentCast.clip;
                            this.soundComponentCast.PlayOneShot(this.clip);
                        }
                        this.PrefabsCast[12].GetComponent<ParticleSystem>().Play();
                        yield return new WaitForSeconds(this.castingTime[EffectNumber]);
                        this.StartCoroutine(this.cameraShaker.Shake(0.3f, 7, 0.4f, 0));
                        this.parentObject = this.Prefabs[7].transform.parent;
                        this.Prefabs[7].transform.position = hit.point;
                        this.Prefabs[7].transform.rotation = Quaternion.LookRotation(forwardCamera);
                        this.Prefabs[7].transform.parent = null;
                        this.Prefabs[7].GetComponent<ParticleSystem>().Play();
                        if (this.Prefabs[7].GetComponent<AudioSource>())
                        {
                            this.soundComponent = this.Prefabs[7].GetComponent<AudioSource>();
                            this.clip = this.soundComponent.clip;
                            this.soundComponent.PlayOneShot(this.clip);
                        }
                    }

                    this.canMove = true;
                    if (EffectNumber == 3 && EffectNumber == 7)
                    {
                        yield return new WaitForSeconds(2);
                        this.Prefabs[EffectNumber].transform.parent = this.parentObject;
                        this.Prefabs[EffectNumber].transform.localPosition = new Vector3(0, 1, 0);
                        this.Prefabs[EffectNumber].transform.localRotation = Quaternion.identity;
                    }
                    this.casting = false;

                    yield break;
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    this.TargetMarker.SetActive(false);
                    this.aim.enabled = true;
                    yield break;
                }
                yield return null;
            }
        }
    }

    public void StopCasting(int EffectNumber)
    {
        this.Prefabs[EffectNumber].transform.parent = this.parentObject;
        this.Prefabs[EffectNumber].transform.localPosition = new Vector3(0, 0, 0);
        /*if (EffectNumber == 2)
            anim.Play("Blend Tree");*/
        this.casting = false;
        this.canMove = true;
    }

    //For standing after skill animation
    private void SetAnimZero()
    {
        this.anim.SetFloat("InputMagnitude", 0);
        this.anim.SetFloat("InputZ", 0);
        this.anim.SetFloat("InputX", 0);
    }

    //Rotate player to target when attack
    public IEnumerator RotateToTarget(float rotatingTime, Vector3 targetPoint)
    {
        this.rotateState = true;
        float delay = rotatingTime;
        Vector3 lookPos = targetPoint - this.transform.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        while (true)
        {
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, rotation, Time.deltaTime * 20);
            delay -= Time.deltaTime;
            if (delay <= 0 || this.transform.rotation == rotation)
            {
                this.rotateState = false;
                yield break;
            }
            yield return null;
        }
    }

    private void PlayerMoveAndRotation()
    {
        this.InputX = Input.GetAxis("Horizontal");
        this.InputZ = Input.GetAxis("Vertical");

        Camera camera = Camera.main;
        Vector3 forward = this.cam.transform.forward;
        Vector3 right = this.cam.transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        //Movement vector
        this.desiredMoveDirection = forward * this.InputZ + right * this.InputX;

        //Character diagonal movement faster fix
        this.desiredMoveDirection.Normalize();

        if (this.blockRotationPlayer == false)
        {
            if (this.aimMoving)
            {
                this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(forward), this.desiredRotationSpeed);
                //Limit back speed
                if (this.InputZ < -0.3)
                {
                    this.controller.Move(this.desiredMoveDirection * Time.deltaTime * (this.velocity / 2.4f));
                }
                else if (this.InputX < -0.1 || this.InputX > 0.1)
                {
                    this.controller.Move(this.desiredMoveDirection * Time.deltaTime * (this.velocity / 2.2f));
                }
                else
                {
                    this.controller.Move(this.desiredMoveDirection * Time.deltaTime * this.velocity / 1.8f);
                }
            }
            else
            {
                this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(this.desiredMoveDirection), this.desiredRotationSpeed);
                this.controller.Move(this.desiredMoveDirection * Time.deltaTime * this.velocity);
            }
        }
    }

    private void InputMagnitude()
    {
        //Calculate Input Vectors
        this.InputX = Input.GetAxis("Horizontal");
        this.InputZ = Input.GetAxis("Vertical");

        this.anim.SetFloat("InputZ", this.InputZ, this.VerticalAnimTime, Time.deltaTime * 2f);
        this.anim.SetFloat("InputX", this.InputX, this.HorizontalAnimSmoothTime, Time.deltaTime * 2f);

        //Calculate the Input Magnitude
        this.Speed = new Vector2(this.InputX, this.InputZ).sqrMagnitude;

        //Change blend trees moving animation
        this.anim.SetBool("AimMoving", this.aimMoving);

        //Physically move player
        if (this.Speed > this.allowPlayerRotation)
        {
            this.anim.SetFloat("InputMagnitude", this.Speed, this.StartAnimTime, Time.deltaTime);
            this.PlayerMoveAndRotation();
        }
        else if (this.Speed < this.allowPlayerRotation)
        {
            this.anim.SetFloat("InputMagnitude", this.Speed, this.StopAnimTime, Time.deltaTime);
        }
    }

    private void UserInterface()
    {
        Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0) / 2;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(this.target.position + (Vector3)this.uiOffset);
        Vector3 CornerDistance = screenPos - screenCenter;
        Vector3 absCornerDistance = new Vector3(Mathf.Abs(CornerDistance.x), Mathf.Abs(CornerDistance.y), Mathf.Abs(CornerDistance.z));

        if (absCornerDistance.x < screenCenter.x / 3 && absCornerDistance.y < screenCenter.y / 3 && screenPos.x > 0 && screenPos.y > 0 && screenPos.z > 0 //If target is in the middle of the screen
            && !Physics.Linecast(this.transform.position + (Vector3)this.uiOffset, this.target.position + (Vector3)this.uiOffset * 2, this.collidingLayer)) //If player can see the target
        {
            this.aim.transform.position = Vector3.MoveTowards(this.aim.transform.position, screenPos, Time.deltaTime * 3000);
            if (!this.activeTarger)
            {
                this.activeTarger = true;
            }
        }
        else
        {
            this.aim.transform.position = Vector3.MoveTowards(this.aim.transform.position, screenCenter, Time.deltaTime * 3000);
            if (this.activeTarger)
            {
                this.activeTarger = false;
            }
        }
    }

    public int targetIndex()
    {
        float[] distances = new float[this.screenTargets.Count];

        for (int i = 0; i < this.screenTargets.Count; i++)
        {
            distances[i] = Vector2.Distance(Camera.main.WorldToScreenPoint(this.screenTargets[i].position), new Vector2(Screen.width / 2, Screen.height / 2));
        }

        float minDistance = Mathf.Min(distances);
        int index = 0;

        for (int i = 0; i < distances.Length; i++)
        {
            if (minDistance == distances[i])
            {
                index = i;
            }
        }
        return index;
    }
}
