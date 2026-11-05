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
        HeroWaypoint[] waypoints = new HeroWaypoint[childCount];

        for (int i = 0; i < childCount; i++)
        {
            Transform child = _pathRoot.GetChild(i);
            waypoints[i] = child.GetComponent<HeroWaypoint>();

            if (waypoints[i] == null)
            {
                Debug.LogWarning($"{name}: Waypoint child '{child.name}' is missing HeroWaypoint component.", this);
            }
        }

        _pathfinding.SetWaypoints(waypoints);
    }
}
