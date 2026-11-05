using UnityEngine;

public enum HeroWaypointType
{
    Walk,
    Jump,
    Climb,
    ExitLadder
}

[DisallowMultipleComponent]
public class HeroWaypoint : MonoBehaviour
{
    [SerializeField] private HeroWaypointType _type = HeroWaypointType.Walk;

    public HeroWaypointType Type => _type;
}
