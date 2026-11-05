using UnityEngine;

public class HeroPathfinding : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float _moveSpeed = 3.5f;
    [SerializeField] private float _arriveDistance = 0.12f;
    [SerializeField] private float _horizontalTolerance = 0.12f;

    [Header("Jump")]
    [SerializeField] private float _jumpForce = 12f;
    [SerializeField] private float _jumpCooldown = 0.35f;
    [SerializeField] private float _jumpTriggerDistance = 0.45f;

    [Header("Climb")]
    [SerializeField] private float _climbSpeed = 3.5f;
    [SerializeField] private float _ladderHorizontalTolerance = 0.35f;
    [SerializeField] private LayerMask _ladderLayer;

    [Header("Ground Check")]
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask _groundLayer;

    [Header("Waypoints")]
    [SerializeField] private HeroWaypoint[] _waypoints;

    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider2D;
    private float _defaultGravityScale;
    private int _currentWaypointIndex;
    private bool _isMoving = true;
    private bool _isClimbing;
    private float _nextAllowedJumpTime;
    private int _lastJumpWaypointIndex = -1;

    public bool HasFinishedPath => _waypoints == null || _waypoints.Length == 0 || _currentWaypointIndex >= _waypoints.Length;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider2D = GetComponent<Collider2D>();
        _defaultGravityScale = _rb != null ? _rb.gravityScale : 1f;

        ConfigurePhysics();
        CacheWaypointsIfNeeded();
    }

    private void FixedUpdate()
    {
        if (!_isMoving || HasFinishedPath)
        {
            StopBody();
            return;
        }

        FollowCurrentWaypoint();
    }

    private void ConfigurePhysics()
    {
        if (_rb == null)
        {
            return;
        }

        _rb.bodyType = RigidbodyType2D.Dynamic;
        _rb.gravityScale = _defaultGravityScale;
        _rb.linearDamping = 0f;
        _rb.angularDamping = 0f;
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void FollowCurrentWaypoint()
    {
        if (_waypoints == null || _waypoints.Length == 0)
        {
            StopBody();
            return;
        }

        if (_currentWaypointIndex < 0 || _currentWaypointIndex >= _waypoints.Length)
        {
            StopBody();
            return;
        }

        HeroWaypoint currentWaypoint = _waypoints[_currentWaypointIndex];
        if (currentWaypoint == null)
        {
            _currentWaypointIndex++;
            return;
        }

        Vector2 currentPosition = _rb.position;
        Vector2 targetPosition = currentWaypoint.transform.position;
        Vector2 delta = targetPosition - currentPosition;
        HeroWaypoint nextWaypoint = GetWaypoint(_currentWaypointIndex + 1);

        if (Vector2.Distance(currentPosition, targetPosition) <= _arriveDistance)
        {
            _currentWaypointIndex++;
            _lastJumpWaypointIndex = -1;
            ExitClimbMode();
            return;
        }

        switch (currentWaypoint.Type)
        {
            case HeroWaypointType.Climb:
                HandleClimbWaypoint(delta);
                break;

            case HeroWaypointType.Jump:
                HandleJumpWaypoint(delta);
                break;

            case HeroWaypointType.ExitLadder:
                HandleExitLadderWaypoint(delta);
                break;

            default:
                HandleWalkWaypoint(delta, nextWaypoint);
                break;
        }

        HandleFlip(delta.x);
    }

    private void HandleWalkWaypoint(Vector2 delta, HeroWaypoint nextWaypoint)
    {
        ExitClimbMode();

        Vector2 targetPosition = nextWaypoint != null && nextWaypoint.Type == HeroWaypointType.Jump
            ? nextWaypoint.transform.position
            : new Vector2(_rb.position.x + delta.x, _rb.position.y + delta.y);

        Vector2 lookAheadDelta = targetPosition - _rb.position;
        float horizontalVelocity = Mathf.Abs(lookAheadDelta.x) <= _horizontalTolerance ? 0f : Mathf.Sign(lookAheadDelta.x) * _moveSpeed;
        float verticalVelocity = _rb.linearVelocity.y;

        if (nextWaypoint != null
            && nextWaypoint.Type == HeroWaypointType.Jump
            && IsGrounded()
            && Mathf.Abs(lookAheadDelta.x) <= _jumpTriggerDistance
            && Time.time >= _nextAllowedJumpTime
            && _lastJumpWaypointIndex != _currentWaypointIndex + 1)
        {
            verticalVelocity = _jumpForce;
            _nextAllowedJumpTime = Time.time + _jumpCooldown;
            _lastJumpWaypointIndex = _currentWaypointIndex + 1;
        }

        _rb.linearVelocity = new Vector2(horizontalVelocity, verticalVelocity);
    }

    private void HandleJumpWaypoint(Vector2 delta)
    {
        ExitClimbMode();

        bool grounded = IsGrounded();
        bool atJumpPoint = Mathf.Abs(delta.x) <= _horizontalTolerance;
        float horizontalVelocity = Mathf.Abs(delta.x) <= _horizontalTolerance ? 0f : Mathf.Sign(delta.x) * _moveSpeed;
        float verticalVelocity = _rb.linearVelocity.y;

        bool shouldJump = grounded
            && atJumpPoint
            && Time.time >= _nextAllowedJumpTime
            && _lastJumpWaypointIndex != _currentWaypointIndex;

        if (shouldJump)
        {
            verticalVelocity = _jumpForce;
            _nextAllowedJumpTime = Time.time + _jumpCooldown;
            _lastJumpWaypointIndex = _currentWaypointIndex;
        }

        _rb.linearVelocity = new Vector2(horizontalVelocity, verticalVelocity);
    }

    private void HandleClimbWaypoint(Vector2 delta)
    {
        bool touchingLadder = IsTouchingLadder();
        bool alignedWithLadder = Mathf.Abs(delta.x) <= _ladderHorizontalTolerance;

        if (touchingLadder && alignedWithLadder)
        {
            EnterClimbMode();
            _rb.linearVelocity = new Vector2(0f, Mathf.Sign(delta.y) * _climbSpeed);
            return;
        }

        ExitClimbMode();
        float horizontalVelocity = Mathf.Abs(delta.x) <= _horizontalTolerance ? 0f : Mathf.Sign(delta.x) * _moveSpeed;
        _rb.linearVelocity = new Vector2(horizontalVelocity, _rb.linearVelocity.y);
    }

    private void HandleExitLadderWaypoint(Vector2 delta)
    {
        ExitClimbMode();

        float horizontalVelocity = Mathf.Abs(delta.x) <= _horizontalTolerance ? 0f : Mathf.Sign(delta.x) * _moveSpeed;
        _rb.linearVelocity = new Vector2(horizontalVelocity, _rb.linearVelocity.y);
    }

    private bool IsGrounded()
    {
        if (_groundCheck != null && _groundLayer.value != 0)
        {
            return Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);
        }

        if (_collider2D != null && _groundLayer.value != 0)
        {
            return _collider2D.IsTouchingLayers(_groundLayer);
        }

        return Mathf.Abs(_rb.linearVelocity.y) < 0.05f;
    }

    private bool IsTouchingLadder()
    {
        if (_collider2D == null)
        {
            return false;
        }

        return _collider2D.IsTouchingLayers(_ladderLayer);
    }

    private void EnterClimbMode()
    {
        if (_isClimbing)
        {
            return;
        }

        _isClimbing = true;
        _rb.gravityScale = 0f;
        _rb.linearVelocity = Vector2.zero;
    }

    private void ExitClimbMode()
    {
        if (!_isClimbing)
        {
            return;
        }

        _isClimbing = false;
        _rb.gravityScale = _defaultGravityScale;
    }

    private void StopBody()
    {
        ExitClimbMode();

        if (_rb != null)
        {
            _rb.linearVelocity = Vector2.zero;
        }
    }

    private void HandleFlip(float deltaX)
    {
        if (_spriteRenderer == null)
        {
            return;
        }

        if (deltaX < 0f)
        {
            _spriteRenderer.flipX = true;
        }
        else if (deltaX > 0f)
        {
            _spriteRenderer.flipX = false;
        }
    }

    private void CacheWaypointsIfNeeded()
    {
        if (_waypoints != null && _waypoints.Length > 0)
        {
            return;
        }

        int childCount = transform.childCount;
        _waypoints = new HeroWaypoint[childCount];

        for (int i = 0; i < childCount; i++)
        {
            _waypoints[i] = transform.GetChild(i).GetComponent<HeroWaypoint>();
        }
    }

    private HeroWaypoint GetWaypoint(int index)
    {
        if (_waypoints == null || index < 0 || index >= _waypoints.Length)
        {
            return null;
        }

        return _waypoints[index];
    }

    public void SetWaypoints(HeroWaypoint[] waypoints)
    {
        _waypoints = waypoints;
        _currentWaypointIndex = 0;
    }

    public void SetMoveSpeed(float moveSpeed)
    {
        _moveSpeed = moveSpeed;
    }

    public void ResetPath()
    {
        _currentWaypointIndex = 0;
        _isMoving = true;
        _lastJumpWaypointIndex = -1;
        _nextAllowedJumpTime = 0f;
    }

    public void StopMoving()
    {
        _isMoving = false;
        StopBody();
    }

    public void ResumeMoving()
    {
        _isMoving = true;
    }
}
