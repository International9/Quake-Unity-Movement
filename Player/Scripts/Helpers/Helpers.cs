using UnityEngine;

/// <summary>
/// A Class For A Bunch Of Random General Helper Functions.
/// </summary>
public static class Helpers
{
    /// <summary>
    /// A Function To Offset The Player's Trace Position To Avoid Intersections (Not Perfect.)
    /// </summary>
    /// <param name="org"> Origin Of Trace. </param>
    /// <param name="dest"> Initially Intended Destination Of Trace. </param>
    /// <param name="hitDistance"> The Distance The Trace Went. </param>
    /// <param name="skin"> Skin Width For Fixing Up Small Errors. </param>
    /// <returns> The Point Of The Trace But Offseted To Fit The Player. </returns>
    public static Vector3 GetOffsetSpawnPoint(Vector3 org, Vector3 dest, float hitDistance, float skin = .05f)
    {
        float safeDist = Mathf.Max(0f, hitDistance - skin);
        Vector3 dir = (dest - org).normalized;

        return org + dir * safeDist;
    }

    /// <summary>
    /// A Quake Function Declared In mathlib.c.
    /// </summary>
    /// 
    /// <remarks>
    /// https://github.com/id-Software/Quake/blob/master/QW/client/mathlib.c#L327
    /// </remarks>
    /// 
    /// <param name="veca"></param>
    /// <param name="scale"></param>
    /// <param name="vecb"></param>
    /// <returns></returns>
    public static Vector3 VectorMa(Vector3 veca, float scale, Vector3 vecb) => veca + scale * vecb;

    /// <summary>
    /// A Quake Function For Normalizing A Vector, Declared In mathlib.c.
    /// </summary>
    /// 
    /// <remarks>
    /// https://github.com/id-Software/Quake/blob/master/QW/client/mathlib.c#L327
    /// </remarks>
    /// 
    /// <param name="v"> The Vector Which Is Normalized </param>
    /// <returns> The Length Of The Vector Before Normalization. (For Some Reason, And Yes This Was An Intentional Decision) </returns>
    public static float VectorNormalize(ref Vector3 v)
    {
        float length = v.magnitude;
        v.Normalize();

        return length;
    }
}

