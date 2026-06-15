using UnityEngine;

[CreateAssetMenu(fileName = "WaveConfigSO", menuName = "New Wave Config")]
public class WaveConfigSO : ScriptableObject
{
    [SerializeField] private Transform _pathPrefab;
    [SerializeField] private float _heroMoveSpeed = 5f;
    [SerializeField] private GameObject[] _heroPrefabs;
    // [SerializeField] private float _timeBetweenEnemySpawns = 1f;
    // [SerializeField] private float _enemySpawnVariance = 0f;
    // [SerializeField] private float _minimumSpawnTime = 0.2f;
    // [SerializeField] private float _timeBetweenWaves = 0f;

    public Transform GetStartingWaypoint()
    {
        return _pathPrefab.GetChild(0);
    }

    public float GetEnemyMoveSpeed()
    {
        return _heroMoveSpeed;
    }

    public Transform[] GetWaypoints()
    {
        Transform[] waypoints = new Transform[_pathPrefab.childCount];
        
        // waypoints[0] = _pathPrefab.GetChild(0);
        // waypoints[1] = _pathPrefab.GetChild(1);

        for (int i = 0; i < _pathPrefab.childCount; i++)
        {
            waypoints[i] = _pathPrefab.GetChild(i);
        }
        return waypoints;
    }

    public int GetHeroCount()
    {
        return _heroPrefabs.Length;
    }

    public GameObject GetHeroPrefab(int index)
    {
        return _heroPrefabs[index];
    }

    // public float GetRandomEnemySpawnTime()
    // {
    //     float spawnTime = Random.Range(
    //         _timeBetweenEnemySpawns - _enemySpawnVariance, 
    //         _timeBetweenEnemySpawns + _enemySpawnVariance
    //         );
    //     spawnTime = Mathf.Clamp(spawnTime, _minimumSpawnTime, float.MaxValue);

    //     return spawnTime;
    // }

    // public float GetTimeBetweenWaves()
    // {
    //     return _timeBetweenWaves;
    // }
}
