using System;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState { Idle, Patrol, Chase, Jump, Attack, Damage }

public class Enemy : MonoBehaviour {
    public static event Action<EnemyState> OnEnemyStateChanged;

    private NavMeshAgent agent;
    private Animator animator;
    private EnemiesManager enemiesManager;

    [Header("Enemy")]
    [SerializeField] GameObject enemyBody;
    [SerializeField] Face enemyFace;
    [SerializeField] Material enemyFaceMaterial;
    public EnemyState enemyState;
    public Vector3 enemyOriginPosition;

    [Header("Patrol")]
    [SerializeField] LayerMask playerLayer;
    [SerializeField] float patrolRange = 6f;
    [SerializeField] float minDistanceToPatrol = 2f;
    [SerializeField] float groundDetectionDistance = 4f;
    [SerializeField] float minPatrolsTime = 10f;
    [SerializeField] float maxPatrolsTime = 20f;
    private Vector3 walkPoint;
    private float timeBetweenPatrols;
    private float patrolTimer = 0f;
    private bool playerInSightRange, playerInAttackRange;

    [Header("Chase")]
    [SerializeField] float chaseRange = 4f;
    [SerializeField] float speedBoost = 1.5f;

    [Header("Attack")]
    [SerializeField] float attackRange = 1.2f;
    [SerializeField] float timeBetweenAttacks = 3f;
    private bool slimeCanAttack = true;
    private bool slimeAttacking = false;
    public bool slimeStunning = false;
    private float startAttackTimer;

    private void Awake() {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        enemiesManager = GetComponent<EnemiesManager>();
        enemyFaceMaterial = enemyBody.GetComponent<Renderer>().materials[1];
    }

    private void Start() {
        enemyOriginPosition = transform.position;
        RandomTimeBetweenPatrols();
    }

    void Update() {
        CheckPlayerDistanceAndUpdateState();
    }

    private void CheckPlayerDistanceAndUpdateState() {
        playerInSightRange = Physics.CheckSphere(transform.position, chaseRange, playerLayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, playerLayer);


        if (!playerInSightRange && !playerInAttackRange) {
            UpdateEnemyState(EnemyState.Patrol);
        }

        if (playerInSightRange && !playerInAttackRange) {
            UpdateEnemyState(EnemyState.Chase);
        }

        if (playerInSightRange && playerInAttackRange) {
            UpdateEnemyState(EnemyState.Attack);
        }

    }

    public void UpdateEnemyState(EnemyState newState) {
        enemyState = newState;

        switch (enemyState) {
            case EnemyState.Patrol:
                Patrol();
                break;

            case EnemyState.Chase:
                Chase();
                break;

            case EnemyState.Jump:
                Jump();
                break;

            case EnemyState.Attack:
                Attack();
                break;

            case EnemyState.Damage:
                Damage();
                break;
        }

        OnEnemyStateChanged?.Invoke(newState);
    }

    private void Patrol() {
        if (slimeStunning) return;

        animator.SetFloat("Speed", agent.velocity.magnitude);

        if (!agent.pathPending) {
            if (agent.remainingDistance <= agent.stoppingDistance) {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f) {
                    patrolTimer += Time.deltaTime;

                    if (patrolTimer >= timeBetweenPatrols) {
                        patrolTimer = 0f;

                        RandomTimeBetweenPatrols();

                        Vector3 randomSpherePosition = UnityEngine.Random.insideUnitSphere * patrolRange;
                        Vector3 randomPatrolPosition = new(enemyOriginPosition.x + randomSpherePosition.x, transform.position.y + 2f, enemyOriginPosition.z + randomSpherePosition.z);

                        bool isOnNavMesh = NavMesh.SamplePosition(randomPatrolPosition, out NavMeshHit hit, groundDetectionDistance, NavMesh.AllAreas);
                        bool isFarEnough = Vector3.Distance(transform.position, hit.position) >= minDistanceToPatrol;

                        if (isOnNavMesh && isFarEnough) {
                            walkPoint = hit.position;

                            ResumeAgent();

                            SetFace(enemyFace.WalkFace);

                            agent.SetDestination(walkPoint);
                        }

                    } else {
                        StopAgent();
                    }

                }
            }
        }
    }

    private void RandomTimeBetweenPatrols() {
        timeBetweenPatrols = UnityEngine.Random.Range(minPatrolsTime, maxPatrolsTime);
    }

    private void Chase() {
        if (slimeStunning) return;

        startAttackTimer = 0;

        animator.SetFloat("Speed", agent.velocity.magnitude * speedBoost);

        ResumeAgent();

        agent.SetDestination(Player.instance.transform.position);

        SetFace(enemyFace.WalkFace);
    }

    private void Attack() {
        if (slimeStunning) return;

        startAttackTimer += Time.deltaTime;

        FaceTarget();

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack")) return;

        if (!slimeAttacking && slimeCanAttack && startAttackTimer > 1f) {
            startAttackTimer = 0f;

            slimeAttacking = true;

            StopAgent();

            SetFace(enemyFace.attackFace);

            animator.SetTrigger("Attack");

        }
    }

    private void Damage() {
        startAttackTimer = 0f;

        StopAgent();

        SetFace(enemyFace.damageFace);

        animator.SetTrigger("Damage");

        AudioManager.instance.PlayRandomSFX("Slime Hit");

        transform.Translate(-transform.forward * 0.1f, Space.World);
    }


    private void Jump() {
        if (slimeStunning) return;

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Jump")) return;

        StopAgent();

        SetFace(enemyFace.jumpFace);

        animator.SetTrigger("Jump");
    }



    private void SetFace(Texture tex) {
        enemyFaceMaterial.SetTexture("_MainTex", tex);
    }

    private void StopAgent() {
        agent.isStopped = true;
        agent.updateRotation = false;
        animator.SetFloat("Speed", 0);
    }

    private void ResumeAgent() {
        agent.isStopped = false;
        agent.updateRotation = true;
    }

    private void FaceTarget() {
        Vector3 direction = (Player.instance.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    public void AlertObservers(string message) {
        if (message.Equals("Attack")) {
            enemiesManager.DealDamage();
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }

        if (message.Equals("AnimationDamageStarted")) {
            slimeStunning = true;
            slimeCanAttack = false;
            CancelInvoke(nameof(ResetAttackAbility));
        }

        if (message.Equals("AnimationDamageEnded")) {
            slimeStunning = false;
            Invoke(nameof(ResetAttackAbility), 0.3f);
        }
    }

    private void ResetAttackAbility() {
        slimeCanAttack = true;
    }

    private void ResetAttack() {
        slimeAttacking = false;
    }


    private void OnAnimatorMove() {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack")) {
            return;
        }

        Vector3 position = animator.rootPosition;
        position.y = agent.nextPosition.y;
        transform.position = position;
        agent.nextPosition = transform.position;
    }
}
