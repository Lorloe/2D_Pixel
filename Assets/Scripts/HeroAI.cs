using UnityEngine;

public class HeroAI : MonoBehaviour
{
    [SerializeField] private float _detectionRange = 15f;
    [SerializeField] private float _attackRange = 15f;
    [SerializeField] private bool _stopMovingWhileAttack = false;

    private bool _canAttack = true;

    private enum State 
    {
        Patrolling,
        Attacking
    }

    private State _currentState = State.Patrolling;
    private HeroPathfinding _pathfinding;

    private void Awake()
    {
        _pathfinding = GetComponent<HeroPathfinding>();
    }

    private void Update()
    {
        MovementStateControl();
    }
    
    private void MovementStateControl()
    {
        switch (_currentState)
        {
            case State.Patrolling:
                Patroling();
                break;
            case State.Attacking:
                Attacking();
                break;
        }
    }

    private void Patroling()
    {
        
    }

    private void Attacking()
    {
        if (_attackRange != 0 && _canAttack)
        {
            _canAttack = false;
        }
    }
}
