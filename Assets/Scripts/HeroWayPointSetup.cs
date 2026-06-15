using UnityEngine;

[DisallowMultipleComponent]
public class HeroWayPointSetup : MonoBehaviour
{
    [SerializeField] private HeroPathfinding _pathfinding;
    [SerializeField] private Transform _pathRoot;
    [SerializeField] private bool _autoCollectChildren = true;

    private void Awake()
    {
        ApplyWaypoints();
    }

    private void Start()
    {
        ApplyWaypoints();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            ApplyWaypoints();
        }
    }

    public void ApplyWaypoints()
    {
        if (_pathfinding == null)
        {
            _pathfinding = GetComponent<HeroPathfinding>();
        }

        if (_pathfinding == null)
        {
            return;
        }

        if (_pathRoot == null)
        {
            _pathfinding.SetWaypoints(null);
            return;
        }

        if (!_autoCollectChildren)
        {
            return;
        }

        int childCount = _pathRoot.childCount;
        Transform[] waypoints = new Transform[childCount];

        for (int i = 0; i < childCount; i++)
        {
            waypoints[i] = _pathRoot.GetChild(i);
        }

        _pathfinding.SetWaypoints(waypoints);
    }
}
