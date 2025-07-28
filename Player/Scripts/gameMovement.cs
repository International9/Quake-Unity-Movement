using UnityEngine;
using UnityEngine.InputSystem;


// Original Source Code: https://github.com/id-Software/Quake/blob/master/QW/client/pmove.c


/// <summary>
/// A <see cref="MonoBehaviour"/> Emulating Quake I's Movement.
/// </summary>
public class gameMovement : MonoBehaviour
{
    public static gameMovement Instance { get; private set; }

    #region Movement Data

    /// <summary>
    /// The Velocity Of The Player.
    /// </summary>
    public static Vector3 velocity; // { get; set; }

    /// <summary>
    /// The Velocity Of The Player Only On The X And Z Axis.
    /// </summary>
    public static Vector3 velocityXZ; // { get; set; }

    /// <summary>
    /// The Direction The Player Wishes To Go In.
    /// </summary>
    public static Vector3 WishDir { get; private set; }

    /// <summary>
    /// A Bool Signifing If The Player Is On The Ground.
    /// </summary>
    public static bool Grounded
    {
        get => _grounded;

        private set
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

    private static bool _grounded;


    public static float Horizontal { get; private set; }
    public static float Vertical   { get; private set; }

    #endregion










    #region Globals

    [Header("References:")]
    [SerializeField] private PlayerInput inp;
    [SerializeField] private BoxCollider myColl;

    [Header("Options:")]
    [SerializeField] private float forwardSpeed = 10f;
    [SerializeField] private float sideSpeed = 15f;
    [SerializeField] private float maxspeed = 25f, stopspeed = 7f;
    [SerializeField] private float rangeToGround = .2f;
    [SerializeField] private float stepHeight = 18f;

    [Header("Physics:")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float accelerate = 7f;
    [SerializeField] private float friction = 5f;
    [SerializeField] private LayerMask layerColl, layerGround;

    [Header("Jump:")]
    [SerializeField] private float jumpForce = 7.5f;
    [SerializeField] private float jumpCooldown = .1f;
    private float nextTimeToJump = 0f;



    private InputAction moveAction, jumpAction;

    #endregion











    #region Callbacks

    private void Awake()
    {
        if (Instance)
        {
            Debug.LogWarning($"2 Or More Instances Of gameMovement Were Found In Scene, Deleting This Object: '{gameObject.name}'!");
            Destroy(gameObject);
        }

        else
            Instance = this;
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
        if (!inp) inp = GetComponent<PlayerInput>();
    }

    private void Update()
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
        Vector3 point = transform.position - Vector3.up * rangeToGround;
        Trace traceHit = Traceist.TraceBox(myColl, point, layerColl);

        if (velocity.y > 8f || traceHit.hitFraction == 1 || Time.time <= nextTimeToJump) // Shoutout To ID For This Magic Number...
            Grounded = false;
        else
        {
            // If Slope Is >45 Degrees - It's Too Steep And The Player Isn't Grounded!
            Grounded = traceHit.hitNormal.y >= .7f;
            if (!Grounded) return;

            transform.position = traceHit.hitPoint;
        }
    }

    private void GroundMove()
    {
        Vector3 original = transform.position;
        Vector3 originalvel = velocity;

        if (velocity == Vector3.zero) return;

        // Adding The Velocity:
        Vector3 dest = transform.position + velocity * Time.deltaTime;
        dest.y = transform.position.y;

        // Checking If We Can Move:
        Trace moveTrace = Traceist.TraceBox(myColl, dest, layerColl);

        // If It Hit Nothing, Congrats - You've Moved The Whole Distance.
        if (moveTrace.hitFraction >= 1)
        {
            transform.position = dest;
            return;
        }

        FlyMove();

        Vector3 down = transform.position;
        Vector3 downVel = velocity;

        transform.position = original;
        velocity = originalvel;

        dest = transform.position;
        dest.y += stepHeight;

        moveTrace = Traceist.TraceBox(myColl, dest, layerColl);

        transform.position = moveTrace.hitPoint;

        FlyMove();

        dest = transform.position;
        dest.y -= stepHeight;

        moveTrace = Traceist.TraceBox(myColl, dest, layerColl);

        if (moveTrace.hitFraction < 1 && moveTrace.hitNormal.y < .7f)
        {
            transform.position = down;
            velocity = downVel;
        }

        transform.position = moveTrace.hitPoint;

        Vector3 up = transform.position;

        float downdist = Vector3.Distance(down, original);
        float updist   = Vector3.Distance(up, original);

        if (downdist > updist)
        {
            transform.position = down;
            velocity = downVel;
        }
        else velocity.y = downVel.y;
    }

    const int MAX_CLIP_PLANES = 5;
    private readonly Vector3[] planes = new Vector3[MAX_CLIP_PLANES];

    private void FlyMove()
    {
        Vector3 original_velocity = velocity, primal_velocity = velocity, end;

        int numbumps = 4, bumpcount, numplanes = 0;
        int i, j; // For Velocity Clipping Loops Later.

        float time_left = Time.deltaTime;

        for (bumpcount = 0; bumpcount < numbumps; bumpcount++)
        {
            end = transform.position + time_left * velocity;
            Trace TraceHit = Traceist.TraceBox(myColl, end, layerColl);

            if (TraceHit.hitFraction > 0f)
            {
                transform.position = TraceHit.hitPoint;
                numplanes = 0;
            }

            if (TraceHit.hitFraction >= 1)
                break; // Moved The Entire Distance.

            time_left -= time_left * TraceHit.hitFraction;

            if (numplanes >= MAX_CLIP_PLANES)
            {
                // This Shouldn't Really Happen...
                velocity = Vector3.zero;
                break;
            }

            planes[numplanes] = TraceHit.hitNormal;
            numplanes++;

            for (i = 0; i < numplanes; i++)
            {
                velocity = ClipVelocity(original_velocity, planes[i]);
                for (j = 0; j < numplanes; j++)
                {
                    if (j != i)
                    {
                        if (Vector3.Dot(velocity, planes[j]) < 0)
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
                    velocity = Vector3.zero;
                    break;
                }

                Vector3 dir = Vector3.Cross(planes[0], planes[1]);
                float d = Vector3.Dot(dir, velocity);

                velocity = dir * d;
            }

            // Tiny Oscilation Avoidance.
            if (Vector3.Dot(velocity, primal_velocity) <= 0f)
            {
                velocity = Vector3.zero;
                break;
            }
        }
    }

    private void AirMove()
    {
        Vector2 moveDir = moveAction.ReadValue<Vector2>();

        Horizontal = moveDir.x;
        Vertical = moveDir.y;

        var wishvel = forwardSpeed * Vertical * transform.forward + sideSpeed * Horizontal * transform.right;
        float wishspeed = Mathf.Min(maxspeed, wishvel.magnitude);

        WishDir = wishvel.normalized;

        if (Grounded)
        {
            Accelerate(ref velocity, wishspeed, accelerate);
            GroundMove();
        }

        else
        {
            AirAccelerate(ref velocity, wishspeed, accelerate);

            velocity += gravity * Time.deltaTime * Vector3.up;
            FlyMove();
        }

        velocityXZ = new(velocity.x, 0f, velocity.z);
    }

    private void PlayerMove()
    {
        CategorizePosition();

        if (jumpAction.IsPressed() && Grounded)
            Jump();

        Friction(ref velocity, friction);

        AirMove();

        CategorizePosition();
    }

    #endregion










    #region Publics

    /// <summary>
    /// A Function To Make The Player Jump.
    /// </summary>
    public static void Jump()
    {
        if (Time.time < Instance.nextTimeToJump || !Grounded) return;

        Instance.nextTimeToJump = Time.time + Instance.jumpCooldown;
        velocity.y += Instance.jumpForce;
    }

    /// <summary>
    /// A Function To Clip The Input Velocity Depending On A Normal Vector.
    /// </summary>
    /// <param name="input"> The Incoming Velocity. </param>
    /// <param name="normal"> The Normal Vector. </param>
    /// <param name="overbounce"> Overbounce To Overshoot The Output Velocity. </param>
    /// <returns></returns>
    public static Vector3 ClipVelocity(Vector3 input, Vector3 normal, float overbounce = 1f)
    {
        var output = input;
        var backoff = Vector3.Dot(output, normal) * overbounce;

        for (int i = 0; i < 3; i++)
        {
            var change = normal[i] * backoff;
            output[i] = output[i] - change;

            if (output[i] > -.1f && output[i] < .1f) output[i] = 0f;
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
    public static void Accelerate(ref Vector3 vel, float wishspeed, float accel)
    {
        float wishspd = wishspeed;
        float currentspeed = Vector3.Dot(vel, WishDir);

        float addspeed = wishspd - currentspeed;
        if (addspeed <= 0) return;

        float accelspeed = accel * Time.deltaTime * wishspd;
        accelspeed = Mathf.Min(accelspeed, addspeed);

        vel.x += accelspeed * WishDir.x;
        vel.z += accelspeed * WishDir.z;
    }

    /// <summary>
    /// A Function To Scale And Manipulate A Velocity Vector Based On An Acceleration
    /// Factor To Emulate Acceleration While Airborne.
    /// </summary>
    /// <param name="vel"> The Velocity To Be Scaled. </param>
    /// <param name="wishspeed"> The Speed Wished To Accelerate To. </param>
    /// <param name="accel"> The Acceleration Factor. </param>
    public static void AirAccelerate(ref Vector3 vel, float wishspeed, float accel)
    {
        float wishspd = Mathf.Min(wishspeed, 3f); // 30f
        float currentspeed = Vector3.Dot(vel, WishDir);

        float addspeed = wishspd - currentspeed;
        if (addspeed <= 0f) return;

        float accelspeed = accel * Time.deltaTime * wishspd;
        accelspeed = Mathf.Min(accelspeed, addspeed);

        vel.x += accelspeed * WishDir.x;
        vel.z += accelspeed * WishDir.z;
    }

    /// <summary>
    /// A Function To Scale The Velocity Based On A Friction Amount.
    /// </summary>
    /// <param name="vel"> The Velocity To Be Scaled. </param>
    /// <param name="frictionAmount"> The Friction Amount. </param>
    /// <param name="stopSpeed"> The Threshold To Finish The Scaling. </param>
    public static void Friction(ref Vector3 vel, float frictionAmount, float stopThreshold = .1f)
    {
        float speed = vel.magnitude, newspeed, control;
        float drop = 0f;

        if (speed < stopThreshold)
        {
            vel.x = 0f;
            vel.z = 0f;
            return;
        }

        if (Grounded)
        {
            control = speed < Instance.stopspeed ? Instance.stopspeed : speed;
            drop += control * frictionAmount * Time.deltaTime;
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