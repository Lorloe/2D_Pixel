using System.Collections;
using UnityEngine;

public class HeroAI : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float _detectionRange = 15f;
    [SerializeField] private float _sameFloorTolerance = 1.25f;
    [SerializeField] private LayerMask _visionBlockers;
    [SerializeField] private Transform _playerTarget;

    [Header("Attack")]
    [SerializeField] private float _attackDashSpeed = 18f;
    [SerializeField] private float _attackDuration = 0.45f;
    [SerializeField] private float _attackHitDistance = 1.2f;

    [Header("Hit Reactions")]
    [SerializeField] private LayerMask _damageLayers;
    [SerializeField] private bool _destroyOnHit = true;
    [SerializeField] private float _destroyDelay = 0.05f;

    [Header("Sprites")]
    [SerializeField] private Sprite _idleSprite;
    [SerializeField] private Sprite _attackSprite;

    [Header("Game Over")]
    [SerializeField] private GameManager _gameManager;

    private HeroPathfinding _pathfinding;
    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider2D;
    private Coroutine _attackRoutine;
    private bool _isDead;

    private enum State
    {
        Patrol,
        Attack,
        Dead
    }

    private State _currentState = State.Patrol;

    private void Awake()
    {
        _pathfinding = GetComponent<HeroPathfinding>();
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider2D = GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (_playerTarget == null && PlayerController.Instance != null)
        {
            _playerTarget = PlayerController.Instance.transform;
        }

        if (_gameManager == null)
        {
            _gameManager = UnityEngine.Object.FindFirstObjectByType<GameManager>();
        }

        SetIdleVisual();
    }

    private void Update()
    {
        if (_currentState != State.Patrol)
        {
            return;
        }

        if (CanAttackPlayer())
        {
            StartAttack();
        }
    }

    private void FixedUpdate()
    {
        if (_currentState != State.Attack)
        {
            return;
        }

        if (_playerTarget == null)
        {
            FinishGameOver();
            return;
        }

        Vector2 direction = (_playerTarget.position - transform.position).normalized;
        MoveBy(direction * _attackDashSpeed * Time.fixedDeltaTime);

        if (Vector2.Distance(transform.position, _playerTarget.position) <= _attackHitDistance)
        {
            FinishGameOver();
        }
    }

    private bool CanAttackPlayer()
    {
        if (_playerTarget == null)
        {
            return false;
        }

        Vector2 heroPosition = transform.position;
        Vector2 playerPosition = _playerTarget.position;
        float distance = Vector2.Distance(heroPosition, playerPosition);
        float verticalGap = Mathf.Abs(heroPosition.y - playerPosition.y);

        if (distance > _detectionRange)
        {
            return false;
        }

        if (verticalGap > _sameFloorTolerance)
        {
            return false;
        }

        return HasLineOfSight(heroPosition, playerPosition);
    }

    private bool HasLineOfSight(Vector2 origin, Vector2 target)
    {
        RaycastHit2D hit = Physics2D.Linecast(origin, target, _visionBlockers);
        return hit.collider == null;
    }

    private void StartAttack()
    {
        if (_currentState != State.Patrol)
        {
            return;
        }

        _currentState = State.Attack;
        _pathfinding?.StopMoving();

        if (_spriteRenderer != null && _attackSprite != null)
        {
            _spriteRenderer.sprite = _attackSprite;
        }

        if (_attackRoutine != null)
        {
            StopCoroutine(_attackRoutine);
        }

        _attackRoutine = StartCoroutine(AttackTimeoutRoutine());
    }

    private IEnumerator AttackTimeoutRoutine()
    {
        float elapsed = 0f;

        while (elapsed < _attackDuration && _currentState == State.Attack)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (_currentState == State.Attack)
        {
            FinishGameOver();
        }
    }

    private void FinishGameOver()
    {
        if (_currentState == State.Dead)
        {
            return;
        }

        _currentState = State.Dead;

        if (_attackRoutine != null)
        {
            StopCoroutine(_attackRoutine);
            _attackRoutine = null;
        }

        if (_rb != null)
        {
            _rb.linearVelocity = Vector2.zero;
        }

        if (_rb == null)
        {
            // no-op, transform movement is used instead of physics velocity
        }

        _pathfinding?.StopMoving();

        if (_gameManager != null)
        {
            _gameManager.TriggerGameOver();
        }
        else
        {
            Debug.Log("Game Over");
            Time.timeScale = 0f;
        }
    }

    private void SetIdleVisual()
    {
        if (_spriteRenderer != null && _idleSprite != null)
        {
            _spriteRenderer.sprite = _idleSprite;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryHandleIncomingHit(other.gameObject, other.tag, other.gameObject.layer);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryHandleIncomingHit(collision.gameObject, collision.collider.tag, collision.gameObject.layer);
    }

    private void TryHandleIncomingHit(GameObject otherObject, string otherTag, int otherLayer)
    {
        if (_isDead)
        {
            return;
        }

        bool hitByThrowable = otherTag == "Throwable";
        bool hitByBombBox = otherTag == "BombBox";
        bool hitByDamageLayer = (_damageLayers.value & (1 << otherLayer)) != 0;

        if (hitByThrowable || hitByBombBox || hitByDamageLayer)
        {
            HandleIncomingHit(otherObject);
        }
    }

    private void HandleIncomingHit(GameObject source)
    {
        if (_isDead)
        {
            return;
        }

        _isDead = true;
        _currentState = State.Dead;

        if (_attackRoutine != null)
        {
            StopCoroutine(_attackRoutine);
            _attackRoutine = null;
        }

        _pathfinding?.StopMoving();

        if (_rb != null)
        {
            _rb.linearVelocity = Vector2.zero;
        }

        if (_collider2D != null)
        {
            _collider2D.enabled = false;
        }

        if (_destroyOnHit)
        {
            Destroy(gameObject, _destroyDelay);
        }
    }

    private void MoveBy(Vector2 delta)
    {
        if (_rb != null)
        {
            _rb.MovePosition(_rb.position + delta);
            return;
        }

        transform.position += (Vector3)delta;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _detectionRange);
    }
}
