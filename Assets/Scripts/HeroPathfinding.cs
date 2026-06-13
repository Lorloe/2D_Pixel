using UnityEngine;

public class HeroPathfinding : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 15f;

    private Vector2 _moveDirection;
    private Transform[] _heroPoints;
    private int _currentPointIndex = 0;

    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        FollowHeroPoints();
    }

    private void FixedUpdate()
    {
        if (_moveDirection.x < 0) 
        {
            _spriteRenderer.flipX = true;
        } 
        else if (_moveDirection.x > 0) 
        {
            _spriteRenderer.flipX = false;
        }
    }

    public void MoveTowards(Vector2 targetPosition)
    {
        _moveDirection = (targetPosition - (Vector2)transform.position).normalized;
        _rb.linearVelocity = _moveDirection * _moveSpeed;
    }

    public Transform[] GetHeroPoints()
    {
        _heroPoints = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            _heroPoints[i] = transform.GetChild(i);
        }
        return _heroPoints;
    }

    private void FollowHeroPoints()
    {
        // if (_heroPoints == null || _heroPoints.Length == 0) return;

        // Transform closestPoint = null;
        // float closestDistance = Mathf.Infinity;

        // foreach (Transform point in _heroPoints)
        // {
        //     float distance = Vector2.Distance(transform.position, point.position);
        //     if (distance < closestDistance)
        //     {
        //         closestDistance = distance;
        //         closestPoint = point;
        //     }
        // }

        // if (closestPoint != null)
        // {
        //     MoveTowards(closestPoint.position);
        // }

        if (_currentPointIndex < _heroPoints.Length)
        {
            Vector2 targetPosition = _heroPoints[_currentPointIndex].position;
            float moveDelta = _moveSpeed * Time.deltaTime;

            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveDelta);
            if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
            {
                _currentPointIndex++;
            }
            else
            {
                MoveTowards(targetPosition);
            }
        }
    }
}
