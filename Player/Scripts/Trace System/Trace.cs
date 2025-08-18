using UnityEngine;

/// <summary>
/// Struct For Trace Information.
/// </summary>
///
/// <remarks>
/// Emulates The 'pmtrace_t' Struct In:
/// https://github.com/id-Software/Quake/blob/master/QW/client/pmove.h#L27
/// </remarks>

// [System.Serializable] <-- Idk If It's Really Needed.
public struct Trace
{   
    /// <summary>
    /// A Bool To Signify If The Trace Has Started While It Was Inside Of A Solid, If So, This Is True. Otherwise - It Is False.
    /// </summary>
    public bool StartSolid;
    
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

    /// <summary>
    /// A Trace Struct With The Values Being As If The Struct Didn't Hit Anything.
    /// </summary>
    public static Trace defaultTrace = new()
    {
        hitFraction = 1f,
        hitPoint = Vector3.zero, // Should Be The Initially Intended Destination Of The Trace.
        hitNormal = Vector3.zero,
        hitObject = null
    };
}


