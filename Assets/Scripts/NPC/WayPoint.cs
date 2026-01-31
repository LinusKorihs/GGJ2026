using UnityEngine;

public class WayPoint : MonoBehaviour
{
    [Header("Room")]
    public string roomId;

    [Header("Allowed NPC Types")]
    public bool guest = true;
    public bool waiter = true;
    public bool vip = false;

    public bool Allows(NPCType.Type type)
    {
        return type switch
        {
            NPCType.Type.Guest => guest,
            NPCType.Type.Staff => waiter,
            NPCType.Type.VIP => vip,
            _ => false
        };
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position, 0.5f);
    }
#endif
}
