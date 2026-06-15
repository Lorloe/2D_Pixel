using UnityEngine;

public class HeroPathfinding : MonoBehaviour
{
    [Header("Path")]
    [SerializeField] private float _moveSpeed = 3.5f;
    [SerializeField] private float _arriveDistance = 0.12f;
    [SerializeField] private bool _loopPath = false;
    [SerializeField] private bool _setColliderAsTrigger = true;

    [Header("Waypoints")]
    [SerializeField] private Transform[] _waypoints;

    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider2D;
    private int _currentWaypointIndex;
    private bool _isMoving = true;

    public bool HasFinishedPath => _waypoints == null || _waypoints.Length == 0 || (!_loopPath && _currentWaypointIndex >= _waypoints.Length);

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider2D = GetComponent<Collider2D>();

        ConfigurePhysics();
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

    private void ConfigurePhysics()
    {
        if (_rb == null)
        {
            return;
        }

        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.gravityScale = 0f;
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

        if (_loopPath)
        {
            _currentWaypointIndex %= _waypoints.Length;
        }

        if (_currentWaypointIndex < 0 || _currentWaypointIndex >= _waypoints.Length)
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

        Vector2 currentPosition = GetCurrentPosition();
        Vector2 targetPosition = currentWaypoint.position;
        Vector2 nextPosition = Vector2.MoveTowards(currentPosition, targetPosition, _moveSpeed * Time.fixedDeltaTime);

        SetCurrentPosition(nextPosition);
        HandleFlip(targetPosition.x - currentPosition.x);

        if (Vector2.Distance(nextPosition, targetPosition) <= _arriveDistance)
        {
            _currentWaypointIndex++;

            if (_loopPath && _waypoints.Length > 0 && _currentWaypointIndex >= _waypoints.Length)
            {
                _currentWaypointIndex = 0;
            }
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

    public void SetMoveSpeed(float moveSpeed)
    {
        _moveSpeed = moveSpeed;
    }

    public void ResetPath()
    {
        _currentWaypointIndex = 0;
        _isMoving = true;
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

    private void StopBody()
    {
        if (_rb != null)
        {
            _rb.linearVelocity = Vector2.zero;
        }
    }

    private Vector2 GetCurrentPosition()
    {
        if (_rb != null)
        {
            return _rb.position;
        }

        return transform.position;
    }

    private void SetCurrentPosition(Vector2 position)
    {
        if (_rb != null)
        {
            _rb.MovePosition(position);
            return;
        }

        transform.position = position;
    }

    private void WarnIfSetupLooksWrong()
    {
        if (_collider2D != null && _setColliderAsTrigger && !_collider2D.isTrigger)
        {
            _collider2D.isTrigger = true;
        }

        if (_waypoints == null || _waypoints.Length == 0)
        {
            Debug.LogWarning($"{name}: no waypoints assigned. Hero will not move until HeroWayPointSetup fills them.", this);
        }
    }
}
