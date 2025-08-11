using UnityEngine;
using UnityEngine.InputSystem;

// Created By: International9
// Original Source Code: https://github.com/id-Software/Quake/blob/master/QW/client/data.c

/// <summary>
/// A <see cref="MonoBehaviour"/> For An FPS Controller Emulating Quake I's Controller.
/// </summary>
public class gameMovement : MonoBehaviour
{
    public static gameMovement Instance { get; private set; }

    #region Globals

    [Tooltip("To Enforce A Singular Static Instance Of gameMovement In Awake.")]
    [SerializeField] private bool OneInstance = true;

    [Tooltip("Option To Hold The Jump Key To Jump Without Having To Manually Press Everytime.")]
    [SerializeField] private bool AutoBhop = true;

    

    [Header("Data:")]
    // Public In Case Another Script Needs To Access It But It Might Be A Bad Choice In The Future.
    public Playermove data; // Should Be Called 'pmove' (Like In The og Source Code) But It Wouldn't Fit Nicely So I Just Kept It As 'data'.

    [Header("References:")]
    [SerializeField] private PlayerInput inp;
    [SerializeField] private BoxCollider myColl;
    [SerializeField] private LayerMask layerColl, layerGround;

    [Header("Options:")]

    [SerializeField]
    private MoveVars movevars = new()
    {
        gravity = -32.5f,
        stopspeed = 1f,
        maxspeed = 35f,
        accelerate = 5f,
        friction = 7f
    };

    [Space]

    [SerializeField] private float forwardSpeed = 10f;
    [SerializeField] private float sideSpeed = 12f;
    [SerializeField] private float rangeToGround = .15f;
    [SerializeField] private float stepHeight = .24f;
    
    [Header("Jump:")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float jumpCooldown = .1f;
    private float nextTimeToJump = 0f;

    private InputAction moveAction, jumpAction;

    // NOTE: Unused.
    // private Vector3 player_mins => -(myColl.size * .5f);

    #endregion











    #region Callbacks

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Debug.LogWarning($"2 Or More Instances Of gameMovement Were Found In Scene, Deleting This Object: '{gameObject.name}'!");
            Destroy(gameObject);
        }

        else if (OneInstance)
            Instance = this;

        if (!inp) inp = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        moveAction = inp.actions["move"];
        jumpAction = inp.actions["jump"];
        moveAction.Enable();
        jumpAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
    }

    // Executes On The First Frame.
    private void Start()
    {
        data.origin = transform.position;
        if (!myColl) myColl = GetComponent<BoxCollider>();

        // Automatic Initializing The Layers In Case They Weren't Beforehand.
        if (layerColl.value == 0)
            layerColl = LayerMask.GetMask(new string[] { "Default" });

        if (layerGround.value == 0)
            layerGround = LayerMask.GetMask(new string[] { "Default" });
    }

    private void FixedUpdate()
    {
        PlayerMove();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, myColl.size);
    }

    #endregion











    #region Functions

    #region Physics

    private void CategorizePosition()
    {
        Vector3 point = data.origin - Vector3.up * rangeToGround;
        Trace traceHit = Traceist.PlayerMove(myColl, data.origin, point, layerGround);

        if (data.velocity.y > 5f || traceHit.hitFraction == 1 || Time.time <= nextTimeToJump)
        {
            data.Grounded = false;
            return;
        }

        // If Slope Is >45 Degrees - It's Too Steep And The Player Isn't data.Grounded!
        data.Grounded = traceHit.hitNormal.y >= .7f;
        if (!data.Grounded) return;

        data.origin = traceHit.hitPoint;
    }


    private void GroundMove()
    {
        if (data.velocity == Vector3.zero) return;

        Vector3 original = data.origin;
        Vector3 originalvel = data.velocity;

        // Adding The Velocity:
        Vector3 dest = data.origin + data.velocity * Time.fixedDeltaTime;
        dest.y = data.origin.y;

        // Checking If We Can Move:
        Trace moveTrace = Traceist.PlayerMove(myColl, data.origin, dest, layerColl);

        // If It Hit Nothing, Congrats - You've Moved The Whole Distance.
        if (moveTrace.hitFraction >= 1)
        {
            data.origin = dest;
            transform.position = dest;

            return;
        }

        FlyMove();

        Vector3 down = data.origin;
        Vector3 downVel = data.velocity;

        data.origin = original;
        data.velocity = originalvel;

        dest = data.origin;
        dest.y += stepHeight;

        moveTrace = Traceist.PlayerMove(myColl, data.origin, dest, layerColl);
        data.origin = moveTrace.hitPoint;

        FlyMove();

        dest = data.origin;
        dest.y -= stepHeight;

        moveTrace = Traceist.PlayerMove(myColl, data.origin, dest, layerColl);

        if (moveTrace.hitFraction < 1 && moveTrace.hitNormal.y < .7f)
        {
            data.origin = down;
            data.velocity = downVel;
        }

        data.origin = moveTrace.hitPoint;
        Vector3 up = data.origin;

        float downdist = Vector3.Distance(down, original);
        float updist = Vector3.Distance(up, original);

        if (downdist > updist)
        {
            data.origin = down;
            data.velocity = downVel;
        }

        else
            data.velocity.y = downVel.y;

        transform.position = data.origin;
    }

    const int MAX_CLIP_PLANES = 5;
    private readonly Vector3[] planes = new Vector3[MAX_CLIP_PLANES];

    private void FlyMove()
    {
        Vector3 original_velocity = data.velocity, primal_velocity = data.velocity, end;

        int numbumps = 4, numplanes = 0;
        int i, j; // For Velocity Clipping Loops Later.

        float time_left = Time.fixedDeltaTime;

        for (int bumpcount = 0; bumpcount < numbumps; bumpcount++)
        {
            end = Helpers.VectorMa(data.origin, time_left, data.velocity);
            Trace TraceHit = Traceist.PlayerMove(myColl, data.origin, end, layerColl);

            if (TraceHit.hitFraction > 0f)
            {
                data.origin = TraceHit.hitPoint;
                numplanes = 0;
            }

            if (TraceHit.hitFraction >= 1)
                break; // Moved The Entire Distance.

            time_left -= time_left * TraceHit.hitFraction;

            if (numplanes >= MAX_CLIP_PLANES)
            {
                // This Shouldn't Really Happen...
                data.velocity = Vector3.zero;
                break;
            }

            planes[numplanes++] = TraceHit.hitNormal;

            for (i = 0; i < numplanes; i++)
            {
                data.velocity = ClipVelocity(original_velocity, planes[i]);
                for (j = 0; j < numplanes; j++)
                {
                    if (j != i)
                    {
                        if (Vector3.Dot(data.velocity, planes[j]) < 0)
                            break;
                    }
                }

                if (j == numplanes)
                    break;
            }

            if (i == numplanes)
            {
                if (numplanes != 2)
                {
                    data.velocity = Vector3.zero;
                    break;
                }

                Vector3 dir = Vector3.Cross(planes[0], planes[1]);
                float d = Vector3.Dot(dir, data.velocity);

                data.velocity = dir * d;
            }

            // Tiny Oscilation Avoidance.
            if (Vector3.Dot(data.velocity, primal_velocity) <= 0f)
            {
                data.velocity = Vector3.zero;
                break;
            }
        }

        transform.position = data.origin;
    }

    private void AirMove()
    {
        Vector2 moveDir = moveAction.ReadValue<Vector2>();

        data.Horizontal = moveDir.x;
        data.Vertical = moveDir.y;

        Vector3 wishvel = forwardSpeed * data.Vertical * transform.forward + sideSpeed * data.Horizontal * transform.right;
        
        // Not Using Helpers.VectorNormalize Here Cuz It Kinda Sucks Ngl..
        float wishspeed = Mathf.Min(movevars.maxspeed, wishvel.magnitude);
        data.WishDir = wishvel.normalized;

        if (data.Grounded)
        {
            Accelerate(ref data.velocity, wishspeed, movevars.accelerate);
            GroundMove();
        }

        else
        {
            AirAccelerate(ref data.velocity, wishspeed, movevars.accelerate);
            data.velocity += movevars.gravity * Time.fixedDeltaTime * Vector3.up;

            FlyMove();
        }

        data.velocityXZ = new(data.velocity.x, 0f, data.velocity.z);
    }

    // The Player's Main Movement Loop.
    private void PlayerMove()
    {
        CategorizePosition();

        if ((AutoBhop ? jumpAction.IsPressed() : jumpAction.WasPressedThisFrame()) && data.Grounded)
            Jump();

        Friction(ref data.velocity, movevars.friction);

        AirMove();

        CategorizePosition();

        transform.position = data.origin;
    }

    #endregion










    #region Publics

    // Public Functions To Be Called Outside Of The Script.
    // These Functions Can Be Turned To Static Ones If There's One Instance Of The Player.

    /// <summary>
    /// A Function To Make The Player Jump.
    /// </summary>
    public void Jump()
    {
        if (Time.time < nextTimeToJump || !data.Grounded) return;

        Instance.nextTimeToJump = Time.time + Instance.jumpCooldown;
        data.velocity.y += Instance.jumpForce;
    }

    const float STOP_EPSILON = .05f;

    /// <summary>
    /// A Function To Clip The Input Velocity Depending On A Normal Vector.
    /// </summary>
    /// <param name="input"> The Incoming Velocity. </param>
    /// <param name="normal"> The Normal Vector. </param>
    /// <param name="overbounce"> Overbounce To Overshoot The Output Velocity. </param>
    /// <returns> The Manipulated Slided Velocity. </returns>
    public Vector3 ClipVelocity(Vector3 input, Vector3 normal, float overbounce = 1f)
    {
        var output = input;
        var backoff = Vector3.Dot(output, normal) * overbounce;

        for (int i = 0; i < 3; i++)
        {
            var change = normal[i] * backoff;
            output[i] = output[i] - change;

            if (output[i] > -STOP_EPSILON && output[i] < STOP_EPSILON) output[i] = 0f;
        }

        float adjust = Vector3.Dot(output, normal);
        if (adjust < 0.0f) output -= normal * adjust;

        return output;
    }

    /// <summary>
    /// A Function To Scale And Manipulate A Velocity Vector Based On An Acceleration
    /// Factor To Emulate Acceleration While On The Ground.
    /// </summary>
    /// <param name="vel"> The Velocity To Be Scaled. </param>
    /// <param name="wishspeed"> The Speed Wished To Accelerate To. </param>
    /// <param name="accel"> The Acceleration Factor. </param>
    public void Accelerate(ref Vector3 vel, float wishspeed, float accel)
    {
        float wishspd = wishspeed;
        float currentspeed = Vector3.Dot(vel, data.WishDir);

        float addspeed = wishspd - currentspeed;
        if (addspeed <= 0) return;

        float accelspeed = accel * Time.fixedDeltaTime * wishspd;
        accelspeed = Mathf.Min(accelspeed, addspeed);

        vel.x += accelspeed * data.WishDir.x;
        vel.z += accelspeed * data.WishDir.z;
    }

    /// <summary>
    /// A Function To Scale And Manipulate A Velocity Vector Based On An Acceleration
    /// Factor To Emulate Acceleration While Airborne.
    /// </summary>
    /// <param name="vel"> The Velocity To Be Scaled. </param>
    /// <param name="wishspeed"> The Speed Wished To Accelerate To. </param>
    /// <param name="accel"> The Acceleration Factor. </param>
    public void AirAccelerate(ref Vector3 vel, float wishspeed, float accel)
    {
        float wishspd = Mathf.Min(wishspeed, 3f); // 30f
        float currentspeed = Vector3.Dot(vel, data.WishDir);

        float addspeed = wishspd - currentspeed;
        if (addspeed <= 0f) return;

        float accelspeed = accel * Time.fixedDeltaTime * wishspd;
        accelspeed = Mathf.Min(accelspeed, addspeed);

        vel.x += accelspeed * data.WishDir.x;
        vel.z += accelspeed * data.WishDir.z;
    }

    /// <summary>
    /// A Function To Scale The Velocity Based On A Friction Amount.
    /// </summary>
    /// <param name="vel"> The Velocity To Be Scaled. </param>
    /// <param name="frictionAmount"> The Friction Amount. </param>
    /// <param name="stopSpeed"> The Threshold To Finish The Scaling. </param>
    public void Friction(ref Vector3 vel, float frictionAmount, float stopThreshold = .1f)
    {
        float speed = vel.magnitude, newspeed, control;
        float drop = 0f, friction = frictionAmount;

        if (speed < stopThreshold)
        {
            vel.x = 0f;
            vel.z = 0f;
            return;
        }

        if (data.Grounded)
        {
            /*(TODO - IMPLEMENT!)
            // Increased Friction Near Falloff:
            
            Vector3 stop, start;
            start = stop = data.origin + vel / speed * .25f;
		    start.z = data.origin.z + player_mins.z;
		    stop.z = start.z - .5f;

            Trace falloffCheck = Traceist.PlayerMove(myColl, start, stop, layerColl);
            if (falloffCheck.hitFraction >= 1) friction *= 2f;
            */

            control = speed < movevars.stopspeed ? movevars.stopspeed : speed;
            drop += control * friction * Time.fixedDeltaTime;
        }

        newspeed = speed - drop;
        if (newspeed < 0f) newspeed = 0;
        newspeed /= speed;

        vel.x *= newspeed;
        vel.z *= newspeed;
    }

    #endregion

    #endregion
}
