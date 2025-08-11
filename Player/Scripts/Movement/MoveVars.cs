using System;

/// <summary>
/// Move Variables For The Player, Used In <see cref="gameMovement"/>.
/// Emulating The 'movevars_t' struct.
/// </summary>
/// 
/// <remarks>
/// Declared Originally In Here: https://github.com/id-Software/Quake/blob/master/QW/client/pmove.h#L74
/// </remarks>

[Serializable]
public struct MoveVars
{
    /// <summary>
    /// The Player's Gravity.
    /// </summary>
    public float gravity;

    /// <summary>
    /// The Player's Stop Speed.
    /// </summary>
	public float stopspeed;

    /// <summary>
    /// The Player's Maximal Speed.
    /// </summary>
	public float maxspeed;

    /// <summary>
    /// The Player's Acceleration Factor On The Ground.
    /// </summary>
	public float accelerate;

    /// <summary>
    /// The Player's Acceleration Factor In The Air.
    /// </summary>
	public float airaccelerate;

    /// <summary>
    /// The Player's Friction Factor.
    /// </summary>
    public float friction;
}
