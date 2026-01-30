using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class NPCMovement : MonoBehaviour
{
    public enum NpcState { Moving, Stationary }

    [Header("Identity")]
    public NPCType.Type npcType = NPCType.Type.Guest;

    [Header("State Control")]
    public bool forceStationary = false;
    public bool forceMoving = false;
    public NpcState state = NpcState.Moving;

    [Header("Timing")]
    public float stationaryCheckInterval = 2.0f;

    [Range(0, 100)] public int statToMov = 60;
    [Range(0, 100)] public int movToStat = 10;
    [Range(0, 100)] public int changingRoom = 25;

    [Header("Movement")]
    public float moveSpeed = 2.0f;
    public float arriveDistance = 0.1f;

    [Header("Waypoints (drag empty objects with NPCWaypoint)")]
    public List<WayPoint> allWaypoints = new();

    [Header("Navigation")]
    public float snapToNavMeshRadius = 2.0f;

    [Header("Runtime & Debug (read-only-ish)")]
    [SerializeField] private string currentRoomId;
    [SerializeField] private WayPoint currentTarget;
    [SerializeField] public bool debugLogs = true;

    private Rigidbody2D rb;
    private float nextStationaryRollTime;
    private NavMeshAgent agent;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        // Start room: find closest waypoint you are allowed to use
        var startWp = GetClosestAllowedWaypoint(transform.position);
        currentRoomId = startWp != null ? startWp.roomId : "";
    }

    private void Start()
    {
        if (debugLogs) Debug.Log($"NavMesh triangulation verts: {NavMesh.CalculateTriangulation().vertices.Length}");
        ApplyForcesToState();

        if (!TrySnapAgentToNavMesh()) return;

        PickNextTarget();
        nextStationaryRollTime = Time.time + stationaryCheckInterval;
    }

    private void Update()
    {
        ApplyForcedStateIfAny();

        if (state == NpcState.Stationary)
        {
            agent.ResetPath();

            if (Time.time >= nextStationaryRollTime && !forceStationary)
            {
                nextStationaryRollTime = Time.time + stationaryCheckInterval;

                if (Roll(statToMov))
                {
                    state = NpcState.Moving;
                    if (debugLogs) Debug.Log($"{name} switching to Moving");
                    PickNextTarget();
                }
            }

            return;
        }

        // Moving
        if (currentTarget == null)
        {
            PickNextTarget();
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= arriveDistance)
        {
            agent.ResetPath();

            if (!forceMoving && Roll(movToStat))
            {
                state = NpcState.Stationary;
                if (debugLogs) Debug.Log($"{name} switching to Stationary");
                nextStationaryRollTime = Time.time + stationaryCheckInterval;
                return;
            }

            // Decide next target (maybe other room)
            PickNextTarget();
            return;
        }
    }

    private void ApplyForcedStateIfAny()
    {
        if (forceStationary && forceMoving)
        {
            // If both are true, stationary wins (safer)
            forceMoving = false;
        }

        if (forceStationary) state = NpcState.Stationary;
        else if (forceMoving) state = NpcState.Moving;
    }

    private void ApplyForcesToState()
    {
        if (state == NpcState.Stationary) 
        {
            agent.ResetPath();
        }
    }

    private void PickNextTarget()
    {
        if (allWaypoints == null || allWaypoints.Count == 0) return;

        bool changeRoom = Roll(changingRoom);

        // Allowed waypoints for this NPC type
        var allowed = allWaypoints.Where(w => w != null && w.Allows(npcType)).ToList();
        if (allowed.Count == 0) return;

        List<WayPoint> candidates;

        if (!changeRoom && !string.IsNullOrEmpty(currentRoomId))
        {
            candidates = allowed.Where(w => w.roomId == currentRoomId).ToList();

            // If none in current room, pick from all allowed
            if (candidates.Count == 0)
            {
                candidates = allowed;
                if (debugLogs) Debug.Log($"{name} wanted to stay in room {currentRoomId} but no allowed waypoints there. Picking from all allowed.");
            }
        }
        else
        {
            // pick from other rooms if possible
            if (!string.IsNullOrEmpty(currentRoomId))
            {
                var otherRooms = allowed.Where(w => w.roomId != currentRoomId).ToList();
                candidates = otherRooms.Count > 0 ? otherRooms : allowed;
            }
            else candidates = allowed;
        }

        if (currentTarget != null) // Exclude current target to avoid picking the same one again
        {
            candidates = candidates.Where(w => w != currentTarget).ToList();
            if (candidates.Count == 0)
            {
                candidates = allowed; // If no other candidates, allow picking the same one
            }
        }
        currentTarget = candidates[Random.Range(0, candidates.Count)];
        currentRoomId = currentTarget.roomId;
        agent.speed = moveSpeed;

        if (!agent.isOnNavMesh)
        {
            if (debugLogs) Debug.LogWarning($"{name} is not on NavMesh -> trying snap before SetDestination");
            if (!TrySnapAgentToNavMesh()) return;
        }

        agent.SetDestination(currentTarget.transform.position);
        if (debugLogs) Debug.Log($"{name} picked new target at {currentTarget.transform.position} in room {currentRoomId}");
    }

    private WayPoint GetClosestAllowedWaypoint(Vector2 pos)
    {
        WayPoint best = null;
        float bestDist = float.MaxValue;

        foreach (var wp in allWaypoints)
        {
            if (wp == null || !wp.Allows(npcType)) continue;
            float d = Vector2.Distance(pos, wp.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = wp;
            }
        }
        if (debugLogs) Debug.Log($"{name} starting in room {best?.roomId ?? "none"}");
        return best;
    }

    private bool Roll(int percent)
    {
        if (percent <= 0) return false;
        if (percent >= 100) return true;
        return Random.Range(0, 100) < percent;
    }

    private bool TrySnapAgentToNavMesh()
    {
        if (NavMesh.SamplePosition(transform.position, out var hit, snapToNavMeshRadius, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            if (debugLogs) Debug.Log($"{name} snapped to NavMesh at {hit.position}");
            return true;
        }

        if (debugLogs) Debug.LogWarning($"{name} could NOT snap to NavMesh (radius={snapToNavMeshRadius}). Position: {transform.position}");
        return false;
    }
}
