using UnityEngine;

/// <summary>
/// Struct For Trace Information.
/// </summary>
///
/// <remarks>
/// Emulates The 'pmtrace_t' Struct In:
/// https://github.com/id-Software/Quake/blob/master/QW/client/pmove.h#L27
/// </remarks>
public struct Trace
{
    /// <summary>
    /// The Fraction Of Where The Trace Hit On The Ray.
    /// </summary>
    public float hitFraction;

    /// <summary>
    /// The World Position Of Where The Trace Hit.
    /// </summary>
    public Vector3 hitPoint;

    /// <summary>
    /// The Normal Vector Of The Plane The Trace Hit.
    /// </summary>
    public Vector3 hitNormal;

    /// <summary>
    /// The Object The Trace Hit.
    /// </summary>
    public GameObject hitObject;
}

public static class Traceist
{
    /// <summary>
    /// Traces The BoxCollider From An Origin To A Destination.
    /// </summary>
    /// 
    /// <remarks>
    /// Emulates The: 'PM_PlayerMove' Function In: 
    /// https://github.com/id-Software/Quake/blob/master/QW/client/pmovetst.c#L350
    /// </remarks>
    /// 
    /// <param name="coll"> The Player's BoxCollider. </param>
    /// <param name="dest"> The Destination Position. </param>
    /// <param name="layerMask"> The LayerMask The Trace Can Hit. </param>
    /// <returns> A Trace Struct With All Of The Hit Information. </returns>
    public static Trace TraceBox(BoxCollider coll, Vector3 dest, int layerMask)
    {
        var origin = coll.transform.position;
        var delta = dest - origin;
        var dist = delta.magnitude;
        var dir = delta.normalized;

        bool traced = Physics.BoxCast
        (
            origin, coll.size * 0.5f,
            dir, out var hit, Quaternion.identity,
            dist, layerMask, QueryTriggerInteraction.Ignore
        );

        float fraction = traced ? hit.distance / dist : 1f;
        Vector3 point = traced ? GetOffsetSpawnPoint(coll, dest, hit.distance) : dest;

        Trace tracefin = new()
        {
            hitFraction = fraction,
            hitPoint = point,
            hitNormal = traced ? hit.normal : default,
            hitObject = traced ? hit.transform.gameObject : null
        };

        return tracefin;
    }

    // This Is For The Singular Line: 'VectorAdd (trace.endpos, offset, trace.endpos);' In 'PM_PlayerMove'..
    // Generated Using ChatGPT (Mostly) Cuz I'm Lazy Asf.
    //
    // IMPORTANT: CAN FAIL!
    private static Vector3 GetOffsetSpawnPoint(
        BoxCollider coll,
        Vector3 destCenter,  // where you tried to move the collider center to
        float hitDistance,   // distance from the *same* origin center you BoxCast from
        float skin = .01f)
    {
        Vector3 originCenter = coll.transform.TransformPoint(coll.center);

        Vector3 dir = destCenter - originCenter;
        float len = dir.magnitude;

        if (len < 1e-8f) return coll.transform.position;
        dir /= len;

        float move = Mathf.Max(0f, hitDistance - skin);
        Vector3 newCenter = originCenter + dir * move;

        Vector3 delta = newCenter - originCenter;
        return coll.transform.position + delta;
    }
}

