using UnityEngine;

public class HeroPathfinding : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float _moveSpeed = 3.5f;
    [SerializeField] private float _arriveDistance = 0.12f;
    [SerializeField] private float _horizontalTolerance = 0.12f;

    [Header("Jump")]
    [SerializeField] private float _jumpForce = 12f;
    [SerializeField] private float _jumpTriggerHeight = 0.6f;

    [Header("Climb")]
    [SerializeField] private float _climbSpeed = 3.5f;
    [SerializeField] private float _ladderHorizontalTolerance = 0.35f;
    [SerializeField] private LayerMask _ladderLayer;

    [Header("Ground Check")]
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask _groundLayer;

    [Header("Waypoints")]
    [SerializeField] private Transform[] _waypoints;

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
        CacheWaypointsIfNeeded();
        WarnIfSetupLooksWrong();
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

    private void FollowCurrentWaypoint()
    {
        if (_waypoints == null || _waypoints.Length == 0)
        {
            StopBody();
            return;
        }

        Transform currentWaypoint = _waypoints[_currentWaypointIndex];
        if (currentWaypoint == null)
        {
            _currentWaypointIndex++;
            return;
        }

        Vector2 currentPosition = _rb.position;
        Vector2 targetPosition = currentWaypoint.position;
        Vector2 delta = targetPosition - currentPosition;

        if (Vector2.Distance(currentPosition, targetPosition) <= _arriveDistance)
        {
            _currentWaypointIndex++;
            _lastJumpWaypointIndex = -1;
            ExitClimbMode();
            return;
        }

        bool grounded = IsGrounded();
        bool touchingLadder = IsTouchingLadder();
        bool needsVerticalMove = delta.y > _jumpTriggerHeight;
        bool onSameHorizontalBand = Mathf.Abs(delta.x) <= _ladderHorizontalTolerance;

        if (touchingLadder && onSameHorizontalBand && Mathf.Abs(delta.y) > _arriveDistance)
        {
            EnterClimbMode();
            _rb.linearVelocity = new Vector2(0f, Mathf.Sign(delta.y) * _climbSpeed);
        }
        else
        {
            ExitClimbMode();

            float horizontalDirection = Mathf.Abs(delta.x) <= _horizontalTolerance ? 0f : Mathf.Sign(delta.x);
            float verticalVelocity = grounded ? 0f : _rb.linearVelocity.y;
            bool shouldJump = needsVerticalMove
                && grounded
                && Time.time >= _nextAllowedJumpTime
                && _lastJumpWaypointIndex != _currentWaypointIndex;

            if (shouldJump)
            {
                verticalVelocity = _jumpForce;
                _nextAllowedJumpTime = Time.time + 0.35f;
                _lastJumpWaypointIndex = _currentWaypointIndex;
            }

            _rb.linearVelocity = new Vector2(horizontalDirection * _moveSpeed, verticalVelocity);
        }

        HandleFlip(delta.x);
    }

    private bool IsGrounded()
    {
        if (_collider2D != null && _groundLayer.value != 0)
        {
            return _collider2D.IsTouchingLayers(_groundLayer);
        }

        if (_groundCheck != null && _groundCheck.IsChildOf(transform) && _groundLayer.value != 0)
        {
            return Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);
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
        _rb.linearVelocity = new Vector2(0f, 0f);
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
        _waypoints = new Transform[childCount];

        for (int i = 0; i < childCount; i++)
        {
            _waypoints[i] = transform.GetChild(i);
        }
    }

    public void SetWaypoints(Transform[] waypoints)
    {
        _waypoints = waypoints;
        _currentWaypointIndex = 0;
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

    private void OnDrawGizmosSelected()
    {
        if (_groundCheck == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
    }

    private void WarnIfSetupLooksWrong()
    {
        if (_groundCheck != null && !_groundCheck.IsChildOf(transform))
        {
            Debug.LogWarning($"{name}: _groundCheck is not a child of the hero. Falling back to collider-based grounded check.", this);
        }

        if (_waypoints == null || _waypoints.Length == 0)
        {
            Debug.LogWarning($"{name}: no waypoints assigned. Hero will not move until HeroWayPointSetup fills them.", this);
        }
    }
}
