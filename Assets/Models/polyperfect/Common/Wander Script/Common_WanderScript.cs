using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PolyPerfect
{
    [RequireComponent(typeof(Animator)), RequireComponent(typeof(CharacterController))]
    public class Common_WanderScript : MonoBehaviour
    {
        private const float contingencyDistance = 1f;

        [SerializeField] public IdleState[] idleStates;
        [SerializeField] private MovementState[] movementStates;
        [SerializeField] private AIState[] attackingStates;
        [SerializeField] private AIState[] deathStates;

        [SerializeField] public string species = "NA";

        [SerializeField, Tooltip("This specific animal stats asset, create a new one from the asset menu under (LowPolyAnimals/NewAnimalStats)")]
        public AIStats stats;

        [SerializeField, Tooltip("How far away from it's origin this animal will wander by itself.")]
        private float wanderZone = 10f;

        public float MaxDistance
        {
            get => this.wanderZone;
            set
            {
#if UNITY_EDITOR
                SceneView.RepaintAll();
#endif
                this.wanderZone = value;
            }
        }

        // [SerializeField, Tooltip("How dominent this animal is in the food chain, agressive animals will attack less dominant animals.")]
        private int dominance = 1;
        private int originalDominance = 0;

        [SerializeField, Tooltip("How far this animal can sense a predator.")]
        private float awareness = 30f;

        [SerializeField, Tooltip("How far this animal can sense it's prey.")]
        private float scent = 30f;

        private float originalScent = 0f;

        // [SerializeField, Tooltip("How many seconds this animal can run for before it gets tired.")]
        private float stamina = 10f;

        // [SerializeField, Tooltip("How much this damage this animal does to another animal.")]
        private float power = 10f;

        // [SerializeField, Tooltip("How much health this animal has.")]
        private float toughness = 5f;

        // [SerializeField, Tooltip("Chance of this animal attacking another animal."), Range(0f, 100f)]
        private float aggression = 0f;
        private float originalAggression = 0f;

        // [SerializeField, Tooltip("How quickly the animal does damage to another animal (every 'attackSpeed' seconds will cause 'power' amount of damage).")]
        private float attackSpeed = 0.5f;

        // [SerializeField, Tooltip("If true, this animal will attack other animals of the same specices.")]
        private bool territorial = false;

        // [SerializeField, Tooltip("Stealthy animals can't be detected by other animals.")]
        private bool stealthy = false;

        [SerializeField, Tooltip("If true, this animal will never leave it's zone, even if it's chasing or running away from another animal.")]
        private bool constainedToWanderZone = false;

        [SerializeField, Tooltip("This animal will be peaceful towards species in this list.")]
        private string[] nonAgressiveTowards;

        private static List<Common_WanderScript> allAnimals = new List<Common_WanderScript>();

        public static List<Common_WanderScript> AllAnimals => allAnimals;

        //[Space(), Space(5)]
        [SerializeField, Tooltip("If true, this animal will rotate to match the terrain. Ensure you have set the layer of the terrain as 'Terrain'.")]
        private bool matchSurfaceRotation = false;

        [SerializeField, Tooltip("How fast the animnal rotates to match the surface rotation.")]
        private float surfaceRotationSpeed = 2f;

        //[Space(), Space(5)]
        [SerializeField, Tooltip("If true, AI changes to this animal will be logged in the console.")]
        private bool logChanges = false;

        [SerializeField, Tooltip("If true, gizmos will be drawn in the editor.")]
        private bool showGizmos = false;

        [SerializeField] private bool drawWanderRange = true;
        [SerializeField] private bool drawScentRange = true;
        [SerializeField] private bool drawAwarenessRange = true;

        public UnityEngine.Events.UnityEvent deathEvent;
        public UnityEngine.Events.UnityEvent attackingEvent;
        public UnityEngine.Events.UnityEvent idleEvent;
        public UnityEngine.Events.UnityEvent movementEvent;


        private Color distanceColor = new Color(0f, 0f, 205f);
        private Color awarnessColor = new Color(1f, 0f, 1f, 1f);
        private Color scentColor = new Color(1f, 0f, 0f, 1f);
        private Animator animator;
        private CharacterController characterController;
        private NavMeshAgent navMeshAgent;
        private Vector3 origin;

        private int totalIdleStateWeight;

        private bool useNavMesh = false;
        private Vector3 targetLocation = Vector3.zero;

        private float turnSpeed = 0f;

        public enum WanderState
        {
            Idle,
            Wander,
            Chase,
            Evade,
            Attack,
            Dead
        }

        private float attackTimer = 0;

        private float MinimumStaminaForAggression => this.stats.stamina * .9f;

        private float MinimumStaminaForFlee => this.stats.stamina * .1f;

        public WanderState CurrentState;
        private Common_WanderScript primaryPrey;
        private Common_WanderScript primaryPursuer;
        private Common_WanderScript attackTarget;
        private float moveSpeed = 0f;
        private float attackReach = 2f;
        private bool forceUpdate = false;
        private float idleStateDuration;
        private Vector3 startPosition;
        private Vector3 wanderTarget;
        private IdleState currentIdleState;
        private float idleUpdateTime;


        public void OnDrawGizmosSelected()
        {
            if (!this.showGizmos)
            {
                return;
            }

            if (this.drawWanderRange)
            {
                // Draw circle of radius wander zone
                Gizmos.color = this.distanceColor;
                Gizmos.DrawWireSphere(this.origin == Vector3.zero ? this.transform.position : this.origin, this.wanderZone);

                Vector3 IconWander = new Vector3(this.transform.position.x, this.transform.position.y + this.wanderZone, this.transform.position.z);
                Gizmos.DrawIcon(IconWander, "ico-wander", true);
            }

            if (this.drawAwarenessRange)
            {
                //Draw circle radius for Awarness.
                Gizmos.color = this.awarnessColor;
                Gizmos.DrawWireSphere(this.transform.position, this.awareness);


                Vector3 IconAwareness = new Vector3(this.transform.position.x, this.transform.position.y + this.awareness, this.transform.position.z);
                Gizmos.DrawIcon(IconAwareness, "ico-awareness", true);
            }

            if (this.drawScentRange)
            {
                //Draw circle radius for Scent.
                Gizmos.color = this.scentColor;
                Gizmos.DrawWireSphere(this.transform.position, this.scent);

                Vector3 IconScent = new Vector3(this.transform.position.x, this.transform.position.y + this.scent, this.transform.position.z);
                Gizmos.DrawIcon(IconScent, "ico-scent", true);
            }

            if (!Application.isPlaying)
            {
                return;
            }

            // Draw target position.
            if (this.useNavMesh)
            {
                if (this.navMeshAgent.remainingDistance > 1f)
                {
                    Gizmos.DrawSphere(this.navMeshAgent.destination + new Vector3(0f, 0.1f, 0f), 0.2f);
                    Gizmos.DrawLine(this.transform.position, this.navMeshAgent.destination);
                }
            }
            else
            {
                if (this.targetLocation != Vector3.zero)
                {
                    Gizmos.DrawSphere(this.targetLocation + new Vector3(0f, 0.1f, 0f), 0.2f);
                    Gizmos.DrawLine(this.transform.position, this.targetLocation);
                }
            }
        }

        private void Awake()
        {
            if (!this.stats)
            {
                Debug.LogError(string.Format("No stats attached to {0}'s Wander Script.", this.gameObject.name));
                this.enabled = false;
                return;
            }

            this.animator = this.GetComponent<Animator>();

            RuntimeAnimatorController runtimeController = this.animator.runtimeAnimatorController;
            if (this.animator)
            {
                this.animatorParameters.UnionWith(this.animator.parameters.Select(p => p.name));
            }

            if (this.logChanges)
            {
                if (runtimeController == null)
                {
                    Debug.LogError(string.Format(
                        "{0} has no animator controller, make sure you put one in to allow the character to walk. See documentation for more details (1)",
                        this.gameObject.name));
                    this.enabled = false;
                    return;
                }

                if (this.animator.avatar == null)
                {
                    Debug.LogError(string.Format("{0} has no avatar, make sure you put one in to allow the character to animate. See documentation for more details (2)",
                        this.gameObject.name));
                    this.enabled = false;
                    return;
                }

                if (this.animator.hasRootMotion == true)
                {
                    Debug.LogError(string.Format(
                        "{0} has root motion applied, consider turning this off as our script will deactivate this on play as we do not use it (3)", this.gameObject.name));
                    this.animator.applyRootMotion = false;
                }

                if (this.idleStates.Length == 0 || this.movementStates.Length == 0)
                {
                    Debug.LogError(string.Format("{0} has no idle or movement states, make sure you fill these out. See documentation for more details (4)",
                        this.gameObject.name));
                    this.enabled = false;
                    return;
                }

                if (this.idleStates.Length > 0)
                {
                    for (int i = 0; i < this.idleStates.Length; i++)
                    {
                        if (this.idleStates[i].animationBool == "")
                        {
                            Debug.LogError(string.Format(
                                "{0} has " + this.idleStates.Length +
                                " Idle states, you need to make sure that each state has an animation boolean. See documentation for more details (4)", this.gameObject.name));
                            this.enabled = false;
                            return;
                        }
                    }
                }

                if (this.movementStates.Length > 0)
                {
                    for (int i = 0; i < this.movementStates.Length; i++)
                    {
                        if (this.movementStates[i].animationBool == "")
                        {
                            Debug.LogError(string.Format(
                                "{0} has " + this.movementStates.Length +
                                " Movement states, you need to make sure that each state has an animation boolean to see the character walk. See documentation for more details (4)",
                                this.gameObject.name));
                            this.enabled = false;
                            return;
                        }

                        if (this.movementStates[i].moveSpeed <= 0)
                        {
                            Debug.LogError(string.Format(
                                "{0} has a movement state with a speed of 0 or less, you need to set the speed higher than 0 to see the character move. See documentation for more details (4)",
                                this.gameObject.name));
                            this.enabled = false;
                            return;
                        }

                        if (this.movementStates[i].turnSpeed <= 0)
                        {
                            Debug.LogError(string.Format(
                                "{0} has a turn speed state with a speed of 0 or less, you need to set the speed higher than 0 to see the character turn. See documentation for more details (4)",
                                this.gameObject.name));
                            this.enabled = false;
                            return;
                        }
                    }
                }

                if (this.attackingStates.Length == 0)
                {
                    Debug.Log(string.Format("{0} has " + this.attackingStates.Length + " this character will not be able to attack. See documentation for more details (4)",
                        this.gameObject.name));
                }

                if (this.attackingStates.Length > 0)
                {
                    for (int i = 0; i < this.attackingStates.Length; i++)
                    {
                        if (this.attackingStates[i].animationBool == "")
                        {
                            Debug.LogError(string.Format(
                                "{0} has " + this.attackingStates.Length +
                                " attacking states, you need to make sure that each state has an animation boolean. See documentation for more details (4)",
                                this.gameObject.name));
                            this.enabled = false;
                            return;
                        }
                    }
                }

                if (this.stats == null)
                {
                    Debug.LogError(string.Format("{0} has no AI stats, make sure you assign one to the wander script. See documentation for more details (5)",
                        this.gameObject.name));
                    this.enabled = false;
                    return;
                }

                if (this.animator)
                {
                    foreach (AIState item in this.AllStates)
                    {
                        if (!this.animatorParameters.Contains(item.animationBool))
                        {
                            Debug.LogError(string.Format(
                                "{0} did not contain {1}. Make sure you set it in the Animation States on the character, and have a matching parameter in the Animator Controller assigned.",
                                this.gameObject.name, item.animationBool));
                            this.enabled = false;
                            return;
                        }
                    }
                }
            }

            foreach (IdleState state in this.idleStates)
            {
                this.totalIdleStateWeight += state.stateWeight;
            }

            this.origin = this.transform.position;
            this.animator.applyRootMotion = false;
            this.characterController = this.GetComponent<CharacterController>();
            this.navMeshAgent = this.GetComponent<NavMeshAgent>();

            //Assign the stats to variables
            this.originalDominance = this.stats.dominance;
            this.dominance = this.originalDominance;

            this.toughness = this.stats.toughness;
            this.territorial = this.stats.territorial;

            this.stamina = this.stats.stamina;

            this.originalAggression = this.stats.agression;
            this.aggression = this.originalAggression;

            this.attackSpeed = this.stats.attackSpeed;
            this.stealthy = this.stats.stealthy;

            this.originalScent = this.scent;
            this.scent = this.originalScent;

            if (this.navMeshAgent)
            {
                this.useNavMesh = true;
                this.navMeshAgent.stoppingDistance = contingencyDistance;
            }

            if (this.matchSurfaceRotation && this.transform.childCount > 0)
            {
                this.transform.GetChild(0).gameObject.AddComponent<Common_SurfaceRotation>().SetRotationSpeed(this.surfaceRotationSpeed);
            }
        }

        private IEnumerable<AIState> AllStates
        {
            get
            {
                foreach (IdleState item in this.idleStates)
                {
                    yield return item;
                }

                foreach (MovementState item in this.movementStates)
                {
                    yield return item;
                }

                foreach (AIState item in this.attackingStates)
                {
                    yield return item;
                }

                foreach (AIState item in this.deathStates)
                {
                    yield return item;
                }
            }
        }

        private void OnEnable()
        {
            allAnimals.Add(this);
        }

        private void OnDisable()
        {
            allAnimals.Remove(this);
            this.StopAllCoroutines();
        }


        private void Start()
        {
            this.startPosition = this.transform.position;
            if (Common_WanderManager.Instance != null && Common_WanderManager.Instance.PeaceTime)
            {
                this.SetPeaceTime(true);
            }

            this.StartCoroutine(this.RandomStartingDelay());
        }

        private bool started = false;
        private readonly HashSet<string> animatorParameters = new HashSet<string>();

        private void Update()
        {
            if (!this.started)
            {
                return;
            }

            if (this.forceUpdate)
            {
                this.UpdateAI();
                this.forceUpdate = false;
            }

            if (this.CurrentState == WanderState.Attack)
            {
                if (!this.attackTarget || this.attackTarget.CurrentState == WanderState.Dead)
                {
                    Common_WanderScript previous = this.attackTarget;
                    this.UpdateAI();
                    if (previous && previous == this.attackTarget)
                    {
                        Debug.LogError(string.Format("Target was same {0}", previous.gameObject.name));
                    }
                }

                this.attackTimer += Time.deltaTime;
            }

            if (this.attackTimer > this.attackSpeed)
            {
                this.attackTimer -= this.attackSpeed;
                if (this.attackTarget)
                {
                    this.attackTarget.TakeDamage(this.power);
                }

                if (this.attackTarget.CurrentState == WanderState.Dead)
                {
                    this.UpdateAI();
                }
            }

            Vector3 position = this.transform.position;
            Vector3 targetPosition = position;
            switch (this.CurrentState)
            {
                case WanderState.Attack:
                    this.FaceDirection((this.attackTarget.transform.position - position).normalized);
                    targetPosition = position;
                    break;
                case WanderState.Chase:
                    if (!this.primaryPrey || this.primaryPrey.CurrentState == WanderState.Dead)
                    {
                        this.primaryPrey = null;
                        this.SetState(WanderState.Idle);
                        goto case WanderState.Idle;
                    }
                    targetPosition = this.primaryPrey.transform.position;
                    this.ValidatePosition(ref targetPosition);
                    if (!this.IsValidLocation(targetPosition))
                    {
                        this.SetState(WanderState.Idle);
                        targetPosition = position;
                        this.UpdateAI();
                        break;
                    }

                    this.FaceDirection((targetPosition - position).normalized);
                    this.stamina -= Time.deltaTime;
                    if (this.stamina <= 0f)
                    {
                        this.UpdateAI();
                    }

                    break;
                case WanderState.Evade:
                    targetPosition = position + Vector3.ProjectOnPlane(position - this.primaryPursuer.transform.position, Vector3.up);
                    if (!this.IsValidLocation(targetPosition))
                    {
                        targetPosition = this.startPosition;
                    }

                    this.ValidatePosition(ref targetPosition);
                    this.FaceDirection((targetPosition - position).normalized);
                    this.stamina -= Time.deltaTime;
                    if (this.stamina <= 0f)
                    {
                        this.UpdateAI();
                    }

                    break;
                case WanderState.Wander:
                    this.stamina = Mathf.MoveTowards(this.stamina, this.stats.stamina, Time.deltaTime);
                    targetPosition = this.wanderTarget;
                    Debug.DrawLine(position, targetPosition, Color.yellow);
                    this.FaceDirection((targetPosition - position).normalized);
                    Vector3 displacementFromTarget = Vector3.ProjectOnPlane(targetPosition - this.transform.position, Vector3.up);
                    if (displacementFromTarget.magnitude < contingencyDistance)
                    {
                        this.SetState(WanderState.Idle);
                        this.UpdateAI();
                    }

                    break;
                case WanderState.Idle:
                    this.stamina = Mathf.MoveTowards(this.stamina, this.stats.stamina, Time.deltaTime);
                    if (Time.time >= this.idleUpdateTime)
                    {
                        this.SetState(WanderState.Wander);
                        this.UpdateAI();
                    }
                    break;
            }

            if (this.navMeshAgent)
            {
                this.navMeshAgent.destination = targetPosition;
                this.navMeshAgent.speed = this.moveSpeed;
                this.navMeshAgent.angularSpeed = this.turnSpeed;
            }
            else
            {
                this.characterController.SimpleMove(this.moveSpeed * UnityEngine.Vector3.ProjectOnPlane(targetPosition - position, Vector3.up).normalized);
            }
        }

        private void FaceDirection(Vector3 facePosition)
        {
            this.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(Vector3.RotateTowards(this.transform.forward,
                facePosition, this.turnSpeed * Time.deltaTime * Mathf.Deg2Rad, 0f), Vector3.up), Vector3.up);
        }

        public void TakeDamage(float damage)
        {
            this.toughness -= damage;
            if (this.toughness <= 0f)
            {
                this.Die();
            }
        }
        public void Die()
        {
            this.SetState(WanderState.Dead);
        }

        public void SetPeaceTime(bool peace)
        {
            if (peace)
            {
                this.dominance = 0;
                this.scent = 0f;
                this.aggression = 0f;
            }
            else
            {
                this.dominance = this.originalDominance;
                this.scent = this.originalScent;
                this.aggression = this.originalAggression;
            }
        }

        private void UpdateAI()
        {
            if (this.CurrentState == WanderState.Dead)
            {
                Debug.LogError("Trying to update the AI of a dead animal, something probably went wrong somewhere.");
                return;
            }

            Vector3 position = this.transform.position;
            this.primaryPursuer = null;
            if (this.awareness > 0)
            {
                float closestDistance = this.awareness;
                if (allAnimals.Count > 0)
                {
                    foreach (Common_WanderScript chaser in allAnimals)
                    {
                        if (chaser.primaryPrey != this && chaser.attackTarget != this)
                        {
                            continue;
                        }

                        if (chaser.CurrentState == WanderState.Dead)
                        {
                            continue;
                        }

                        float distance = Vector3.Distance(position, chaser.transform.position);
                        if ((chaser.attackTarget != this && chaser.stealthy) || chaser.dominance <= this.dominance || distance > closestDistance)
                        {
                            continue;
                        }

                        closestDistance = distance;
                        this.primaryPursuer = chaser;
                    }
                }
            }

            bool wasSameTarget = false;
            if (this.primaryPrey)
            {
                if (this.primaryPrey.CurrentState == WanderState.Dead)
                {
                    this.primaryPrey = null;
                }
                else
                {
                    float distanceToPrey = Vector3.Distance(position, this.primaryPrey.transform.position);
                    if (distanceToPrey > this.scent)
                    {
                        this.primaryPrey = null;
                    }
                    else
                    {
                        wasSameTarget = true;
                    }
                }
            }
            if (!this.primaryPrey)
            {
                this.primaryPrey = null;
                if (this.dominance > 0 && this.attackingStates.Length > 0)
                {
                    float aggFrac = this.aggression * .01f;
                    aggFrac *= aggFrac;
                    float closestDistance = this.scent;
                    foreach (Common_WanderScript potentialPrey in allAnimals)
                    {
                        if (potentialPrey.CurrentState == WanderState.Dead)
                        {
                            Debug.LogError(string.Format("Dead animal found: {0}", potentialPrey.gameObject.name));
                        }

                        if (potentialPrey == this || (potentialPrey.species == this.species && !this.territorial) ||
                            potentialPrey.dominance > this.dominance || potentialPrey.stealthy)
                        {
                            continue;
                        }

                        if (this.nonAgressiveTowards.Contains(potentialPrey.species))
                        {
                            continue;
                        }

                        if (Random.Range(0f, 0.99999f) >= aggFrac)
                        {
                            continue;
                        }

                        Vector3 preyPosition = potentialPrey.transform.position;
                        if (!this.IsValidLocation(preyPosition))
                        {
                            continue;
                        }

                        float distance = Vector3.Distance(position, preyPosition);
                        if (distance > closestDistance)
                        {
                            continue;
                        }

                        if (this.logChanges)
                        {
                            Debug.Log(string.Format("{0}: Found prey ({1}), chasing.", this.gameObject.name, potentialPrey.gameObject.name));
                        }

                        closestDistance = distance;
                        this.primaryPrey = potentialPrey;
                    }
                }
            }

            bool aggressiveOption = false;
            if (this.primaryPrey)
            {
                if ((wasSameTarget && this.stamina > 0) || this.stamina > this.MinimumStaminaForAggression)
                {
                    aggressiveOption = true;
                }
                else
                {
                    this.primaryPrey = null;
                }
            }

            bool defensiveOption = false;
            if (this.primaryPursuer && !aggressiveOption)
            {
                if (this.stamina > this.MinimumStaminaForFlee)
                {
                    defensiveOption = true;
                }
            }

            bool updateTargetAI = false;
            bool isPreyInAttackRange = aggressiveOption && Vector3.Distance(position, this.primaryPrey.transform.position) < this.CalcAttackRange(this.primaryPrey);
            bool isPursuerInAttackRange = defensiveOption && Vector3.Distance(position, this.primaryPursuer.transform.position) < this.CalcAttackRange(this.primaryPursuer);
            if (isPursuerInAttackRange)
            {
                this.attackTarget = this.primaryPursuer;
            }
            else if (isPreyInAttackRange)
            {
                this.attackTarget = this.primaryPrey;
                if (!this.attackTarget.attackTarget == this)
                {
                    updateTargetAI = true;
                }
            }
            else
            {
                this.attackTarget = null;
            }

            bool shouldAttack = this.attackingStates.Length > 0 && (isPreyInAttackRange || isPursuerInAttackRange);

            if (shouldAttack)
            {
                this.SetState(WanderState.Attack);
            }
            else if (aggressiveOption)
            {
                this.SetState(WanderState.Chase);
            }
            else if (defensiveOption)
            {
                this.SetState(WanderState.Evade);
            }
            else if (this.CurrentState != WanderState.Idle && this.CurrentState != WanderState.Wander)
            {
                this.SetState(WanderState.Idle);
            }

            if (shouldAttack && updateTargetAI)
            {
                this.attackTarget.forceUpdate = true;
            }
        }

        private bool IsValidLocation(Vector3 targetPosition)
        {
            if (!this.constainedToWanderZone)
            {
                return true;
            }

            float distanceFromWander = Vector3.Distance(this.startPosition, targetPosition);
            bool isInWander = distanceFromWander < this.wanderZone;
            return isInWander;
        }

        private float CalcAttackRange(Common_WanderScript other)
        {
            float thisRange = this.navMeshAgent ? this.navMeshAgent.radius : this.characterController.radius;
            float thatRange = other.navMeshAgent ? other.navMeshAgent.radius : other.characterController.radius;
            return this.attackReach + thisRange + thatRange;
        }

        private void SetState(WanderState state)
        {
            WanderState previousState = this.CurrentState;
            if (previousState == WanderState.Dead)
            {
                Debug.LogError("Attempting to set a state to a dead animal.");
                return;
            }
            //if (state != previousState)
            {
                this.CurrentState = state;
                switch (this.CurrentState)
                {
                    case WanderState.Idle:
                        this.HandleBeginIdle();
                        break;
                    case WanderState.Chase:
                        this.HandleBeginChase();
                        break;
                    case WanderState.Evade:
                        this.HandleBeginEvade();
                        break;
                    case WanderState.Attack:
                        this.HandleBeginAttack();
                        break;
                    case WanderState.Dead:
                        this.HandleBeginDeath();
                        break;
                    case WanderState.Wander:
                        this.HandleBeginWander();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void ClearAnimatorBools()
        {
            foreach (IdleState item in this.idleStates)
            {
                this.TrySetBool(item.animationBool, false);
            }

            foreach (MovementState item in this.movementStates)
            {
                this.TrySetBool(item.animationBool, false);
            }

            foreach (AIState item in this.attackingStates)
            {
                this.TrySetBool(item.animationBool, false);
            }

            foreach (AIState item in this.deathStates)
            {
                this.TrySetBool(item.animationBool, false);
            }
        }

        private void TrySetBool(string parameterName, bool value)
        {
            if (!string.IsNullOrEmpty(parameterName))
            {
                if (this.logChanges || this.animatorParameters.Contains(parameterName))
                {
                    this.animator.SetBool(parameterName, value);
                }
            }
        }

        private void HandleBeginDeath()
        {
            this.ClearAnimatorBools();
            if (this.deathStates.Length > 0)
            {
                this.TrySetBool(this.deathStates[Random.Range(0, this.deathStates.Length)].animationBool, true);
            }

            this.deathEvent.Invoke();
            if (this.navMeshAgent && this.navMeshAgent.isOnNavMesh)
            {
                this.navMeshAgent.destination = this.transform.position;
            }

            this.enabled = false;
        }

        private void HandleBeginAttack()
        {
            int attackState = Random.Range(0, this.attackingStates.Length);
            this.turnSpeed = 120f;
            this.ClearAnimatorBools();
            this.TrySetBool(this.attackingStates[attackState].animationBool, true);
            this.attackingEvent.Invoke();
        }

        private void HandleBeginEvade()
        {
            this.SetMoveFast();
            this.movementEvent.Invoke();
        }

        private void HandleBeginChase()
        {
            this.SetMoveFast();
            this.movementEvent.Invoke();
        }

        private void SetMoveFast()
        {
            MovementState moveState = null;
            float maxSpeed = 0f;
            foreach (MovementState state in this.movementStates)
            {
                float stateSpeed = state.moveSpeed;
                if (stateSpeed > maxSpeed)
                {
                    moveState = state;
                    maxSpeed = stateSpeed;
                }
            }

            UnityEngine.Assertions.Assert.IsNotNull(moveState, string.Format("{0}'s wander script does not have any movement states.", this.gameObject.name));
            this.turnSpeed = moveState.turnSpeed;
            this.moveSpeed = maxSpeed;
            this.ClearAnimatorBools();
            this.TrySetBool(moveState.animationBool, true);
        }

        private void SetMoveSlow()
        {
            MovementState moveState = null;
            float minSpeed = float.MaxValue;
            foreach (MovementState state in this.movementStates)
            {
                float stateSpeed = state.moveSpeed;
                if (stateSpeed < minSpeed)
                {
                    moveState = state;
                    minSpeed = stateSpeed;
                }
            }

            UnityEngine.Assertions.Assert.IsNotNull(moveState, string.Format("{0}'s wander script does not have any movement states.", this.gameObject.name));
            this.turnSpeed = moveState.turnSpeed;
            this.moveSpeed = minSpeed;
            this.ClearAnimatorBools();
            this.TrySetBool(moveState.animationBool, true);
        }

        private void HandleBeginIdle()
        {
            this.primaryPrey = null;
            int targetWeight = Random.Range(0, this.totalIdleStateWeight);
            int curWeight = 0;
            foreach (IdleState idleState in this.idleStates)
            {
                curWeight += idleState.stateWeight;
                if (targetWeight > curWeight)
                {
                    continue;
                }

                this.idleUpdateTime = Time.time + Random.Range(idleState.minStateTime, idleState.maxStateTime);
                this.ClearAnimatorBools();
                this.TrySetBool(idleState.animationBool, true);
                this.moveSpeed = 0f;
                break;
            }
            this.idleEvent.Invoke();
        }

        private void HandleBeginWander()
        {
            this.primaryPrey = null;
            Vector3 rand = Random.insideUnitSphere * this.wanderZone;
            Vector3 targetPos = this.startPosition + rand;
            this.ValidatePosition(ref targetPos);

            this.wanderTarget = targetPos;
            this.SetMoveSlow();
        }

        private void ValidatePosition(ref Vector3 targetPos)
        {
            if (this.navMeshAgent)
            {
                NavMeshHit hit;
                if (!NavMesh.SamplePosition(targetPos, out hit, Mathf.Infinity, 1 << NavMesh.GetAreaFromName("Walkable")))
                {
                    Debug.LogError("Unable to sample nav mesh. Please ensure there's a Nav Mesh layer with the name Walkable");
                    this.enabled = false;
                    return;
                }

                targetPos = hit.position;
            }
        }

        private IEnumerator RandomStartingDelay()
        {
            yield return new WaitForSeconds(Random.Range(0f, 2f));
            this.started = true;
            this.StartCoroutine(this.ConstantTicking(Random.Range(.7f, 1f)));
        }

        private IEnumerator ConstantTicking(float delay)
        {
            while (true)
            {
                this.UpdateAI();
                yield return new WaitForSeconds(delay);
            }
            // ReSharper disable once IteratorNeverReturns
        }

        [ContextMenu("This will delete any states you have set, and replace them with the default ones, you can't undo!")]
        public void BasicWanderSetUp()
        {
            PolyPerfect.MovementState walking = new MovementState(), running = new MovementState();
            PolyPerfect.IdleState idle = new IdleState();
            PolyPerfect.AIState attacking = new AIState(), death = new AIState();

            walking.stateName = "Walking";
            walking.animationBool = "isWalking";
            running.stateName = "Running";
            running.animationBool = "isRunning";
            this.movementStates = new MovementState[2];
            this.movementStates[0] = walking;
            this.movementStates[1] = running;


            idle.stateName = "Idle";
            idle.animationBool = "isIdling";
            this.idleStates = new IdleState[1];
            this.idleStates[0] = idle;

            attacking.stateName = "Attacking";
            attacking.animationBool = "isAttacking";
            this.attackingStates = new AIState[1];
            this.attackingStates[0] = attacking;

            death.stateName = "Dead";
            death.animationBool = "isDead";
            this.deathStates = new AIState[1];
            this.deathStates[0] = death;
        }
    }
}