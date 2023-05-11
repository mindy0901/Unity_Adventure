using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour {
    public static Player instance;

    private CharacterController controller;
    private Animator animator;
    private PlayerController playerController;
    private GameObject mainCamera;
    private PlayerManager playerManager;
    public Transform respawnPoint;

    [Header("Attack")]
    public Weapon weapon;

    [Header("Runing")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float SpeedChangeRate = 10f;
    [SerializeField] private LayerMask whatIsSlime;
    private float speed;
    private float animationBlend;
    private float targetRotation = 0.0f;
    private float rotationVelocity;
    private float verticalVelocity;
    private float terminalVelocity = 53.0f;

    [Header("Jumping")]
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float gravity = -15.0f;
    [SerializeField] private float groundedOffset = -0.14f;
    [SerializeField] private float groundedRadius = 0.28f;
    [SerializeField] private float jumpTimeout = 0.50f;
    [SerializeField] private float fallTimeout = 0.15f;
    private float jumpTimeoutDelta;
    private float fallTimeoutDelta;
    private bool wasGroundedLastFrame;

    [Header("Cinemachine")]
    [SerializeField] private GameObject CinemachineCameraTarget;
    [SerializeField] private float TopClamp = 70.0f;
    [SerializeField] private float BottomClamp = -30.0f;
    [SerializeField][Range(0f, 0.3f)] private float RotationSmoothTime = 0.12f;
    private const float threshold = 0.01f;
    private float cinemachineTargetYaw;
    private float cinemachineTargetPitch;

    // animation layers index
    private int battleLayerIndex;
    private int comboLayerIndex;

    // animation params ID.
    private int moveSpeedId;
    private int motionSpeedId;
    private int isSprintId;
    private int isJumpId;
    private int isFreefallId;
    private int isGroundedId;
    private int deathId;
    private int takeDamageId;

    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);

        } else {
            Destroy(gameObject);
        }

        if (mainCamera == null) mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
        playerManager = GetComponent<PlayerManager>();
    }

    private void Start() {
        transform.position = respawnPoint.position;

        AssignAnimationIDs();

        cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

        battleLayerIndex = animator.GetLayerIndex("Battle");
        comboLayerIndex = animator.GetLayerIndex("Combo");

        jumpTimeoutDelta = jumpTimeout;
        fallTimeoutDelta = fallTimeout;
    }

    private void Update() {
        if (!PlayerManager.instance.isAlive) return;

        JumpAndGravity();
        GroundedCheck();
        Move();
        Attack();
    }

    private void LateUpdate() {
        CameraRotation();
    }

    private void Attack() {
        if (playerController.isAttack && !playerController.isAttacking) {
            playerController.isAttacking = true;

            animator.SetTrigger("attack_" + playerController.comboCount);

            AudioManager.instance.PlaySFX("Attack");

            controller.Move(0.2f * Time.deltaTime * transform.forward);

            if (playerController.comboCount == 1 && animator.GetLayerWeight(battleLayerIndex) == 0) {
                animator.SetLayerWeight(battleLayerIndex, 1);
            }
        }

        if (Time.time - playerController.lastBattleTime > 10f) {
            animator.SetLayerWeight(battleLayerIndex, 0);
        }
    }


    private void Move() {
        AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(comboLayerIndex);

        animator.SetBool(isSprintId, playerController.isSprint);

        float targetSpeed = playerController.isSprint ? sprintSpeed : moveSpeed;

        // player cant sprint when fighting
        if (!currentAnimatorStateInfo.IsName("Idle_Battle")) targetSpeed = moveSpeed;

        // set speed to 0 when player stop moving
        if (playerController.move == Vector2.zero) targetSpeed = 0;

        // a reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude;

        float speedOffset = 0.1f;

        float inputMagnitude = playerController.analogMovement ? playerController.move.magnitude : 1f;

        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset) {
            // creates curved result rather than a linear one giving a more organic speed change
            // note T in Lerp is clamped, so we don't need to clamp our speed
            speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

            // round speed to 3 decimal places
            speed = Mathf.Round(speed * 1000f) / 1000f;
        } else {
            speed = targetSpeed;
        }
        animationBlend = Mathf.Lerp(animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);

        if (animationBlend < 0.01f) animationBlend = 0f;

        // normalise input direction
        Vector3 inputDirection = new Vector3(playerController.move.x, 0.0f, playerController.move.y).normalized;

        // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is a move input rotate player when the player is moving
        if (playerController.move != Vector2.zero) {
            targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, RotationSmoothTime);

            // rotate to face input direction relative to camera position
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;

        controller.Move(targetDirection.normalized * (speed * Time.deltaTime) + new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);

        animator.SetFloat(moveSpeedId, animationBlend);
        animator.SetFloat(motionSpeedId, inputMagnitude);
    }



    private void CameraRotation() {
        if (playerController.look.sqrMagnitude >= threshold) {
            float deltaTimeMultiplier = 1f;
            //"KeyboardMouse" ? 1.0f : Time.deltaTime;

            cinemachineTargetYaw += playerController.look.x * deltaTimeMultiplier;
            cinemachineTargetPitch += playerController.look.y * deltaTimeMultiplier;
        }

        // Clamp our rotations so our values are limited 360 degrees
        cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
        cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(cinemachineTargetPitch, cinemachineTargetYaw, 0.0f);
    }

    private float ClampAngle(float lfAngle, float lfMin, float lfMax) {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }


    private void JumpAndGravity() {
        if (playerController.isGrounded) {
            fallTimeoutDelta = fallTimeout;

            playerController.isFreeFall = false;

            animator.SetBool(isJumpId, false);
            animator.SetBool(isFreefallId, false);

            // stop our velocity dropping infinitely when grounded
            if (verticalVelocity < 0.0f) {
                verticalVelocity = -2f;
            }

            // Jump
            if (playerController.isJump && jumpTimeoutDelta <= 0.0f) {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

                AudioManager.instance.PlaySFX("Jump");

                animator.SetBool(isJumpId, true);
            }

            // jump timeout
            if (jumpTimeoutDelta >= 0.0f) {
                jumpTimeoutDelta -= Time.deltaTime;


            }

        } else {
            jumpTimeoutDelta = jumpTimeout;

            if (fallTimeoutDelta >= 0.0f) {
                fallTimeoutDelta -= Time.deltaTime;


            } else {
                playerController.isFreeFall = true;
                animator.SetBool(isFreefallId, true);

            }
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (verticalVelocity < terminalVelocity) {
            verticalVelocity += gravity * Time.deltaTime;
        }
    }

    private void GroundedCheck() {
        Vector3 spherePosition = new(transform.position.x, transform.position.y - groundedOffset, transform.position.z);

        playerController.isGrounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);

        if (animator) {
            animator.SetBool(isGroundedId, playerController.isGrounded);
        }

        if (playerController.isGrounded && !wasGroundedLastFrame) {
            // play landing sound effect
            AudioManager.instance.PlaySFX("Landing");
        }

        wasGroundedLastFrame = playerController.isGrounded;
    }

    private void AssignAnimationIDs() {
        moveSpeedId = Animator.StringToHash("moveSpeed");
        motionSpeedId = Animator.StringToHash("motionSpeed");

        isJumpId = Animator.StringToHash("isJump");
        isFreefallId = Animator.StringToHash("isFreeFall");
        isGroundedId = Animator.StringToHash("isGrounded");
        isSprintId = Animator.StringToHash("isSprint");

        deathId = Animator.StringToHash("death");
        takeDamageId = Animator.StringToHash("takeDamage");
    }

    public void EnableColliders() {
        weapon.EnableColliders();
    }

    public void DisableColliders() {
        weapon.DisableColliders();
    }

    public void BeHit() {
        EnterBattleState();

        animator.SetTrigger(takeDamageId);

        PlayerController.instance.lastBattleTime = Time.time;
    }

    private void EnterBattleState() {
        if (animator.GetLayerWeight(battleLayerIndex) == 1) return;
        animator.SetLayerWeight(battleLayerIndex, 1);
    }

    public void Death() {
        animator.SetTrigger(deathId);
    }

    public void ResetPosition() {
        controller.enabled = false;
        transform.position = respawnPoint.position;
        controller.enabled = true;

        animator.SetLayerWeight(battleLayerIndex, 0);
        animator.Play("Idle_Battle", battleLayerIndex, 0f);
    }
}