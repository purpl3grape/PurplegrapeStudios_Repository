using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#region Movement Specific Enumerations
public enum MovementType
{
    Player,
    ThirdPersonPlayer,
    BumperCar,
}

public enum InputRange
{
    Zero_One,
    MinusOne_Zero,
    MinusOne_One,
}

public enum FlightRunnerInputType
{
    MouseAndKeyboard,
    LogitechG920Wheel,
}

public enum FlightRunnerDriveType
{
    TwoWheel,
    FourWheel,
}
#endregion

public class PlayerMovement : MonoBehaviour
{
    #region Hidden Public Variables
    //Player / Flight Runner Movement Type Information (Enums)
    [HideInInspector] public MovementType MovementType;
    [HideInInspector] public FlightRunnerInputType FlighRunnerInputType;
    [HideInInspector] public FlightRunnerDriveType FlightRunnerDriveType;
    #endregion Hidden Public Variables

    #region Public Variables
    [Header("Current Spawned Prefab (Player / Flight Runner)")] public SpawnCharacterType SpawnCharacterType;
    [Header("Layers for Groundcheck (Player / Flight Runner)")] public LayerMask RayCastLayersToHit;
    [Header("Player Movement Settings")] public PlayerMovementSettings PMS;
    [Header("Flight Runner Movement Settings")] public FlightRunnerMovementSettings FMS;
    #endregion Public variables

    #region Private Variables

    #region Command Class Instances
    [Header("Player Commands (Forward, back, jump, etc)")] private Cmd cmd;
    [Header("Flight Runner Commands (Forward, Steer, etc)")] private FlightRunnerCmd FCmd;
    #endregion Command Class Instances

    #region References: General Components
    private Transform Transform;
    private Transform MainCameraTransform;
    private Rigidbody Rigidbody;
    private PhotonView PhotonView;
    #endregion References: General

    #region References: Player Specific Components
    private PlayerObjectComponents PlayerObjectComponents;
    private PlayerAnimation PlayerAnimation;
    #endregion References: Player

    #region References: FlightRunner Specific Components
    private FlightRunnerObjectComponents FlightRunnerObjectComponents;
    #endregion References: FlightRunner

    #region FPS Calculation
    private float fpsDisplayRatePerSecond = 4.0f;
    private float frameCount = 0;
    private float dt = 0.0f;
    private float fps = 0.0f;
    #endregion FPS Calculation

    #region Strings
    private string S_BouncePad = "BouncePad";
    private string S_Level = "Level";
    private string S_MetersPerSecond = " m/s";
    private string S_MouseX = "Mouse X";
    private string S_SpeedRamp = "SpeedRamp";
    #endregion Strings

    #endregion Private Variables

    #region Required Classes (PlayerMovementSettings + CMD / FlightRunnerMovementSettings + FCMD)
    [System.Serializable]
    public class PlayerMovementSettings
    {
        //Parameters Movement
        [Range(0, 200)] [Header("Physics: Player acceleration moving in air")] public float P_AirAcceleration = 2.0f;
        [Range(0, 200)] [Header("Physics: Player decceleration moving in air")] public float P_AirDeacceleration = 2.0f;
        [Range(0, 20)] [Header("Physics: Player strafe Left/Right Weight moving in air")] public float P_AirControl = 0.3f;
        [Range(0, 2)] [Header("Physics: Amout of jumps player is allowed to make")] public int P_JumpAttempts = 2;
        [Range(0, 20)] [Header("Physics: Time interval for Second Jump")] public float P_DoubleJumpDeltaTime = 0.25f;
        [Range(0, 200)] [Header("Physics: Friction applied to player when grounded")] public float P_Friction = 6;
        [Range(0, 200)] [Header("Physics: Gravity force applied to player")] public float P_Gravity = 20.0f;
        [Range(0, 200)] [Header("Physics: Jump force applied by player")] public float P_JumpSpeed = 8.0f;
        [Range(0, 200)] [Header("Physics: Maximum speed player can move")] public float P_MaxSpeed = 150f;
        [Range(0, 200)] [Header("Physics: Speed V.S. Input Weight (Usually Double Movespeed for best results)")] public float P_MoveScale = 1.0f;
        [Range(0, 200)] [Header("Physics: Player movement speed")] public float P_MoveSpeed = 7.0f;
        [Range(0, 200)] [Header("Physics: Player acceleration moving on ground")] public float P_RunAcceleration = 14;
        [Range(0, 200)] [Header("Physics: Player decceleration  moving on ground")] public float P_RunDeacceleration = 10;
        [Range(0, 200)] [Header("Physics: Player strafe Left/Right acceleration")] public float P_SideStrafeAcceleration = 50;
        [Range(0, 200)] [Header("Physics: Player strafe Left/Right speed")] public float P_SideStrafeSpeed = 1;
        [Range(0, 20)] [Header("Audio: Player Footsteps")] public float P_WalkSoundRate = 0.15f;
        //Values: Movement General
        [HideInInspector] public bool V_Airjump = false;
        [HideInInspector] public Vector3 V_BoostVelocity;
        [HideInInspector] public bool V_IsBoosted;                              // For Jumppads / Bounce Pads
        [HideInInspector] public bool V_IsBouncePadWallDetected;
        [HideInInspector] public bool V_IsDisplayDustFX;
        [HideInInspector] public bool V_IsDoubleJumping = false;
        [HideInInspector] public bool V_IsFloorDetected;                        // True Measure of 'Grounded', since IsPlayerGrounded may be false upon landing on Jump Pad / Bounce Pad / Speed Ramp
        [HideInInspector] public bool V_IsGrounded = false;
        [HideInInspector] public bool V_IsHitCeiling = false;
        [HideInInspector] public bool V_IsJumping = false;
        [HideInInspector] public bool V_IsLanded = true;
        [HideInInspector] public bool V_IsQuietWalk = false;
        [HideInInspector] public bool V_IsSliding;                              // For Speed Ramps
        [HideInInspector] public bool V_KnockBackOverride = false;
        [HideInInspector] public float V_MouseSensitivity = 100f;
        [HideInInspector] public Vector3 V_MoveDirectionNorm = Vector3.zero;
        [HideInInspector] public float V_RotationY = 0.0f;
        [HideInInspector] public float V_TopVelocity = 0.0f;
        [HideInInspector] public float V_PlayerFriction = 0.0f;                 // Used to display real time friction values
        [HideInInspector] public Vector3 V_PlayerVelocity = Vector3.zero;
        [HideInInspector] public bool V_WishJump = false;
        //Values: Raycasting
        [HideInInspector] public int V_RaycastFloorType = -1;
        [HideInInspector] public Ray[] V_Rays_Ground = new Ray[5];
        [HideInInspector] public Ray V_Ray_Ceiling;
        [HideInInspector] public Ray V_Ray_Velocity;
        [HideInInspector] public RaycastHit[] V_CeilingHits;
        [HideInInspector] public RaycastHit[] V_WallHits;
        [HideInInspector] public RaycastHit[] V_GroundHits;
        [HideInInspector] public RaycastHit V_GroundHit;
        [HideInInspector] public Vector3 V_SpeedReduction;
        [HideInInspector] public Transform V_GroundHitTransform;
        [HideInInspector] public float V_TempJumpSpeed;
        [HideInInspector] public float V_TempRampJumpSpeed;
        [HideInInspector] public string v_GroundHitTransformName;
    }

    [System.Serializable]
    public class FlightRunnerMovementSettings
    {
        [Range(0,20)] [Header("Physics: Gravity force applied on FlightRunner")] public float P_Gravity;
        [Range(0, 20)] [Header("Physics: Break force applied on FlightRunner")] public float P_BreakAccelMultiplier;
        [Range(0,20)] [Header("Physics: Acceleration force applied on FlightRunner")] public float P_AccelMultiplier;
        [Range(0, 20)] [Header("Physics: Road Friction force applied on FlightRunner")] public float P_FrictionMultiplier;
        [Header("Layers for FlightRunner SlopeCheck to rotate to slope")] public LayerMask SlopeLayer;
        public Dictionary<int, int> D_GearMaxSpeed = new Dictionary<int, int>();
        [HideInInspector] public Ray[] V_Rays_Ground = new Ray[5];
        [HideInInspector] public RaycastHit[] V_GroundHits;
        [HideInInspector] public RaycastHit lr;
        [HideInInspector] public RaycastHit rr;
        [HideInInspector] public RaycastHit lf;
        [HideInInspector] public RaycastHit rf;
        [HideInInspector] public Vector3 upDir;

        [HideInInspector] public Ray V_Ray_Velocity;
        [HideInInspector] public int V_Gear;
        [HideInInspector] public float V_AccelMag;
        [HideInInspector] public float V_BreakMag;
        [HideInInspector] public bool V_IsFlightRunnerBoost;
        [HideInInspector] public bool V_IsFourWheelDrive;
        [HideInInspector] public float V_SteerMag;
        [HideInInspector] public float tiltLerpValue;
        [HideInInspector] public bool V_IsGrounded;
        [HideInInspector] public bool V_IsCollision;
        [HideInInspector] public Vector3 V_Velocity;
        [HideInInspector] public RaycastHit[] V_WallHits;
        [HideInInspector] public Vector3 V_SpeedReduction;


    }

    private class Cmd
    {
        public float forwardmove;
        public float rightmove;
    }

    private class FlightRunnerCmd
    {
        public float steeringWheel;
        public float accelPedal;
        public float breakPedal;
        public int gearNumber;
    }
    #endregion Required Classes

    #region Main Functions

    #region Initialization Methods
    private void Start()
    {
        InitGeneralReferences();

        // Player/FlightRunner References for all Clients initialized
        InitPlayerReferences();
        InitFlightRunnerReferences();

        if (!PhotonView.isMine) return;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Player/FlightRunner Data locally initialized
        InitPlayerData();
        InitFlightRunnerData();
    }

    private void InitGeneralReferences()
    {
        PhotonView = GetComponent<PhotonView>();
        Rigidbody = GetComponent<Rigidbody>();
        Transform = GetComponent<Transform>();
    }

    #region Init: Player/FlightRunner Specifics
    private void InitPlayerReferences()
    {
        if (SpawnCharacterType.Equals(SpawnCharacterType.Player))
        {
            PlayerObjectComponents = GetComponent<PlayerObjectComponents>();
            MainCameraTransform = PlayerObjectComponents.PlayerCamera.transform;
            PlayerAnimation = GetComponent<PlayerAnimation>();
        }
    }

    private void InitFlightRunnerReferences()
    {
        if (SpawnCharacterType.Equals(SpawnCharacterType.FlightRunner))
        {
            FlightRunnerObjectComponents = GetComponent<FlightRunnerObjectComponents>();
            MainCameraTransform = FlightRunnerObjectComponents.MainCamera.transform;
        }
    }

    private void InitPlayerData()
    {
        if (SpawnCharacterType.Equals(SpawnCharacterType.Player))
        {
            cmd = new Cmd();
            PMS.V_GroundHits = new RaycastHit[255];
            PMS.V_CeilingHits = new RaycastHit[255];
            PMS.V_WallHits = new RaycastHit[255];
        }
    }


    private void InitFlightRunnerData()
    {
        if (SpawnCharacterType.Equals(SpawnCharacterType.FlightRunner))
        {
            FCmd = new FlightRunnerCmd();
            FCmd.gearNumber = 1;
            FMS.V_Gear = FCmd.gearNumber;
            FMS.V_GroundHits = new RaycastHit[255];
            FMS.V_WallHits = new RaycastHit[255];
            FlighRunnerInputType = FlightRunnerInputType.MouseAndKeyboard;
            FlightRunnerDriveType = FlightRunnerDriveType.TwoWheel;

            FMS.D_GearMaxSpeed.Add(-1, 60);
            FMS.D_GearMaxSpeed.Add(1, 20);
            FMS.D_GearMaxSpeed.Add(2, 30);
            FMS.D_GearMaxSpeed.Add(3, 40);
            FMS.D_GearMaxSpeed.Add(4, 50);
            FMS.D_GearMaxSpeed.Add(5, 60);
        }
    }
    #endregion Init: Player/FlightRunner Specifics

    #endregion InitializationMethods

    #region Update / FixedUpdate Loops
    private void Update()
    {
        //LOCAL PLAYER SECTION
        if (PhotonView.isMine)
        {
            if (InputManager.Instance.GetKeyDown(InputCode.Settings))
            {
                if (Cursor.lockState == CursorLockMode.None)
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
                else if (Cursor.lockState == CursorLockMode.Locked)
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
            }

            frameCount++;
            dt += Time.deltaTime;
            if (dt > 1.0f / fpsDisplayRatePerSecond)
            {
                fps = Mathf.Round(frameCount / dt);
                frameCount = 0;
                dt -= 1.0f / fpsDisplayRatePerSecond;

                PlayerScreenDisplay.Instance.Text_CurrentFrameRate.text = fps.ToString();
                PlayerScreenDisplay.Instance.Text_Ping.text = PhotonNetwork.GetPing().ToString();
                PlayerScreenDisplay.Instance.Text_PlayerVelocity.text = ((int)PMS.V_PlayerVelocity.magnitude).ToString() + S_MetersPerSecond;
            }

            if (CameraManager.Instance.spectateMode == CameraManager.SpectateMode.Free)
                return;

            //Player Behavior Update
            if (SpawnCharacterType.Equals(SpawnCharacterType.Player))
                Update_PlayerBehavior();

            //Car Behavior Update
            else if (SpawnCharacterType.Equals(SpawnCharacterType.FlightRunner))
            {
                FlightRunner_GetInput(Time.deltaTime);
                if (Cursor.visible)
                {
                    GameCanvas.Instance.gameObject.SetActive(true);
                }
                else
                {
                    GameCanvas.Instance.gameObject.SetActive(false);
                }

            }
        }
        else
        {
            //NETWORKED PLAYER SECTION
            if (SpawnCharacterType.Equals(SpawnCharacterType.Player))
            {
                if (PlayerObjectComponents.networkPlayerMovement.NetworkPlayerHealth > 0)
                {
                    //Dust FX for Player Movement
                    DustFX_Behavior(PlayerObjectComponents.networkPlayerMovement.NetworkPlayerVelocity, PlayerObjectComponents.networkPlayerMovement.NetworkPlayerFloorDetected);
                }
            }
            else if (SpawnCharacterType.Equals(SpawnCharacterType.FlightRunner))
            {

            }

        }
    }

    // Physical Movement of Rigidbody performed in FixedUpdate Loop
    private void FixedUpdate()
    {
        if (!PhotonView.isMine) { return; }

        /* MOVEMENT PLAYER: here's the important part*/
        if (SpawnCharacterType.Equals(SpawnCharacterType.Player))
        {
            if (Rigidbody.velocity.magnitude > 0) { Rigidbody.velocity = Vector3.zero; }

            Rigidbody.MovePosition(Rigidbody.position + PMS.V_PlayerVelocity * Time.fixedDeltaTime);
        }

        /* MOVEMENT Flight Runner: here's the important part*/
        else if (SpawnCharacterType.Equals(SpawnCharacterType.FlightRunner))
        {
            FlightRunner_BladesRotate();
            FlightRunner_SpinWheels();
            FlightRunner_RotateWheels();
            FlightRunner_RotateChasis(Time.fixedDeltaTime);
            FlightRunner_GroundCheck();
            FlightRunner_TiltSlope();
            FlightRunner_MoveRigidBody();
        }
    }
    #endregion Update / FixedUpdate Loops

    #region Player Movement Methods
    /* Player Behavior Section 
     *
     * 2)   Queue Jump
     * 3)   GroundCheck
     * 4)   CeilingCheck
     * 5)   WallCheck
     * 6)   GroundMove / AirMove ~ MovementDirection, Accelerate
     * 7)   SpeedLimiter
     * 8)   ApplyFriction
     * 9)   Player Rotate
     * 10)  Player Move
     */

    // Called in Update(). This is the General Player Behavior Loop, for updating values in inputs
    private void Update_PlayerBehavior()
    {
        //TODO: Zoom_Vs_NoZoom
        Player_Rotate();                                            // 1) Rotate Player along Y Axis

        //IF PLAYER IS ALIVE
        if (!PlayerObjectComponents.playerShooting.IsPlayerDead)
        {
            //Player Alive Movement Behavior
            Player_DetectCollision(2.5f);                           // 2) Detect Collision Against Wall
            Player_GroundCheck(2.1f);                               // 3) Detect Collision Against Ground
            Player_CeilingCheck();                                  // 4) Detect Collision Against Ceiling
            Player_QueueJump();                                     // 5) Check Jump Input

            GetComponent<CapsuleCollider>().radius = .5f;
            GetComponent<CapsuleCollider>().height = 2f;

            DustFX_Behavior(PMS.V_PlayerVelocity, PMS.V_IsGrounded);

            if (PMS.V_IsGrounded)
            {
                if (PMS.V_IsLanded == false)
                    PMS.V_IsLanded = true;
                PMS.V_IsJumping = false;
                Player_GroundMove();
            }
            else if (!PMS.V_IsGrounded)
            {
                Player_AirMove();
            }

            Player_LimitSpeed();
        }
        else
        {
            //IF PLAYER IS DEAD
            GetComponent<CapsuleCollider>().height = .25f;
            GetComponent<CapsuleCollider>().radius = .25f;
            if (PMS.V_IsGrounded) PMS.V_PlayerVelocity.y -= PMS.P_Gravity * Time.deltaTime;
            Player_DetectCollision(.5f);                            // 2) Detect Collision Against Wall
            Player_GroundCheck(1f);                                 // 3) Detect Collision Against Ground
            Player_CeilingCheck();                                  // 4) Detect Collision Against Ceiling
            if (PMS.V_IsGrounded)
            {
                Player_ApplyFriction(.5f);
            }
        }
    }

    // Detects Player Ground Collision, and initializes multiple rays
    private void Player_GroundCheck(float distance)
    {
        if (PMS.V_KnockBackOverride)
        {
            PMS.V_IsGrounded = false;
            return;
        }

        PMS.V_TempJumpSpeed = PMS.P_JumpSpeed;
        PMS.V_TempRampJumpSpeed = PMS.P_JumpSpeed * 10;
        if (PMS.V_WishJump) { PMS.V_TempJumpSpeed *= 10; PMS.V_TempRampJumpSpeed *= 2; }
        PMS.V_Rays_Ground[0] = new Ray(Transform.position, -Transform.up);
        PMS.V_Rays_Ground[1] = new Ray(Transform.position + Transform.forward, -Transform.up);
        PMS.V_Rays_Ground[2] = new Ray(Transform.position - Transform.forward, -Transform.up);
        PMS.V_Rays_Ground[3] = new Ray(Transform.position + Transform.right, -Transform.up);
        PMS.V_Rays_Ground[4] = new Ray(Transform.position - Transform.right, -Transform.up);
        //DO NOT WANT NOT BEING ABLE TO JUMP UNEXPECTEDLY. MUST RAYCAST AT LEAST >= PLAYER HEIGHT=4

        if (!Player_GroundRayCast(PMS.V_Rays_Ground, distance))
        {
            PMS.V_IsFloorDetected = false;
            PMS.V_IsGrounded = false;
            if (PMS.V_PlayerVelocity.y < -100f)
            {
                PMS.V_IsLanded = false;
            }
        }
    }

    // Sets up multiple rays for ground ray casting
    private bool Player_GroundRayCast(Ray[] rays, float dist)
    {
        foreach (Ray ray in rays)
        {
            if (Physics.RaycastNonAlloc(ray, PMS.V_GroundHits, dist, RayCastLayersToHit) > 0)
            {
                foreach (RaycastHit hit in PMS.V_GroundHits)
                {

                    PMS.V_IsFloorDetected = true;
                    PMS.V_GroundHit = hit;
                    PMS.V_GroundHitTransform = PMS.V_GroundHit.transform;
                    PMS.v_GroundHitTransformName = PMS.V_GroundHitTransform.name;

                    if (hit.transform.CompareTag(S_SpeedRamp))
                    {
                        PMS.V_IsGrounded = false;
                        if (!PMS.V_IsBoosted)
                        {
                            PMS.V_BoostVelocity = Vector3.Cross(PMS.V_GroundHit.normal, -PMS.V_GroundHit.transform.right) * 75 + PMS.V_TempRampJumpSpeed * Transform.up / 10;
                            PMS.V_PlayerVelocity = PMS.V_BoostVelocity;
                            PMS.V_IsBoosted = true;
                            PMS.V_IsSliding = true;
                        }
                    }
                    else if (hit.transform.CompareTag(S_BouncePad))
                    {
                        PMS.V_IsGrounded = false;
                        if (!PMS.V_IsBoosted)
                        {
                            PMS.V_BoostVelocity = PMS.V_GroundHit.normal * 50 + PMS.V_TempJumpSpeed * Transform.up / 10 + new Vector3(PMS.V_PlayerVelocity.x, 0, PMS.V_PlayerVelocity.z);
                            PMS.V_PlayerVelocity = PMS.V_BoostVelocity;
                            PMS.V_IsBoosted = true;
                            PMS.V_IsSliding = false;
                        }

                    }
                    else
                    {
                        if (!PMS.V_IsBouncePadWallDetected)
                        {
                            PMS.V_IsBoosted = false;
                            PMS.V_IsGrounded = true;
                            PMS.V_PlayerVelocity.y = 0;
                        }
                        if (PMS.V_IsSliding) PMS.V_IsSliding = false;
                    }
                    return true;
                }

            }
        }
        return false;
    }

    // Sets Player movement direction bassed on Inputs
    private void Player_SetMovementDir()
    {
        if (EventManager.Instance.GetScore(PhotonView.owner.NickName, PlayerStatCodes.Health) <= 0)
            return;

        if (CameraManager.Instance.spectateMode.Equals(CameraManager.SpectateMode.Free))
        {
            cmd.forwardmove = 0;
            cmd.rightmove = 0;
            return;
        }

        if (PMS.V_IsSliding) return;


        if (InputManager.Instance.GetKey(InputCode.Forward).Equals(true))
        {
            cmd.forwardmove = 1;
        }
        else if (InputManager.Instance.GetKey(InputCode.Back).Equals(true))
        {
            cmd.forwardmove = -1;
        }
        else
        {
            cmd.forwardmove = 0;
        }
        if (InputManager.Instance.GetKey(InputCode.Left).Equals(true))
        {
            cmd.rightmove = -1;
        }
        else if (InputManager.Instance.GetKey(InputCode.Right).Equals(true))
        {
            cmd.rightmove = 1;
        }
        else
        {
            cmd.rightmove = 0;
        }

        if (InputManager.Instance.GetKey(InputCode.Run).Equals(true))
        {
            PMS.P_MoveSpeed = 20;
        }
        else
        {
            PMS.P_MoveSpeed = 10;
        }
        //cmd.forwardmove = Input.GetAxis("Vertical");
        //cmd.rightmove   = Input.GetAxis("Horizontal");
    }

    // Schedules a Jump request for Jump Behavior
    private void Player_QueueJump()
    {
        if (CameraManager.Instance.spectateMode == CameraManager.SpectateMode.Free)
            return;

        //if (Input.GetKey(KeyCode.Space))
        if (InputManager.Instance.GetKey(InputCode.Jump))
        {
            PMS.V_WishJump = true;
        }
        else
        {
            PMS.V_WishJump = false;
        }
    }

    // Updates PlayerVelocity while player is in the air
    private void Player_AirMove()
    {
        Vector3 wishdir;
        float wishvel = PMS.P_AirAcceleration;
        float accel;

        float scale = CmdScale();

        PMS.V_KnockBackOverride = false;
        PMS.V_IsBoosted = false;

        Player_SetMovementDir();

        wishdir = new Vector3(cmd.rightmove, 0, cmd.forwardmove);
        wishdir = Transform.TransformDirection(wishdir);

        float wishspeed = wishdir.magnitude;
        wishspeed *= PMS.P_MoveSpeed;

        wishdir.Normalize();
        PMS.V_MoveDirectionNorm = wishdir;
        wishspeed *= scale;

        // CPM: Aircontrol
        float wishspeed2 = wishspeed;
        if (Vector3.Dot(PMS.V_PlayerVelocity, wishdir) < 0)
            accel = PMS.P_AirDeacceleration;
        else
            accel = PMS.P_AirAcceleration;
        // If the player is ONLY strafing left or right
        if (cmd.forwardmove == 0 && cmd.rightmove != 0)
        {
            if (wishspeed > PMS.P_SideStrafeSpeed)
                wishspeed = PMS.P_SideStrafeSpeed;
            accel = PMS.P_SideStrafeAcceleration;
        }

        Player_Accelerate(wishdir, wishspeed, accel);
        if (PMS.P_AirControl > 0)
        {
            Player_AirControl(wishdir, wishspeed2);
        }
        // Apply gravity
        PMS.V_PlayerVelocity.y -= PMS.P_Gravity * Time.deltaTime;
    }

    // Updates PlayerVelocity based on AirControl Parameter Values
    private void Player_AirControl(Vector3 wishdir, float wishspeed)
    {
        float zspeed;
        float speed;
        float dot;
        float k;

        // Can't control movement if not moving forward or backward
        if (cmd.forwardmove == 0 || wishspeed == 0)
            return;

        zspeed = PMS.V_PlayerVelocity.y;
        PMS.V_PlayerVelocity.y = 0;
        /* Next two lines are equivalent to idTech's VectorNormalize() */
        speed = PMS.V_PlayerVelocity.magnitude;
        PMS.V_PlayerVelocity.Normalize();

        dot = Vector3.Dot(PMS.V_PlayerVelocity, wishdir);
        k = 32;
        k *= PMS.P_AirControl * dot * dot * Time.deltaTime;

        // Change direction while slowing down
        if (dot > 0)
        {
            PMS.V_PlayerVelocity.x = PMS.V_PlayerVelocity.x * speed + wishdir.x * k;
            PMS.V_PlayerVelocity.y = PMS.V_PlayerVelocity.y * speed + wishdir.y * k;
            PMS.V_PlayerVelocity.z = PMS.V_PlayerVelocity.z * speed + wishdir.z * k;

            PMS.V_PlayerVelocity.Normalize();
            PMS.V_MoveDirectionNorm = PMS.V_PlayerVelocity;
        }

        PMS.V_PlayerVelocity.x *= speed;
        PMS.V_PlayerVelocity.y = zspeed; // Note this line
        PMS.V_PlayerVelocity.z *= speed;

    }

    // Updates PlayerVelocity while player is grounded.
    private void Player_GroundMove()
    {
        Vector3 wishdir;

        //set airjump to false again to allow an Extra Jump in Airmove()
        PMS.V_Airjump = false;

        // Do not apply friction if the player is queueing up the next jump
        if (!PMS.V_WishJump)
        {
            if (PMS.V_RaycastFloorType == 1)
            {
                Player_ApplyFriction(0f);
            }
            else
            {
                Player_ApplyFriction(1f);
            }
        }
        else
        {
            Player_ApplyFriction(0);
        }

        float scale = CmdScale();

        Player_SetMovementDir();

        wishdir = new Vector3(cmd.rightmove, 0, cmd.forwardmove);
        wishdir = Transform.TransformDirection(wishdir);
        wishdir.Normalize();
        PMS.V_MoveDirectionNorm = wishdir;
        float wishspeed = wishdir.magnitude;
        wishspeed *= PMS.P_MoveSpeed;

        Player_Accelerate(wishdir, wishspeed, PMS.P_RunAcceleration);

        // Reset the gravity velocity		
        PMS.V_PlayerVelocity.y = 0;

        //SingleJump
        if (PMS.V_WishJump && !PMS.V_IsHitCeiling)
        {
            PMS.V_IsJumping = true;
            PMS.V_PlayerVelocity.y = PMS.P_JumpSpeed;
            PMS.V_IsDoubleJumping = false;

            PMS.V_WishJump = false;

            PMS.V_IsLanded = false;
        }
    }

    // Detects Player Ceiling Collision
    private void Player_CeilingCheck()
    {
        PMS.V_Ray_Ceiling = new Ray(Transform.position, Transform.up);

        //DO NOT WANT NOT BEING ABLE TO JUMP UNEXPECTEDLY. MUST RAYCAST AT LEAST >= PLAYER HEIGHT=4
        if (Physics.RaycastNonAlloc(PMS.V_Ray_Ceiling, PMS.V_CeilingHits, 4f + .1f, RayCastLayersToHit) > 0)
        {
            foreach (RaycastHit hit in PMS.V_CeilingHits)
            {
                PMS.V_IsHitCeiling = true;
                if (PMS.V_PlayerVelocity.y > 0)
                    PMS.V_PlayerVelocity.y *= -1;
            }
        }
        else
        {
            PMS.V_IsHitCeiling = false;
        }
    }

    // Detects Player Wall Collision
    private void Player_DetectCollision(float dist)
    {
        PMS.V_Ray_Velocity = new Ray(Transform.position, PMS.V_PlayerVelocity);
        Debug.DrawRay(Transform.position, PMS.V_PlayerVelocity, Color.red);
        PMS.V_SpeedReduction = Vector3.zero;

        if (Physics.RaycastNonAlloc(PMS.V_Ray_Velocity, PMS.V_WallHits, dist, RayCastLayersToHit) > 0)
        {
            foreach (RaycastHit collisionHit in PMS.V_WallHits)
            {
                if (collisionHit.collider == null) continue;
                if (collisionHit.transform.CompareTag(S_Level))
                {
                    PMS.V_SpeedReduction = collisionHit.normal;
                    float xMag = Mathf.Abs(PMS.V_PlayerVelocity.x);
                    float yMag = Mathf.Abs(PMS.V_PlayerVelocity.y);
                    float zMag = Mathf.Abs(PMS.V_PlayerVelocity.z);

                    PMS.V_SpeedReduction.x = PMS.V_SpeedReduction.x * xMag * 1.1f;
                    PMS.V_SpeedReduction.z = PMS.V_SpeedReduction.z * zMag * 1.1f;

                    PMS.V_PlayerVelocity.x += PMS.V_SpeedReduction.x;
                    PMS.V_PlayerVelocity.z += PMS.V_SpeedReduction.z;
                }

                if (collisionHit.transform.CompareTag(S_BouncePad))
                {
                    //Debug.Log("HitBouncePadWall");
                    PMS.V_IsGrounded = false;
                    //_playerVelocity.y = jumpSpeed * 4;
                    if (!PMS.V_IsBouncePadWallDetected)
                    {
                        PMS.V_BoostVelocity = collisionHit.normal * 50 + PMS.P_JumpSpeed * Transform.up;
                        PMS.V_PlayerVelocity = PMS.V_BoostVelocity;
                        PMS.V_IsBouncePadWallDetected = true;
                    }

                }
                else
                {
                    PMS.V_IsBouncePadWallDetected = false;
                }

            }
        }
        else
        {
            PMS.V_IsBouncePadWallDetected = false;
        }
    }

    // Controls when the dust FX for player is turned on or off
    private void DustFX_Behavior(Vector3 vel, bool onGround)
    {
        if (vel.magnitude > 0 && onGround)
        {
            if (!PMS.V_IsDisplayDustFX)
            {
                PMS.V_IsDisplayDustFX = true;
                DustFX_Activate(PMS.V_IsDisplayDustFX);
            }
        }
        else
        {
            if (PMS.V_IsDisplayDustFX)
            {
                PMS.V_IsDisplayDustFX = false;
                DustFX_Activate(PMS.V_IsDisplayDustFX);
            }
        }
    }

    // Turns On or Off Dust FX
    private void DustFX_Activate(bool val)
    {
        PlayerObjectComponents.DustPrefab.SetActive(val);
    }

    // Applies Fiction to the Player
    private void Player_ApplyFriction(float t)
    {
        if (PMS.V_IsSliding) return;

        Vector3 vec = PMS.V_PlayerVelocity;
        float speed;
        float newspeed;
        float control;
        float drop;

        vec.y = 0.0f;
        speed = vec.magnitude;
        drop = 0.0f;

        /* Only if the player is on the ground then apply friction */
        if (PMS.V_IsGrounded)
        {
            control = speed < PMS.P_RunDeacceleration ? PMS.P_RunDeacceleration : speed;
            drop = control * PMS.P_Friction * Time.deltaTime * t;
        }

        newspeed = speed - drop;
        PMS.V_PlayerFriction = newspeed;
        if (newspeed < 0)
            newspeed = 0;
        if (speed > 0)
            newspeed /= speed;

        PMS.V_PlayerVelocity.x *= newspeed;
        // playerVelocity.y *= newspeed;
        PMS.V_PlayerVelocity.z *= newspeed;
    }

    // Calculates wish acceleration based on player's cmd wishes i.e. WASD movement
    private void Player_Accelerate(Vector3 wishdir, float wishspeed, float accel)
    {
        float addspeed;
        float accelspeed;
        float currentspeed;

        if (PMS.V_PlayerVelocity.magnitude >= 100)
        {
            return;
        }


        currentspeed = Vector3.Dot(PMS.V_PlayerVelocity, wishdir);
        addspeed = wishspeed - currentspeed;
        if (addspeed <= 0)
            return;
        accelspeed = accel * Time.deltaTime * wishspeed;
        if (accelspeed > addspeed)
            accelspeed = addspeed;

        PMS.V_PlayerVelocity.x += accelspeed * wishdir.x;
        PMS.V_PlayerVelocity.z += accelspeed * wishdir.z;

    }


    // Gets Scale of movement
    private float CmdScale()
    {
        int max;
        float total;
        float scale;

        max = (int)Mathf.Abs(cmd.forwardmove);

        if (Mathf.Abs(cmd.rightmove) > max)
            max = (int)Mathf.Abs(cmd.rightmove);
        if (max == 0)
        {
            return 0;
        }
        total = Mathf.Sqrt(cmd.forwardmove * cmd.forwardmove + cmd.rightmove * cmd.rightmove);
        scale = PMS.P_MoveSpeed * max / (PMS.P_MoveScale * total);

        return scale;
    }


    // Limits Player Velocity
    private void Player_LimitSpeed()
    {
        if (PMS.V_IsBoosted || PMS.V_IsSliding)
        {
            return;
        }

        if (PMS.V_PlayerVelocity.x < -PMS.P_MaxSpeed)
        {
            PMS.V_PlayerVelocity.x = -PMS.P_MaxSpeed;
        }
        if (PMS.V_PlayerVelocity.x > PMS.P_MaxSpeed)
        {
            PMS.V_PlayerVelocity.x = PMS.P_MaxSpeed;
        }
        if (PMS.V_PlayerVelocity.z < -PMS.P_MaxSpeed)
        {
            PMS.V_PlayerVelocity.z = -PMS.P_MaxSpeed;
        }
        if (PMS.V_PlayerVelocity.z > PMS.P_MaxSpeed)
        {
            PMS.V_PlayerVelocity.z = PMS.P_MaxSpeed;
        }

    }

    // Rotates the Player
    private void Player_Rotate()
    {
        if (EventManager.Instance.GetScore(PhotonView.owner.NickName, PlayerStatCodes.Health) > 0)
        {
            PMS.V_RotationY += Input.GetAxis(S_MouseX) * (PMS.V_MouseSensitivity / 3f) * 0.1f * Time.timeScale;
        }
        Transform.rotation = Quaternion.Euler(0, PMS.V_RotationY, 0); // 1) Rotates the collider
    }

    #endregion Player Movement Methods

    #region Flight Runner Movement Methods
    /* Flight Runner Behavior Section
     * 
     * 0)    Flight Runner Input                   :: Check Input               [Update]
     * 1)    Wheel Rotate (Front / Rear if 4 wd)   :: Horiz Input               [Update]
     * 2)    Wheel Spin (Front + Rear)             :: RigidBody.Velocity        [Update]
     * 2)    Blades Rotate                         :: AccelerationMagnitude     [FixedUpdate]
     * 3)    FlightRunnerBody Rotate               :: Horiz Input               [FixedUpdate]
     * 4)    RigidBody.Velocity Update             :: Vert Input                [FixedUpdate]
     * 5)    FlightRunner Tilt                     :: AutoCheck                 [FixedUpdate]
     */

    float V_BoostTimer;
    float V_GravityValue;
    private void FlightRunner_GetInput(float DeltaTime)
    {
        if (EventManager.Instance.GetScore(PhotonView.owner.NickName, PlayerStatCodes.Health) <= 0)
            return;

        FCmd.accelPedal = Input.GetAxis("GasInput");
        FCmd.breakPedal = Input.GetAxis("BreakInput");
        FMS.V_SteerMag = FCmd.steeringWheel = Input.GetAxis("SteeringInput") * 30;

        if (Input.GetButton("Gear1Input"))
        {
            FCmd.gearNumber = 1;
        }
        else if (Input.GetButton("Gear2Input"))
        {
            FCmd.gearNumber = 2;
        }
        else if (Input.GetButton("Gear3Input"))
        {
            FCmd.gearNumber = 3;
        }
        else if (Input.GetButton("Gear4Input"))
        {
            FCmd.gearNumber = 4;
        }
        else if (Input.GetButton("Gear5Input"))
        {
            FCmd.gearNumber = 5;
        }
        else if (Input.GetButton("ReverseInput"))
        {
            FCmd.gearNumber = -1;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            FMS.V_IsFlightRunnerBoost = true;
            V_BoostTimer = 0;
        }

        if (FMS.V_IsFlightRunnerBoost)
        {
            if (V_BoostTimer < .25f)
            {
                if (V_BoostTimer == 0)
                {
                    V_GravityValue = 0;
                }
                V_BoostTimer += DeltaTime;
            }
            else
            {
                V_BoostTimer = 0;
                FMS.V_IsFlightRunnerBoost = false;
            }
        }

        if (!FMS.V_IsGrounded)
        {
            V_GravityValue = Mathf.Min(V_GravityValue += FMS.P_Gravity * DeltaTime, 40);
        }
        else
        {
            V_GravityValue = 0;
        }

        if (Input.GetKeyDown(KeyCode.F6))
        {
            if (FlightRunnerDriveType.Equals(FlightRunnerDriveType.TwoWheel))
            {
                FlightRunnerDriveType = FlightRunnerDriveType.FourWheel;
                FMS.V_IsFourWheelDrive = true;
                //UpdateDriveType(FlightRunnerDriveType.FourWheel);
            }
            else if (FlightRunnerDriveType.Equals(FlightRunnerDriveType.FourWheel))
            {
                FlightRunnerDriveType = FlightRunnerDriveType.TwoWheel;
                FMS.V_IsFourWheelDrive = false;
                //UpdateDriveType(FlightRunnerDriveType.TwoWheel);
                foreach (GameObject Wheel in FlightRunnerObjectComponents.Axel_Rear)
                {
                    Wheel.transform.localRotation = Quaternion.Euler(0, 0, 0);
                }
            }
        }

        FMS.V_Gear = FCmd.gearNumber;
        FCmd.accelPedal = FlighRunnerInputType == FlightRunnerInputType.LogitechG920Wheel ? NormalizeInput(FCmd.accelPedal, InputRange.MinusOne_One) : NormalizeInput(FCmd.accelPedal, InputRange.Zero_One);
        FCmd.breakPedal = FlighRunnerInputType == FlightRunnerInputType.LogitechG920Wheel ? NormalizeInput(FCmd.breakPedal, InputRange.MinusOne_One) : NormalizeInput(FCmd.breakPedal, InputRange.Zero_One);

        //Flight Runner Acceleration Input Calculations
        if (FCmd.accelPedal > 0)
        {
            if (FMS.V_Gear != -1)
            {
                //Forward Movement
                FMS.V_AccelMag = FMS.V_AccelMag < FMS.D_GearMaxSpeed[FMS.V_Gear] ? FMS.V_AccelMag += 5 * FCmd.accelPedal * DeltaTime : FMS.D_GearMaxSpeed[FMS.V_Gear];
            }
            else
            {
                //Reverse Movement
                FMS.V_AccelMag = FMS.V_AccelMag > -FMS.D_GearMaxSpeed[FMS.V_Gear] ? FMS.V_AccelMag -= 5 * FCmd.accelPedal * DeltaTime : -FMS.D_GearMaxSpeed[FMS.V_Gear];
            }
        }
        //Flight Runner Breaking Input Calculations
        else
        {
            //Break or Friction with Forward Move
            if (FMS.V_AccelMag > 0)
            {
                FMS.V_AccelMag = FCmd.breakPedal > 0 ? FMS.V_AccelMag -= 20 * FCmd.breakPedal * DeltaTime : FMS.V_AccelMag -= 4 * DeltaTime;
            }
            //Break or Friction with Reverse Move
            else if (FMS.V_AccelMag < 0)
            {
                FMS.V_AccelMag = FCmd.breakPedal > 0 ? FMS.V_AccelMag += 20 * FCmd.breakPedal * DeltaTime : FMS.V_AccelMag += 4 * DeltaTime;
            }
            //Idle
            else
            {
                FMS.V_AccelMag = 0;
            }
        }
    }

    private void FlightRunner_SpinWheels()
    {
        foreach (GameObject Wheel in FlightRunnerObjectComponents.Wheels_Front)
        {
            Wheel.transform.rotation *= (Quaternion.Euler(0, FMS.V_AccelMag * .35f, 0));
        }
        foreach (GameObject Wheel in FlightRunnerObjectComponents.Wheels_Rear)
        {
            Wheel.transform.rotation *= (Quaternion.Euler(0, FMS.V_AccelMag * .35f, 0));
        }

    }

    private void FlightRunner_RotateWheels()
    {
        foreach (GameObject Wheel in FlightRunnerObjectComponents.Axel_Front)
        {
            Wheel.transform.localRotation = Quaternion.Euler(0, FCmd.steeringWheel, 0);
        }
        if (FlightRunnerDriveType.Equals(FlightRunnerDriveType.FourWheel))
        {
            foreach (GameObject Wheel in FlightRunnerObjectComponents.Axel_Rear)
            {
                Wheel.transform.localRotation = Quaternion.Euler(0, -FCmd.steeringWheel, 0);
            }
        }
    }

    private void FlightRunner_RotateChasis(float DeltaTime)
    {
        if (FMS.V_Gear != -1)
        {
            if (FlightRunnerDriveType.Equals(FlightRunnerDriveType.TwoWheel))
            {
                FlightRunnerObjectComponents.HeadingObject.transform.rotation *= Quaternion.Euler(0, FCmd.steeringWheel * 2 * DeltaTime, 0);
            }
            else
            {
                FlightRunnerObjectComponents.HeadingObject.transform.rotation *= Quaternion.Euler(0, FCmd.steeringWheel * 4 * DeltaTime, 0);
            }
        }
        else
        {
            if (FlightRunnerDriveType.Equals(FlightRunnerDriveType.TwoWheel))
            {
                FlightRunnerObjectComponents.HeadingObject.transform.rotation *= Quaternion.Euler(0, -FCmd.steeringWheel * 2 * DeltaTime, 0);
            }
            else
            {
                FlightRunnerObjectComponents.HeadingObject.transform.rotation *= Quaternion.Euler(0, -FCmd.steeringWheel * 4 * DeltaTime, 0);
            }
        }
    }

    private void FlightRunner_BladesRotate()
    {
        foreach (GameObject Blade in FlightRunnerObjectComponents.BoosterBlades)
        {
            Blade.transform.rotation *= Quaternion.Euler(0, FMS.V_AccelMag * .7f + 5f, 0);
        }
    }

    private void FlightRunner_GroundCheck()
    {
        FMS.V_Rays_Ground[0] = new Ray(Transform.position, -Transform.up);
        FMS.V_Rays_Ground[1] = new Ray(Transform.position + Transform.forward * 4.0f, -Transform.up);
        FMS.V_Rays_Ground[2] = new Ray(Transform.position - Transform.forward * 4.0f, -Transform.up);
        FMS.V_Rays_Ground[3] = new Ray(Transform.position + Transform.right * 4.0f, -Transform.up);
        FMS.V_Rays_Ground[4] = new Ray(Transform.position - Transform.right * 4.0f, -Transform.up);

        FMS.V_IsGrounded = FlightRunner_GroundRaycast(FMS.V_Rays_Ground, 4.0f);
    }

    private bool FlightRunner_GroundRaycast(Ray[] rays, float dist)
    {
        foreach (Ray ray in rays)
        {
            Debug.DrawRay(Transform.position, -Transform.up, Color.red);
            if (Physics.RaycastNonAlloc(ray, FMS.V_GroundHits, dist, RayCastLayersToHit) > 0)
            {
                return true;
            }
        }
        return false;
    }

    private Vector3 FlightRunner_GetSlope(Transform tr)
    {
        Physics.Raycast(tr.position - Vector3.forward * 4.1f - (Vector3.right * 4.1f) + Vector3.up * 1, Vector3.down, out FMS.lr, 8, FMS.SlopeLayer);
        Physics.Raycast(tr.position - Vector3.forward * 4.1f + (Vector3.right * 4.1f) + Vector3.up * 1, Vector3.down, out FMS.rr, 8, FMS.SlopeLayer);
        Physics.Raycast(tr.position + Vector3.forward * 4.1f - (Vector3.right * 4.1f) + Vector3.up * 1, Vector3.down, out FMS.lf, 8, FMS.SlopeLayer);
        Physics.Raycast(tr.position + Vector3.forward * 4.1f + (Vector3.right * 4.1f) + Vector3.up * 1, Vector3.down, out FMS.rf, 8, FMS.SlopeLayer);
        FMS.upDir = (Vector3.Cross(FMS.rr.point - Vector3.up * 1, FMS.lr.point - Vector3.up * 1) +
                                                Vector3.Cross(FMS.lr.point - Vector3.up * 1, FMS.lf.point - Vector3.up * 1) +
                                                Vector3.Cross(FMS.lf.point - Vector3.up * 1, FMS.rf.point - Vector3.up * 1) +
                                                Vector3.Cross(FMS.rf.point - Vector3.up * 1, FMS.rr.point - Vector3.up * 1)
                                                ).normalized;

        Debug.DrawRay(tr.position - Vector3.forward * 4.1f - (Vector3.right * 4.1f) + Vector3.up * 1, Vector3.down, Color.green);
        Debug.DrawRay(tr.position - Vector3.forward * 4.1f + (Vector3.right * 4.1f) + Vector3.up * 1, Vector3.down, Color.green);
        Debug.DrawRay(tr.position + Vector3.forward * 4.1f - (Vector3.right * 4.1f) + Vector3.up * 1, Vector3.down, Color.green);
        Debug.DrawRay(tr.position + Vector3.forward * 4.1f + (Vector3.right * 4.1f) + Vector3.up * 1, Vector3.down, Color.green);

        FMS.upDir = new Vector3(FMS.upDir.x, tr.up.y, FMS.upDir.z);
        return FMS.upDir;
    }

    private void FlightRunner_TiltSlope()
    {
        FMS.tiltLerpValue = Mathf.Max(0.1f, (0.5f - FMS.V_AccelMag / 60));
        Transform.up = Vector3.Lerp(Transform.up, FlightRunner_GetSlope(Transform), FMS.tiltLerpValue);
    }

    private void FlightRunner_MoveRigidBody()
    {
        FlightRunner_DetectCollision(8);

        if (FMS.V_IsGrounded)
        {
            //Flight Runner Ground Movement
            if (FMS.V_IsFlightRunnerBoost)
            {
                FMS.V_AccelMag = Mathf.Min(FMS.V_AccelMag += 200 * Time.deltaTime, 50);

                Rigidbody.velocity = (FlightRunnerObjectComponents.HeadingObject.transform.forward * FMS.V_AccelMag);
            }
            else
            {
                if (FMS.V_IsCollision)
                {

                }
                else
                {
                    Rigidbody.velocity = (FlightRunnerObjectComponents.HeadingObject.transform.forward * FMS.V_AccelMag);
                }
            }
        }
        else
        {
            //Flight Runner Air Movement
            if (FMS.V_IsFlightRunnerBoost)
            {
                FMS.V_AccelMag = Mathf.Min(FMS.V_AccelMag += 200 * Time.deltaTime, 50);
                Rigidbody.velocity = (FlightRunnerObjectComponents.HeadingObject.transform.forward * FMS.V_AccelMag);
            }
            else
            {
                if (FMS.V_IsCollision)
                {
                    Rigidbody.velocity = (-Transform.up * V_GravityValue);
                }
                else
                {
                    Rigidbody.velocity = (FlightRunnerObjectComponents.HeadingObject.transform.forward * FMS.V_AccelMag - Transform.up * V_GravityValue);
                }
            }
        }
    }


    private void FlightRunner_DetectCollision(float dist)
    {
        FMS.V_Velocity = Rigidbody.velocity;

        FMS.V_Ray_Velocity = new Ray(Rigidbody.position, FMS.V_Velocity);
        Debug.DrawRay(Rigidbody.position, FMS.V_Velocity, Color.red);
        FMS.V_SpeedReduction = Vector3.zero;

        FMS.V_IsCollision = false;

        if (Physics.RaycastNonAlloc(FMS.V_Ray_Velocity, FMS.V_WallHits, dist, RayCastLayersToHit) > 0)
        {
            foreach (RaycastHit collisionHit in FMS.V_WallHits)
            {
                if (collisionHit.collider == null) continue;

                if (collisionHit.transform.CompareTag(S_Level) || 1 == 1)
                {
                    FMS.V_SpeedReduction = collisionHit.normal;
                    float xMag = Mathf.Abs(FMS.V_Velocity.x);
                    float yMag = Mathf.Abs(FMS.V_Velocity.y);
                    float zMag = Mathf.Abs(FMS.V_Velocity.z);

                    FMS.V_SpeedReduction.x = FMS.V_SpeedReduction.x * xMag * 1.5f;
                    FMS.V_SpeedReduction.z = FMS.V_SpeedReduction.z * zMag * 1.5f;

                    FMS.V_Velocity.x += FMS.V_SpeedReduction.x;
                    FMS.V_Velocity.z += FMS.V_SpeedReduction.z;
                    FMS.V_IsCollision = true;
                }

            }
        }

        //Rigidbody.velocity = flightRunnerMovementSettings.V_Velocity;
    }

    // Normalizes inputs to a range of 0 to 1
    private float NormalizeInput(float input, InputRange inputRange)
    {
        //Range: -1, 1, normalized to 0, 1

        if (inputRange.Equals(InputRange.MinusOne_One))
        {
            input = ((input + 1) / 2);
            return input;
        }
        else if (inputRange.Equals(InputRange.MinusOne_Zero))
        {
            input = input + 1;
            return input;
        }
        else if (inputRange.Equals(InputRange.Zero_One))
        {
            return input;
        }
        else
        {
            return input;
        }
    }

    #endregion Flight Runner Movement Methods

    #region PUN: RaiseEvent

    // Add PhotonEvent Callbacks
    private void OnEnable()
    {
        PhotonNetwork.OnEventCall += PhotonNetwork_OnEventCall;
    }

    // Remove PhotonEvent Callbacks
    private void OnDisable()
    {
        PhotonNetwork.OnEventCall -= PhotonNetwork_OnEventCall;
    }

    // PhotonEvent Callbacks
    private void PhotonNetwork_OnEventCall(byte eventCode, object content, int senderId)
    {
        PhotonEventCodes code = (PhotonEventCodes)eventCode;

        if (code.Equals(PhotonEventCodes.UpdateDriveType))
        {
            object[] datas = content as object[];
            if (datas.Length.Equals(4))
            {
                UpdateDriveType((FlightRunnerDriveType)datas[0]);
            }
        }
    }

    #region PUN -> RaiseEvent: Player Called Methods (Offline / Online Mode Supported)
    private void FlightRunnerDriveType_Event(FlightRunnerDriveType driveType)
    {
        object[] datas = new object[] { driveType };

        if (PhotonNetwork.offlineMode)
        {
            UpdateDriveType(driveType);
        }
        else
        {
            RaiseEventOptions options = new RaiseEventOptions()
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.All
            };

            PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.UpdateDriveType, datas, true, options);

        }
    }
    #endregion PUN -> RaiseEvent: Player Called Methods

    #region PUN -> RaiseEvent: Local Methods (For Offline mode or other specific use cases)
    private void UpdateDriveType(FlightRunnerDriveType driveType)
    {
        FlightRunnerDriveType = driveType;
    }
    #endregion PUN -> RaiseEvent: Local Methods

    #endregion PUN: RaiseEvent

    #endregion Main Functions

    #region Unused
    /* Unused
    [PunRPC]
    public void SetKnockBackOverride(bool val, Vector3 newVelocity)
    {
        playerMovementSettings.V_KnockBackOverride = val;
        playerMovementSettings.V_PlayerVelocity = newVelocity;
    }
    */
    #endregion
}