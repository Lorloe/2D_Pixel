using UnityEngine;

public class SelfDestroy : MonoBehaviour
{
    private ParticleSystem _particleSystem;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        if (_particleSystem && _particleSystem.IsAlive())
        {
            SelfDestroyAfterEffect();
        }
    }

    public void SelfDestroyAfterEffect()
    {
        if (_particleSystem != null)
        {
            Destroy(gameObject, _particleSystem.main.duration);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
