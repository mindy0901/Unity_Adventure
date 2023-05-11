using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {

    public static PlayerController instance;

    private PlayerInput playerInput;

    [Header("Movement and Look")]
    public Vector2 move;
    public Vector2 look;

    [Header("State")]
    public bool isAttack;
    public bool isAttacking;

    public bool isSprint;
    public bool isJump;
    public bool isFreeFall;
    public bool isGrounded;

    [Header("Attack Settings")]
    public int comboCount = 1;
    public int comboLength = 3;
    public float lastBattleTime;

    [Header("Movement Settings")]
    public bool analogMovement;

    [Header("Mouse Cursor Settings")]
    public bool cursorLocked = true;

    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);

        } else {
            Destroy(gameObject);
        }

        playerInput = new PlayerInput();
    }

    private void OnEnable() {
        playerInput.Player.Enable();

        playerInput.Player.Move.performed += OnMovePerformed;
        playerInput.Player.Move.canceled += OnMoveCanceled;

        playerInput.Player.Sprint.performed += OnSprintPerformed;
        playerInput.Player.Sprint.canceled += OnSprintCanceled;

        playerInput.Player.Look.performed += OnLookPerformed;
        playerInput.Player.Look.canceled += OnLookCanceled;

        playerInput.Player.Jump.performed += OnJumpPerformed;
        playerInput.Player.Jump.canceled += OnJumpCanceled;

        playerInput.Player.Attack.performed += OnAttackPerformed;
        playerInput.Player.Attack.canceled += OnAttackCanceled;
    }

    private void OnDisable() {
        playerInput.Player.Disable();

        playerInput.Player.Move.performed -= OnMovePerformed;
        playerInput.Player.Move.canceled -= OnMoveCanceled;


        playerInput.Player.Sprint.performed -= OnSprintPerformed;
        playerInput.Player.Sprint.canceled -= OnSprintCanceled;

        playerInput.Player.Look.performed -= OnLookPerformed;
        playerInput.Player.Look.canceled -= OnLookCanceled;

        playerInput.Player.Jump.performed -= OnJumpPerformed;
        playerInput.Player.Jump.canceled -= OnJumpCanceled;

        playerInput.Player.Attack.performed -= OnAttackPerformed;
        playerInput.Player.Attack.canceled -= OnAttackCanceled;
    }

    // MOVE
    public void OnMovePerformed(InputAction.CallbackContext ctx) {
        move = ctx.ReadValue<Vector2>();
    }

    public void OnMoveCanceled(InputAction.CallbackContext ctx) {
        move = Vector2.zero;
    }

    // LOOK
    public void OnLookPerformed(InputAction.CallbackContext ctx) {
        look = ctx.ReadValue<Vector2>();
    }

    public void OnLookCanceled(InputAction.CallbackContext ctx) {
        look = Vector2.zero;
    }

    // JUMP
    public void OnJumpPerformed(InputAction.CallbackContext ctx) {
        isJump = true;
    }

    public void OnJumpCanceled(InputAction.CallbackContext ctx) {
        isJump = false;
    }

    // SPRINT
    public void OnSprintPerformed(InputAction.CallbackContext ctx) {
        isSprint = true;
    }
    public void OnSprintCanceled(InputAction.CallbackContext ctx) {
        isSprint = false;
    }

    // ATTACK
    public void OnAttackPerformed(InputAction.CallbackContext ctx) {
        isAttack = true;
        lastBattleTime = Time.time;
    }

    public void OnAttackCanceled(InputAction.CallbackContext ctx) {
        isAttack = false;
    }

    // ANIM EVENT
    public void ReadyForNextAttack() {
        isAttacking = false;

        if (comboCount < comboLength) {
            comboCount++;
        }
    }

    public void ResetCombo(string message) {
        if (message.Equals("NormalAttack")) {
            isAttacking = false;
            comboCount = 1;
        }

        if (message.Equals("LastAttack")) {
            Invoke(nameof(ReadyForNextCombo), 0.3f);
        }
    }

    private void ReadyForNextCombo() {
        isAttacking = false;
        comboCount = 1;
    }

    public void Step() {
        AudioManager.instance.PlayRandomSFX("Footstep");
    }

    // ON APP FOCUS
    private void OnApplicationFocus(bool hasFocus) {
        Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
    }
}
