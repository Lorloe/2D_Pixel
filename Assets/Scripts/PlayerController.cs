using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _jumpForce = 15f;

    [Header("Dash Settings")]
    [SerializeField] private float _dashSpeed = 20f;
    [SerializeField] private float _dashDuration = 0.5f;
    [SerializeField] private float _dashCooldown = 10f;
    [SerializeField] private TrailRenderer _dashTrail;

    [Header("Climb Settings")]
    [SerializeField] private float _climbSpeed = 5f;
    [SerializeField] private LayerMask _ladderLayer;

    [Header("Attack Settings")]
    [SerializeField] private GameObject _throwablePrefab;
    [SerializeField] private Transform _throwableSpawnPoint;

    [Header("Ground Check")]
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask _groundLayer;

    private InputAction _moveAction, _jumpAction, _dashAction, _attackAction, _invisibleAction;

    private AttackController _attackController;
    private InvisibleSkill _invisibleSkill;

    private bool _isGrounded = false;
    private bool _isDashing = false;
    private bool _isClimbing = false;
    private bool _canDash = true;
    private float _moveInput;
    private float _defaultGravityScale;

    private Rigidbody2D _rb;
    private Collider2D _collider2D;

    public void Awake() 
    {
        if (Instance == null) { Instance = this; }
        _moveAction = InputSystem.actions.FindAction("Move");
        _jumpAction = InputSystem.actions.FindAction("Jump");
        _dashAction = InputSystem.actions.FindAction("Dash");
        _attackAction = InputSystem.actions.FindAction("Attack");
        _invisibleAction = InputSystem.actions.FindAction("Invisible");
        _rb = GetComponent<Rigidbody2D>();
        _collider2D = GetComponent<Collider2D>();
        _defaultGravityScale = _rb.gravityScale;
    }

    private void OnEnable() 
    {
        _moveAction.Enable();
        _jumpAction.Enable();
        _dashAction.Enable();
        _attackAction.Enable();
        _invisibleAction.Enable();
    }

    private void OnDisable() 
    {
        _moveAction.Disable();
        _jumpAction.Disable();
        _dashAction.Disable();
        _attackAction.Disable();
        _invisibleAction.Disable();
    }

    private void Update()
    {
        if (_isDashing) return;

        HandleSpriteFlip();
        GroundCheck();
        OnJump();
        OnDash();
        OnAttack();
        OnInvisible();
    }

    private void FixedUpdate()
    {
        if (_isDashing) return;

        OnClimb();

        if (!_isClimbing)
        {
            OnMove();
        }
    }

    #region ACTION

    private void OnMove() 
    {
        _moveInput = _moveAction.ReadValue<Vector2>().x;
        _rb.linearVelocity = new Vector2(_moveInput * _moveSpeed, _rb.linearVelocity.y);
    }

    private void OnJump()
    {
        if (_jumpAction.WasPressedThisFrame() && (_isGrounded || _isClimbing)) 
        {
            // If climbing, release the ladder first
            if (_isClimbing)
            {
                _isClimbing = false;
                _rb.gravityScale = _defaultGravityScale;
            }
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce);
        }
    }

    private void GroundCheck()
    {
        _isGrounded = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);
    }

    #endregion

    #region SPRITE FLIP

    private void HandleSpriteFlip()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        if (mousePosition.x < transform.position.x)
        {
            transform.eulerAngles = new Vector3(0f, -180f, 0f);
        }
        else
        {
            transform.eulerAngles = new Vector3(0f, 0f, 0f);
        }
    }

    public bool IsFacingRight()
    {
        return transform.eulerAngles.y < 90f || transform.eulerAngles.y > 270f;
    }

    #endregion

    #region DASH

    private void OnDash()
    {
        if (_dashAction.WasPressedThisFrame() && _canDash && !_isDashing)
        {
            _dashTrail.emitting = true;
            StartCoroutine(DashRoutine());
        }
    }

    private IEnumerator DashRoutine()
    {
        _isDashing = true;
        _canDash = false;

        // Dash in the facing direction
        float dashDirection = IsFacingRight() ? -1f : 1f;
        _rb.linearVelocity = new Vector2(dashDirection * _dashSpeed, 0f);

        yield return new WaitForSeconds(_dashDuration);

        // End dash — restore normal velocity
        _rb.linearVelocity = new Vector2(_moveInput * _moveSpeed, _rb.linearVelocity.y);
        _isDashing = false;
        _dashTrail.emitting = false;

        // Cooldown
        yield return new WaitForSeconds(_dashCooldown);
        _canDash = true;
    }

    #endregion

    #region CLIMB

    private void OnClimb()
    {
        bool isTouchingLadder = _collider2D.IsTouchingLayers(_ladderLayer);
        Vector2 moveInput = _moveAction.ReadValue<Vector2>();

        // Start climbing: touching ladder and pressing up/down
        if (isTouchingLadder && moveInput.y != 0)
        {
            _isClimbing = true;
            _rb.gravityScale = 0f;
            _rb.linearVelocity = new Vector2(moveInput.x * _moveSpeed, moveInput.y * _climbSpeed);
        }
        // Stop climbing: left the ladder
        else if (_isClimbing && !isTouchingLadder)
        {
            _isClimbing = false;
            _rb.gravityScale = _defaultGravityScale;
        }
        // On ladder but not pressing up/down: hang still
        else if (_isClimbing && isTouchingLadder)
        {
            _rb.linearVelocity = new Vector2(moveInput.x * _moveSpeed, 0f);
        }
    }

    #endregion

    #region ATTACK

    private void OnAttack()
    {
        Debug.Log("Attack triggered!");
    }

    #endregion


    #region INVISIBLE

    private void OnInvisible()
    {
        if (_invisibleAction.WasPressedThisFrame())
        {
            // _invisibleSkill.StartCoroutine(_invisibleSkill.ToggleInvisibility());
            Debug.Log("Invisible triggered!");
        }
    }

    #endregion
}

