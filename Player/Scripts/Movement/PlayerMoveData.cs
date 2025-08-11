using UnityEngine;
using System;

/// <summary>
/// Player Movement Data Struct, Used In <see cref="gameMovement"/>.
/// Emulating The 'playermove_t' struct.
/// </summary>
/// 
/// <remarks>
/// Declared Originally In Here: https://github.com/id-Software/Quake/blob/master/QW/client/pmove.h#L49
/// </remarks>

[Serializable]
public struct Playermove
{
    /// <summary>
    /// The Player's Origin/Position.
    /// </summary>
    public Vector3 origin;

    /// <summary>
    /// The Player's Velocity.
    /// </summary>
    public Vector3 velocity;

    /// <summary>
    /// The Player's Velocity But Only On The X And Z Axis. 
    /// </summary>
    public Vector3 velocityXZ;

    /// <summary>
    /// The Player's Intended Walking Direction.
    /// </summary>
    public Vector3 WishDir;

    /// <summary>
    /// Signifies If The Player Is On The Ground Or Not.
    /// </summary>
    /// 
    /// <remarks>
    /// Quake's 'onground' Equivilant However It's Not A Bitmask But A Boolean.
    /// </remarks>
    public bool Grounded
    {
        get => _grounded;

        set
        {
            if (_grounded == value)
                return;

            _grounded = value;

            if (_grounded)
            {
                velocity.y = 0f;
                // print("Grounded!");
            }
        }
    }

    [SerializeField] private bool _grounded;

    public float Horizontal; 
    public float Vertical; 
}
