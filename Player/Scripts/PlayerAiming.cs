using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAiming : MonoBehaviour
{
    public static PlayerAiming Instance {get; private set;}

    #region Data

    public static float defaultFOV      {get; private set;}
    public static float xMovement       {get; private set;}
    public static float yMovement       {get; private set;}
    public static Vector2 punchAngle    {get; private set;}
    public static Vector3 realRotation  {get; private set;}
    public static Vector2 punchAngleVel {get; private set;}


    #endregion








    #region Globals


    #region References

    [Header("References")]
    [SerializeField] private Transform bodyTransform;
    [SerializeField] private Camera cam; //, cam2; // <-- For Weapon Camera.
    [SerializeField] private PlayerInput inp;

    #endregion


    [Space]


    #region Sensitivity

    [Header("Sensitivity")]
    [SerializeField] private float sensitivityMultiplier = 7f;
    [SerializeField] private float horizontalSensitivity = 1f;
    [SerializeField] private float verticalSensitivity   = 1f;
    
    #endregion


    [Space]


    #region Restrictions

    [Header("Restrictions")]
    [SerializeField] private bool restrictY = true;
    [SerializeField] private float minYRotation = -90f;
    [SerializeField] private float maxYRotation = 90f;

    [Space]

    [SerializeField] private bool restrictX = false;
    [SerializeField] private float minXRotation = -90f;
    [SerializeField] private float maxXRotation = 90f;

    #endregion

    
    [Space]
    

    #region Axis

	[Header("Invert Axis")]
	[SerializeField] private bool invertX = false;
	[SerializeField] private bool invertY = false;

    [Header("Camera Smoothing")]
    [SerializeField] private bool smoothingEnabled = true;
    [SerializeField] private float SmoothingSpeed = 8f;

    #endregion


    [Space]


    #region Camera Roll

    [Header("Camera Roll")]
    [SerializeField] private bool enableCameraRoll = true;
    [SerializeField] private float rollAngle = 5f, rollSpeed = 6.5f;

    #endregion


    [Space]


    #region Dynamic Fov

    [Header("Dynamic Field Of View")]
    [SerializeField] private bool enableDynamicFieldOfView = true;
    [SerializeField] private float fovMultiplier = 1.5f;
    [SerializeField] private Vector2 dynamicFovRange = new Vector3(-10f, 10f);

    #endregion


    [Space]


    #region Privates

    private Vector3 smoothBodyRotation;
    private Vector3 smoothCameraRotation;

    private InputAction CameraLook;

    #endregion


    #endregion








    #region Callbacks

    private void OnEnable()
    {
        CameraLook = inp.actions["look"];
        CameraLook.Enable();
    }

    private void OnDisable() 
    {
        CameraLook.Disable();
    }

    private void Awake()
    {
        if (Instance) 
        {
            Debug.LogWarning($"2 Or More Instances Of PlayerAiming Were Found In Scene, Deleting This Object: '{gameObject.name}'!");
            Destroy(gameObject);
        }

        else
            Instance = this;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        defaultFOV = cam.fieldOfView;

        if (!bodyTransform) bodyTransform = transform.parent;
        if (!cam) cam = GetComponentInChildren<Camera>();

        smoothBodyRotation = bodyTransform.eulerAngles;
        smoothCameraRotation = transform.eulerAngles;

    }

    private void Update()
    {
        if (Mathf.Abs(Time.timeScale) <= 0f)
            return;
        
        if (!bodyTransform)
        {
            Debug.Log("No Body For Camera To Exist On! Script Will Not Work!");
            return;
        }

        if (!cam) Debug.Log("Camera Cannot Be Found At All! Make Sure To Assign It, Camera Effects Will Not Work.");

        GetInput();

        Vector3 targetBodyRotation = new(0f, realRotation.y + 90f, 0f);
        Vector3 targetCameraRotation = new(realRotation.x, realRotation.y + 90f, realRotation.z);

        // Rotation Appliance.
        if (smoothingEnabled) {
            smoothBodyRotation   = Vector3.Lerp(smoothBodyRotation, targetBodyRotation, SmoothingSpeed * Time.deltaTime);
            smoothCameraRotation = Vector3.Lerp(smoothCameraRotation, targetCameraRotation, SmoothingSpeed * Time.deltaTime);
        } else {
            smoothBodyRotation   = targetBodyRotation;
            smoothCameraRotation = targetCameraRotation;
        }

        bodyTransform.eulerAngles = smoothBodyRotation;
        transform.eulerAngles = smoothCameraRotation;

        Vector3 cameraRotationWithPunch = transform.eulerAngles;
        cameraRotationWithPunch.x += punchAngle.x;
        cameraRotationWithPunch.y += punchAngle.y;

        transform.eulerAngles = cameraRotationWithPunch;

        // Dynamic FOV:
        if (!cam || !enableDynamicFieldOfView) return;

        cam.fieldOfView = defaultFOV + Vector3.Dot(gameMovement.velocityXZ, transform.forward) * fovMultiplier;
        cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, defaultFOV - dynamicFovRange.x, defaultFOV + dynamicFovRange.y);
    }

    #endregion








    #region Functions
    
    private void GetInput ()
    {
        // Mouse Input:
        Vector2 lookDir = CameraLook.ReadValue<Vector2>();
        float xMovement = lookDir.x * horizontalSensitivity * sensitivityMultiplier * Time.timeScale;
        float yMovement = -lookDir.y * verticalSensitivity *  sensitivityMultiplier * Time.timeScale;

        // Real Rotation Calculation:
        realRotation = new Vector3
        (
            // X:
            restrictY ? Mathf.Clamp(
                realRotation.x + (invertY ? -yMovement : yMovement), minYRotation, maxYRotation
            ) : realRotation.x + yMovement,

            // Y:
            restrictX ? Mathf.Clamp(
                realRotation.y + (invertX ? -xMovement : xMovement), minXRotation, maxXRotation
            ) : realRotation.y + xMovement,
            
            // Z:
            enableCameraRoll ? Mathf.Lerp(realRotation.z, rollAngle * gameMovement.Horizontal, Time.deltaTime * rollSpeed) : 0f
        );
    }

    #endregion
}
