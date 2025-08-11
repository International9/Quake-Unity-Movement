using UnityEngine;

/// <summary>
/// A Class For A Bunch Of Random Helper Trace Functions.
/// </summary>
public static class Traceist
{
    #region Trace Box

    /// <summary>
    /// A Function That Traces A Box Into A Destination And Returns Hit Information.
    /// </summary>
    /// 
    /// <param name="origin"> The Start Of The Trace. </param>
    /// <param name="halfExtents"> The Half Extents Of The Collider </param>
    /// <param name="dest"> The Destination Position. </param>
    /// <param name="rotation"> The Rotation Of The Collider. </param>
    /// <param name="layerMask"> The Layermasks The Trace Can Collide With. </param>
    /// <param name="trigInter"> The Trigger Interaction Of The Trace. </param>
    /// <returns> A Trace Struct With All Of The Hit Information </returns>
    public static Trace TraceBox(Vector3 origin, Vector3 halfExtents, Vector3 dest, Quaternion rotation, int layerMask, QueryTriggerInteraction trigInter = QueryTriggerInteraction.Ignore)
    {
        var delta = dest - origin;
        var dist = delta.magnitude;
        var dir = delta.normalized;

        // Default Trace:
        Trace finTrace = Trace.defaultTrace;
        finTrace.hitPoint = dest;

        bool traced = Physics.BoxCast
        (
            origin, halfExtents,
            dir, out var hit, rotation,
            dist, layerMask, trigInter
        );

        if (!traced) return finTrace;

        finTrace = new()
        {
            hitFraction = hit.distance / dist,
            hitPoint = Helpers.GetOffsetSpawnPoint(origin, dest, hit.distance),
            hitNormal = hit.normal,
            hitObject = hit.transform.gameObject
        };

        // Stolen From: https://github.com/Olezen/UnitySourceMovement/blob/master/Modified%20fragsurf/TraceUtil/Tracer.cs#L70
        Ray normalRay = new(hit.point - dir * 0.001f, dir);
        if (hit.collider.Raycast(normalRay, out var normalHit, 0.002f))
            finTrace.hitNormal = normalHit.normal;

        return finTrace;
    }

    #endregion









    #region Trace Functions

    // Misc Trace Functions: 

    /// <summary>
    /// Traces The Player AABB From An Origin To A Destination.
    /// </summary>
    /// 
    /// <remarks>
    /// Emulates The: 'PM_PlayerMove' Function In: 
    /// https://github.com/id-Software/Quake/blob/master/QW/client/pmovetst.c#L350
    /// </remarks>
    /// 
    /// <param name="coll"> The Player's BoxCollider (Acting As It's AABB). </param>
    /// <param name="origin"> The Trace's Starting Position. </param>
    /// <param name="dest"> The Destination Position. </param>
    /// <param name="layerMask"> The LayerMask The Trace Can Hit. </param>
    /// <param name="trigInter"> The Trigger Interaction Of The Trace. </param>
    /// <returns> A Trace Struct With All Of The Hit Information. </returns>
    public static Trace PlayerMove(BoxCollider coll, Vector3 origin, Vector3 dest, int layerMask, QueryTriggerInteraction trigInter = QueryTriggerInteraction.Ignore)
        => TraceBox(origin, coll.size * .5f, dest, Quaternion.identity, layerMask, trigInter);

    /// <summary>
    /// Traces The Player AABB From It's Origin To A Destination.
    /// </summary>
    /// 
    /// <remarks>
    /// Emulates The: 'PM_PlayerMove' Function In: 
    /// https://github.com/id-Software/Quake/blob/master/QW/client/pmovetst.c#L350
    /// </remarks>
    /// 
    /// <param name="coll"> The Player's BoxCollider (Acting As It's AABB). </param>
    /// <param name="dest"> The Destination Position. </param>
    /// <param name="layerMask"> The LayerMask The Trace Can Hit. </param>
    /// <param name="trigInter"> The Trigger Interaction Of The Trace. </param>
    /// <returns> A Trace Struct With All Of The Hit Information. </returns>
    public static Trace PlayerMove(BoxCollider coll, Vector3 dest, int layerMask, QueryTriggerInteraction trigInter = QueryTriggerInteraction.Ignore)
        => TraceBox(coll.transform.position, coll.size * .5f, dest, Quaternion.identity, layerMask, trigInter);
    
    #endregion
}

