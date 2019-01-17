using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

public class PlayerMovement : MonoBehaviour
{
    [HideInInspector] public MovementType MovementType;
    [HideInInspector] public FlightRunnerInputType FlighRunnerInputType;
    [HideInInspector] public FlightRunnerDriveType FlightRunnerDriveType;

    public SpawnCharacterType SpawnCharacterType;
    public LayerMask RayCastLayersToHit;

    /// <summary>
    /// Basic Components (Player and Car)
    /// </summary>
    private Transform Transform;
    private Transform MainCameraTransform;
    private Rigidbody RigidBody;
    private PhotonView PhotonView;
    /// <summary>
    /// Player Specific Components
    /// </summary>
    private PlayerObjectComponents PlayerObjectComponents;
    private PlayerAnimation PlayerAnimation;
    /// <summary>
    /// Flight Runner Specific Components
    /// </summary>
    private FlightRunnerObjectComponents FlightRunnerObjectComponents;

    /// <summary>
    /// FPS Calculation (For Checking FPS Performance)
    /// </summary>
    private float fpsDisplayRate = 4.0f;  // 4 updates per sec.
    private float frameCount = 0;
    private float dt = 0.0f;
    private float fps = 0.0f;

    /// <summary>
    /// All Strings
    /// </summary>
    private string S_BouncePad = "BouncePad";
    private string S_Level = "Level";
    private string S_MetersPerSecond = " m/s";
    private string S_MouseX = "Mouse X";
    private string S_SpeedRamp = "SpeedRamp";

    /// <summary>
    /// Player Parameters and Values
    /// </summary>
    [System.Serializable]
    public class PlayerMovementSettings
    {
        //Parameters Movement
        public float P_AirAcceleration = 2.0f;                                  // Air accel
        public float P_AirControl = 0.3f;                                       // How precise air control is
        public float P_AirDeacceleration = 2.0f;                                // Deacceleration experienced when opposite strafing
        public float P_DoubleJumpDeltaTime = 0.25f;
        public float P_Friction = 6;                                            // Ground friction
        public float P_Gravity = 20.0f;
        public int P_JumpAttempts = 2;
        public float P_JumpSpeed = 8.0f;                                        // The speed at which the character's up axis gains when hitting jump
        public float P_MaxSpeed = 150f;
        public float P_MoveScale = 1.0f;
        public float P_MoveSpeed = 7.0f;                                        // Ground move speed
        public float P_RunAcceleration = 14;                                    // Ground accel
        public float P_RunDeacceleration = 10;                                  // Deacceleration that occurs when running on the ground
        public float P_SideStrafeAcceleration = 50;                             // How fast acceleration occurs to get up to sideStrafeSpeed when side strafing
        public float P_SideStrafeSpeed = 1;                                     // What the max speed to generate when side strafing
        public float P_WalkSoundRate = 0.15f;
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
    public PlayerMovementSettings playerMovementSettings;
   
    [System.Serializable]
    public class FlightRunnerMovementSettings
    {
        public float P_Gravity;
        public float P_BreakAccelMultiplier;
        public float P_AccelMultiplier;
        public float P_FrictionMultiplier;
        public Dictionary<int, int> D_GearMaxSpeed = new Dictionary<int, int>();
        public LayerMask SlopeLayer;
        [HideInInspector] public Ray[] V_Rays_Ground = new Ray[5];
        [HideInInspector] public RaycastHit[] V_GroundHits;
        [HideInInspector] public RaycastHit lr;
        [HideInInspector] public RaycastHit rr;
        [HideInInspector] public RaycastHit lf;
        [HideInInspector] public RaycastHit rf;
        [HideInInspector] public Vector3 upDir;

        [HideInInspector] public int V_Gear;
        [HideInInspector] public float V_AccelMag;
        [HideInInspector] public float V_BreakMag;
        [HideInInspector] public bool V_IsFourWheelDrive;
        [HideInInspector] public float tiltLerpValue;
        public bool V_IsGrounded;
       
    }
    public FlightRunnerMovementSettings flightRunnerMovementSettings;

    /// <summary>
    /// Contains the command the user wishes upon the character
    /// </summary>
    private class Cmd
    {
        public float forwardmove;
        public float rightmove;
    }
    private Cmd cmd; // Player commands, stores wish commands that the player asks for (Forward, back, jump, etc)

    private class FlightRunnerCmd
    {
        public float steeringWheel;
        public float accelPedal;
        public float breakPedal;
        public int gearNumber;
    }
    private FlightRunnerCmd flightRunnerCmd;


    //SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS
    //SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS
    //SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS - SCRIPT BEGINS

    /// <summary>
    /// Initialization
    /// </summary>
    private void Start()
    {
        PhotonView = GetComponent<PhotonView>();
        RigidBody = GetComponent<Rigidbody>();
        Transform = GetComponent<Transform>();

        // Player / Flight Runner Specifics
        if (SpawnCharacterType.Equals(SpawnCharacterType.Player))
        {
            PlayerObjectComponents = GetComponent<PlayerObjectComponents>();
            MainCameraTransform = PlayerObjectComponents.PlayerCamera.transform;
            PlayerAnimation = GetComponent<PlayerAnimation>();
        }
        if (SpawnCharacterType.Equals(SpawnCharacterType.FlightRunner))
        {
            FlightRunnerObjectComponents = GetComponent<FlightRunnerObjectComponents>();
            MainCameraTransform = FlightRunnerObjectComponents.MainCamera.transform;
        }

        if (!PhotonView.isMine) return;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Player / Flight Runner Specifics
        if (SpawnCharacterType.Equals(SpawnCharacterType.Player))
        {
            cmd = new Cmd();
            playerMovementSettings.V_GroundHits = new RaycastHit[255];
            playerMovementSettings.V_CeilingHits = new RaycastHit[255];
            playerMovementSettings.V_WallHits = new RaycastHit[255];
        }
        if (SpawnCharacterType.Equals(SpawnCharacterType.FlightRunner))
        {
            flightRunnerCmd = new FlightRunnerCmd();
            flightRunnerCmd.gearNumber = 1;
            flightRunnerMovementSettings.V_Gear = flightRunnerCmd.gearNumber;
            flightRunnerMovementSettings.V_GroundHits = new RaycastHit[255];
            FlighRunnerInputType = FlightRunnerInputType.MouseAndKeyboard;
            FlightRunnerDriveType = FlightRunnerDriveType.TwoWheel;

            flightRunnerMovementSettings.D_GearMaxSpeed.Add(-1, 60);
            flightRunnerMovementSettings.D_GearMaxSpeed.Add(1, 20);
            flightRunnerMovementSettings.D_GearMaxSpeed.Add(2, 30);
            flightRunnerMovementSettings.D_GearMaxSpeed.Add(3, 40);
            flightRunnerMovementSettings.D_GearMaxSpeed.Add(4, 50);
            flightRunnerMovementSettings.D_GearMaxSpeed.Add(5, 60);
        }
    }

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
            if (dt > 1.0f / fpsDisplayRate)
            {
                fps = Mathf.Round(frameCount / dt);
                frameCount = 0;
                dt -= 1.0f / fpsDisplayRate;

                PlayerScreenDisplay.Instance.Text_CurrentFrameRate.text = fps.ToString();
                PlayerScreenDisplay.Instance.Text_Ping.text = PhotonNetwork.GetPing().ToString();
                PlayerScreenDisplay.Instance.Text_PlayerVelocity.text = ((int)playerMovementSettings.V_PlayerVelocity.magnitude).ToString() + S_MetersPerSecond;
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

    /// <summary>
    /// Physical Movement of Rigidbody performed in FixedUpdate Loop
    /// </summary>
    private void FixedUpdate()
    {
        if (!PhotonView.isMine) { return; }

        /* MOVEMENT PLAYER: here's the important part*/
        if (SpawnCharacterType.Equals(SpawnCharacterType.Player))
        {
            if (RigidBody.velocity.magnitude > 0) { RigidBody.velocity = Vector3.zero; }

            RigidBody.MovePosition(RigidBody.position + playerMovementSettings.V_PlayerVelocity * Time.fixedDeltaTime);
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

    /// <summary>
    /// Called in Update(). This is the General Player Behavior Loop, for updating values in inputs
    /// </summary>
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

            DustFX_Behavior(playerMovementSettings.V_PlayerVelocity, playerMovementSettings.V_IsGrounded);

            if (playerMovementSettings.V_IsGrounded)
            {
                if (playerMovementSettings.V_IsLanded == false)
                    playerMovementSettings.V_IsLanded = true;
                playerMovementSettings.V_IsJumping = false;
                Player_GroundMove();
            }
            else if (!playerMovementSettings.V_IsGrounded)
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
            if (playerMovementSettings.V_IsGrounded) playerMovementSettings.V_PlayerVelocity.y -= playerMovementSettings.P_Gravity * Time.deltaTime;
            Player_DetectCollision(.5f);                            // 2) Detect Collision Against Wall
            Player_GroundCheck(1f);                                 // 3) Detect Collision Against Ground
            Player_CeilingCheck();                                  // 4) Detect Collision Against Ceiling
            if (playerMovementSettings.V_IsGrounded)
            {
                Player_ApplyFriction(.5f);
            }
        }
    }

    /// <summary>
    /// Detects Player Ground Collision, and initializes multiple rays
    /// </summary>
    /// <param name="distance"></param>
    private void Player_GroundCheck(float distance)
    {
        if (playerMovementSettings.V_KnockBackOverride)
        {
            playerMovementSettings.V_IsGrounded = false;
            return;
        }

        playerMovementSettings.V_TempJumpSpeed = playerMovementSettings.P_JumpSpeed;
        playerMovementSettings.V_TempRampJumpSpeed = playerMovementSettings.P_JumpSpeed * 10;
        if (playerMovementSettings.V_WishJump) { playerMovementSettings.V_TempJumpSpeed *= 10; playerMovementSettings.V_TempRampJumpSpeed *= 2; }
        playerMovementSettings.V_Rays_Ground[0] = new Ray(Transform.position, -Transform.up);
        playerMovementSettings.V_Rays_Ground[1] = new Ray(Transform.position + Transform.forward, -Transform.up);
        playerMovementSettings.V_Rays_Ground[2] = new Ray(Transform.position - Transform.forward, -Transform.up);
        playerMovementSettings.V_Rays_Ground[3] = new Ray(Transform.position + Transform.right, -Transform.up);
        playerMovementSettings.V_Rays_Ground[4] = new Ray(Transform.position - Transform.right, -Transform.up);
        //DO NOT WANT NOT BEING ABLE TO JUMP UNEXPECTEDLY. MUST RAYCAST AT LEAST >= PLAYER HEIGHT=4

        if (!Player_GroundRayCast(playerMovementSettings.V_Rays_Ground, distance))
        {
            playerMovementSettings.V_IsFloorDetected = false;
            playerMovementSettings.V_IsGrounded = false;
            if (playerMovementSettings.V_PlayerVelocity.y < -100f)
            {
                playerMovementSettings.V_IsLanded = false;
            }
        }
    }

    /// <summary>
    /// Sets up multiple rays for ground ray casting
    /// </summary>
    /// <param name="rays"></param>
    /// <param name="dist"></param>
    /// <returns></returns>
    private bool Player_GroundRayCast(Ray[] rays, float dist)
    {
        foreach (Ray ray in rays)
        {
            if (Physics.RaycastNonAlloc(ray, playerMovementSettings.V_GroundHits, dist, RayCastLayersToHit) > 0)
            {
                foreach (RaycastHit hit in playerMovementSettings.V_GroundHits)
                {

                    playerMovementSettings.V_IsFloorDetected = true;
                    playerMovementSettings.V_GroundHit = hit;
                    playerMovementSettings.V_GroundHitTransform = playerMovementSettings.V_GroundHit.transform;
                    playerMovementSettings.v_GroundHitTransformName = playerMovementSettings.V_GroundHitTransform.name;

                    if (hit.transform.CompareTag(S_SpeedRamp))
                    {
                        playerMovementSettings.V_IsGrounded = false;
                        if (!playerMovementSettings.V_IsBoosted)
                        {
                            playerMovementSettings.V_BoostVelocity = Vector3.Cross(playerMovementSettings.V_GroundHit.normal, -playerMovementSettings.V_GroundHit.transform.right) * 75 + playerMovementSettings.V_TempRampJumpSpeed * Transform.up / 10;
                            playerMovementSettings.V_PlayerVelocity = playerMovementSettings.V_BoostVelocity;
                            playerMovementSettings.V_IsBoosted = true;
                            playerMovementSettings.V_IsSliding = true;
                        }
                    }
                    else if (hit.transform.CompareTag(S_BouncePad))
                    {
                        playerMovementSettings.V_IsGrounded = false;
                        if (!playerMovementSettings.V_IsBoosted)
                        {
                            playerMovementSettings.V_BoostVelocity = playerMovementSettings.V_GroundHit.normal * 50 + playerMovementSettings.V_TempJumpSpeed * Transform.up / 10 + new Vector3(playerMovementSettings.V_PlayerVelocity.x, 0, playerMovementSettings.V_PlayerVelocity.z);
                            playerMovementSettings.V_PlayerVelocity = playerMovementSettings.V_BoostVelocity;
                            playerMovementSettings.V_IsBoosted = true;
                            playerMovementSettings.V_IsSliding = false;
                        }

                    }
                    else
                    {
                        if (!playerMovementSettings.V_IsBouncePadWallDetected)
                        {
                            playerMovementSettings.V_IsBoosted = false;
                            playerMovementSettings.V_IsGrounded = true;
                            playerMovementSettings.V_PlayerVelocity.y = 0;
                        }
                        if (playerMovementSettings.V_IsSliding) playerMovementSettings.V_IsSliding = false;
                    }
                    return true;
                }

            }
        }
        return false;
    }

    /// <summary>
    /// Sets Player movement direction bassed on Inputs
    /// </summary>
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

        if (playerMovementSettings.V_IsSliding) return;


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
            playerMovementSettings.P_MoveSpeed = 20;
        }
        else
        {
            playerMovementSettings.P_MoveSpeed = 10;
        }
        //cmd.forwardmove = Input.GetAxis("Vertical");
        //cmd.rightmove   = Input.GetAxis("Horizontal");
    }

    /// <summary>
    /// Schedules a Jump request for Jump Behavior
    /// </summary>
    private void Player_QueueJump()
    {
        if (CameraManager.Instance.spectateMode == CameraManager.SpectateMode.Free)
            return;

        //if (Input.GetKey(KeyCode.Space))
        if (InputManager.Instance.GetKey(InputCode.Jump))
        {
            playerMovementSettings.V_WishJump = true;
        }
        else
        {
            playerMovementSettings.V_WishJump = false;
        }
    }

    /// <summary>
    /// Updates PlayerVelocity while player is in the air
    /// </summary>
    private void Player_AirMove()
    {
        Vector3 wishdir;
        float wishvel = playerMovementSettings.P_AirAcceleration;
        float accel;

        float scale = CmdScale();

        playerMovementSettings.V_KnockBackOverride = false;
        playerMovementSettings.V_IsBoosted = false;

        Player_SetMovementDir();

        wishdir = new Vector3(cmd.rightmove, 0, cmd.forwardmove);
        wishdir = Transform.TransformDirection(wishdir);

        float wishspeed = wishdir.magnitude;
        wishspeed *= playerMovementSettings.P_MoveSpeed;

        wishdir.Normalize();
        playerMovementSettings.V_MoveDirectionNorm = wishdir;
        wishspeed *= scale;

        // CPM: Aircontrol
        float wishspeed2 = wishspeed;
        if (Vector3.Dot(playerMovementSettings.V_PlayerVelocity, wishdir) < 0)
            accel = playerMovementSettings.P_AirDeacceleration;
        else
            accel = playerMovementSettings.P_AirAcceleration;
        // If the player is ONLY strafing left or right
        if (cmd.forwardmove == 0 && cmd.rightmove != 0)
        {
            if (wishspeed > playerMovementSettings.P_SideStrafeSpeed)
                wishspeed = playerMovementSettings.P_SideStrafeSpeed;
            accel = playerMovementSettings.P_SideStrafeAcceleration;
        }

        Player_Accelerate(wishdir, wishspeed, accel);
        if (playerMovementSettings.P_AirControl > 0)
        {
            Player_AirControl(wishdir, wishspeed2);
        }
        // Apply gravity
        playerMovementSettings.V_PlayerVelocity.y -= playerMovementSettings.P_Gravity * Time.deltaTime;
    }

    /// <summary>
    /// Updates PlayerVelocity based on AirControl Parameter Values
    /// </summary>
    /// <param name="wishdir"></param>
    /// <param name="wishspeed"></param>
    private void Player_AirControl(Vector3 wishdir, float wishspeed)
    {
        float zspeed;
        float speed;
        float dot;
        float k;

        // Can't control movement if not moving forward or backward
        if (cmd.forwardmove == 0 || wishspeed == 0)
            return;

        zspeed = playerMovementSettings.V_PlayerVelocity.y;
        playerMovementSettings.V_PlayerVelocity.y = 0;
        /* Next two lines are equivalent to idTech's VectorNormalize() */
        speed = playerMovementSettings.V_PlayerVelocity.magnitude;
        playerMovementSettings.V_PlayerVelocity.Normalize();

        dot = Vector3.Dot(playerMovementSettings.V_PlayerVelocity, wishdir);
        k = 32;
        k *= playerMovementSettings.P_AirControl * dot * dot * Time.deltaTime;

        // Change direction while slowing down
        if (dot > 0)
        {
            playerMovementSettings.V_PlayerVelocity.x = playerMovementSettings.V_PlayerVelocity.x * speed + wishdir.x * k;
            playerMovementSettings.V_PlayerVelocity.y = playerMovementSettings.V_PlayerVelocity.y * speed + wishdir.y * k;
            playerMovementSettings.V_PlayerVelocity.z = playerMovementSettings.V_PlayerVelocity.z * speed + wishdir.z * k;

            playerMovementSettings.V_PlayerVelocity.Normalize();
            playerMovementSettings.V_MoveDirectionNorm = playerMovementSettings.V_PlayerVelocity;
        }

        playerMovementSettings.V_PlayerVelocity.x *= speed;
        playerMovementSettings.V_PlayerVelocity.y = zspeed; // Note this line
        playerMovementSettings.V_PlayerVelocity.z *= speed;

    }

    /// <summary>
    /// Updates PlayerVelocity while player is grounded.
    /// </summary>
    private void Player_GroundMove()
    {
        Vector3 wishdir;

        //set airjump to false again to allow an Extra Jump in Airmove()
        playerMovementSettings.V_Airjump = false;

        // Do not apply friction if the player is queueing up the next jump
        if (!playerMovementSettings.V_WishJump)
        {
            if (playerMovementSettings.V_RaycastFloorType == 1)
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
        playerMovementSettings.V_MoveDirectionNorm = wishdir;
        float wishspeed = wishdir.magnitude;
        wishspeed *= playerMovementSettings.P_MoveSpeed;

        Player_Accelerate(wishdir, wishspeed, playerMovementSettings.P_RunAcceleration);

        // Reset the gravity velocity		
        playerMovementSettings.V_PlayerVelocity.y = 0;

        //SingleJump
        if (playerMovementSettings.V_WishJump && !playerMovementSettings.V_IsHitCeiling)
        {
            playerMovementSettings.V_IsJumping = true;
            playerMovementSettings.V_PlayerVelocity.y = playerMovementSettings.P_JumpSpeed;
            playerMovementSettings.V_IsDoubleJumping = false;

            playerMovementSettings.V_WishJump = false;

            playerMovementSettings.V_IsLanded = false;
        }
    }

    /// <summary>
    /// Detects Player Ceiling Collision
    /// </summary>
    private void Player_CeilingCheck()
    {
        playerMovementSettings.V_Ray_Ceiling = new Ray(Transform.position, Transform.up);

        //DO NOT WANT NOT BEING ABLE TO JUMP UNEXPECTEDLY. MUST RAYCAST AT LEAST >= PLAYER HEIGHT=4
        if (Physics.RaycastNonAlloc(playerMovementSettings.V_Ray_Ceiling, playerMovementSettings.V_CeilingHits, 4f + .1f, RayCastLayersToHit) > 0)
        {
            foreach (RaycastHit hit in playerMovementSettings.V_CeilingHits)
            {
                playerMovementSettings.V_IsHitCeiling = true;
                if (playerMovementSettings.V_PlayerVelocity.y > 0)
                    playerMovementSettings.V_PlayerVelocity.y *= -1;
            }
        }
        else
        {
            playerMovementSettings.V_IsHitCeiling = false;
        }
    }

    /// <summary>
    /// Detects Player Wall Collision
    /// </summary>
    /// <param name="dist"></param>
    private void Player_DetectCollision(float dist)
    {
        playerMovementSettings.V_Ray_Velocity = new Ray(Transform.position, playerMovementSettings.V_PlayerVelocity);
        Debug.DrawRay(Transform.position, playerMovementSettings.V_PlayerVelocity, Color.red);
        playerMovementSettings.V_SpeedReduction = Vector3.zero;

        if (Physics.RaycastNonAlloc(playerMovementSettings.V_Ray_Velocity, playerMovementSettings.V_WallHits, dist, RayCastLayersToHit) > 0)
        {
            foreach (RaycastHit collisionHit in playerMovementSettings.V_WallHits)
            {
                if (collisionHit.collider == null) continue;
                if (collisionHit.transform.CompareTag(S_Level))
                {
                    playerMovementSettings.V_SpeedReduction = collisionHit.normal;
                    float xMag = Mathf.Abs(playerMovementSettings.V_PlayerVelocity.x);
                    float yMag = Mathf.Abs(playerMovementSettings.V_PlayerVelocity.y);
                    float zMag = Mathf.Abs(playerMovementSettings.V_PlayerVelocity.z);

                    playerMovementSettings.V_SpeedReduction.x = playerMovementSettings.V_SpeedReduction.x * xMag * 1.1f;
                    playerMovementSettings.V_SpeedReduction.z = playerMovementSettings.V_SpeedReduction.z * zMag * 1.1f;

                    playerMovementSettings.V_PlayerVelocity.x += playerMovementSettings.V_SpeedReduction.x;
                    playerMovementSettings.V_PlayerVelocity.z += playerMovementSettings.V_SpeedReduction.z;
                }

                if (collisionHit.transform.CompareTag(S_BouncePad))
                {
                    //Debug.Log("HitBouncePadWall");
                    playerMovementSettings.V_IsGrounded = false;
                    //_playerVelocity.y = jumpSpeed * 4;
                    if (!playerMovementSettings.V_IsBouncePadWallDetected)
                    {
                        playerMovementSettings.V_BoostVelocity = collisionHit.normal * 50 + playerMovementSettings.P_JumpSpeed * Transform.up;
                        playerMovementSettings.V_PlayerVelocity = playerMovementSettings.V_BoostVelocity;
                        playerMovementSettings.V_IsBouncePadWallDetected = true;
                    }

                }
                else
                {
                    playerMovementSettings.V_IsBouncePadWallDetected = false;
                }

            }
        }
        else
        {
            playerMovementSettings.V_IsBouncePadWallDetected = false;
        }
    }

    /// <summary>
    /// Controls when the dust FX for player is turned on or off
    /// </summary>
    /// <param name="vel"></param>
    /// <param name="onGround"></param>
    private void DustFX_Behavior(Vector3 vel, bool onGround)
    {
        if (vel.magnitude > 0 && onGround)
        {
            if (!playerMovementSettings.V_IsDisplayDustFX)
            {
                playerMovementSettings.V_IsDisplayDustFX = true;
                DustFX_Activate(playerMovementSettings.V_IsDisplayDustFX);
            }
        }
        else
        {
            if (playerMovementSettings.V_IsDisplayDustFX)
            {
                playerMovementSettings.V_IsDisplayDustFX = false;
                DustFX_Activate(playerMovementSettings.V_IsDisplayDustFX);
            }
        }
    }

    /// <summary>
    /// Turns On or Off Dust FX
    /// </summary>
    /// <param name="val"></param>
    private void DustFX_Activate(bool val)
    {
        PlayerObjectComponents.DustPrefab.SetActive(val);
    }

    /// <summary>
    /// Applies Fiction to the Player
    /// </summary>
    /// <param name="t"></param>
    private void Player_ApplyFriction(float t)
    {
        if (playerMovementSettings.V_IsSliding) return;

        Vector3 vec = playerMovementSettings.V_PlayerVelocity;
        float speed;
        float newspeed;
        float control;
        float drop;

        vec.y = 0.0f;
        speed = vec.magnitude;
        drop = 0.0f;

        /* Only if the player is on the ground then apply friction */
        if (playerMovementSettings.V_IsGrounded)
        {
            control = speed < playerMovementSettings.P_RunDeacceleration ? playerMovementSettings.P_RunDeacceleration : speed;
            drop = control * playerMovementSettings.P_Friction * Time.deltaTime * t;
        }

        newspeed = speed - drop;
        playerMovementSettings.V_PlayerFriction = newspeed;
        if (newspeed < 0)
            newspeed = 0;
        if (speed > 0)
            newspeed /= speed;

        playerMovementSettings.V_PlayerVelocity.x *= newspeed;
        // playerVelocity.y *= newspeed;
        playerMovementSettings.V_PlayerVelocity.z *= newspeed;
    }

    /// <summary>
    /// Calculates wish acceleration based on player's cmd wishes i.e. WASD movement
    /// </summary>
    private void Player_Accelerate(Vector3 wishdir, float wishspeed, float accel)
    {
        float addspeed;
        float accelspeed;
        float currentspeed;

        if (playerMovementSettings.V_PlayerVelocity.magnitude >= 100)
        {
            return;
        }


        currentspeed = Vector3.Dot(playerMovementSettings.V_PlayerVelocity, wishdir);
        addspeed = wishspeed - currentspeed;
        if (addspeed <= 0)
            return;
        accelspeed = accel * Time.deltaTime * wishspeed;
        if (accelspeed > addspeed)
            accelspeed = addspeed;

        playerMovementSettings.V_PlayerVelocity.x += accelspeed * wishdir.x;
        playerMovementSettings.V_PlayerVelocity.z += accelspeed * wishdir.z;

    }

    /// <summary>
    /// Gets Scale of movement
    /// </summary>
    /// <returns></returns>
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
        scale = playerMovementSettings.P_MoveSpeed * max / (playerMovementSettings.P_MoveScale * total);

        return scale;
    }


    /// <summary>
    /// Limits Player Velocity
    /// </summary>
    private void Player_LimitSpeed()
    {
        if (playerMovementSettings.V_IsBoosted || playerMovementSettings.V_IsSliding)
        {
            return;
        }

        if (playerMovementSettings.V_PlayerVelocity.x < -playerMovementSettings.P_MaxSpeed)
        {
            playerMovementSettings.V_PlayerVelocity.x = -playerMovementSettings.P_MaxSpeed;
        }
        if (playerMovementSettings.V_PlayerVelocity.x > playerMovementSettings.P_MaxSpeed)
        {
            playerMovementSettings.V_PlayerVelocity.x = playerMovementSettings.P_MaxSpeed;
        }
        if (playerMovementSettings.V_PlayerVelocity.z < -playerMovementSettings.P_MaxSpeed)
        {
            playerMovementSettings.V_PlayerVelocity.z = -playerMovementSettings.P_MaxSpeed;
        }
        if (playerMovementSettings.V_PlayerVelocity.z > playerMovementSettings.P_MaxSpeed)
        {
            playerMovementSettings.V_PlayerVelocity.z = playerMovementSettings.P_MaxSpeed;
        }

    }

    /// <summary>
    /// Rotates the Player
    /// </summary>
    private void Player_Rotate()
    {
        if (EventManager.Instance.GetScore(PhotonView.owner.NickName, PlayerStatCodes.Health) > 0)
        {
            playerMovementSettings.V_RotationY += Input.GetAxis(S_MouseX) * (playerMovementSettings.V_MouseSensitivity / 3f) * 0.1f * Time.timeScale;           
        }
        Transform.rotation = Quaternion.Euler(0, playerMovementSettings.V_RotationY, 0); // 1) Rotates the collider
    }

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

        flightRunnerCmd.accelPedal = Input.GetAxis("GasInput");
        flightRunnerCmd.breakPedal = Input.GetAxis("BreakInput");
        flightRunnerCmd.steeringWheel = Input.GetAxis("SteeringInput") * 30;

        if (Input.GetButton("Gear1Input"))
        {
            flightRunnerCmd.gearNumber = 1;
        }
        else if (Input.GetButton("Gear2Input"))
        {
            flightRunnerCmd.gearNumber = 2;
        }
        else if (Input.GetButton("Gear3Input"))
        {
            flightRunnerCmd.gearNumber = 3;
        }
        else if (Input.GetButton("Gear4Input"))
        {
            flightRunnerCmd.gearNumber = 4;
        }
        else if (Input.GetButton("Gear5Input"))
        {
            flightRunnerCmd.gearNumber = 5;
        }
        else if (Input.GetButton("ReverseInput"))
        {
            flightRunnerCmd.gearNumber = -1;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            V_IsFlightRunnerBoost = true;
            V_BoostTimer = 0;
        }

        if (V_IsFlightRunnerBoost)
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
                V_IsFlightRunnerBoost = false;
            }
        }

        if (!flightRunnerMovementSettings.V_IsGrounded)
        {
            V_GravityValue = Mathf.Min(V_GravityValue += flightRunnerMovementSettings.P_Gravity * DeltaTime, 40);
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
            }
            else if (FlightRunnerDriveType.Equals(FlightRunnerDriveType.FourWheel))
            {
                FlightRunnerDriveType = FlightRunnerDriveType.TwoWheel;
                foreach (GameObject Wheel in FlightRunnerObjectComponents.Axel_Rear)
                {
                    Wheel.transform.localRotation = Quaternion.Euler(0, 0, 0);
                }
            }
        }

        flightRunnerMovementSettings.V_Gear = flightRunnerCmd.gearNumber;
        flightRunnerCmd.accelPedal = FlighRunnerInputType == FlightRunnerInputType.LogitechG920Wheel ? NormalizeInput(flightRunnerCmd.accelPedal, InputRange.MinusOne_One) : NormalizeInput(flightRunnerCmd.accelPedal, InputRange.Zero_One);
        flightRunnerCmd.breakPedal = FlighRunnerInputType == FlightRunnerInputType.LogitechG920Wheel ? NormalizeInput(flightRunnerCmd.breakPedal, InputRange.MinusOne_One) : NormalizeInput(flightRunnerCmd.breakPedal, InputRange.Zero_One);

        //Flight Runner Acceleration Input Calculations
        if (flightRunnerCmd.accelPedal > 0)
        {
            if (flightRunnerMovementSettings.V_Gear != -1)
            {
                //Forward Movement
                flightRunnerMovementSettings.V_AccelMag = flightRunnerMovementSettings.V_AccelMag < flightRunnerMovementSettings.D_GearMaxSpeed[flightRunnerMovementSettings.V_Gear] ? flightRunnerMovementSettings.V_AccelMag += 5 * flightRunnerCmd.accelPedal * DeltaTime : flightRunnerMovementSettings.D_GearMaxSpeed[flightRunnerMovementSettings.V_Gear];
            }
            else
            {
                //Reverse Movement
                flightRunnerMovementSettings.V_AccelMag = flightRunnerMovementSettings.V_AccelMag > -flightRunnerMovementSettings.D_GearMaxSpeed[flightRunnerMovementSettings.V_Gear] ? flightRunnerMovementSettings.V_AccelMag -= 5 * flightRunnerCmd.accelPedal * DeltaTime : -flightRunnerMovementSettings.D_GearMaxSpeed[flightRunnerMovementSettings.V_Gear];
            }
        }
        //Flight Runner Breaking Input Calculations
        else
        {
            //Break or Friction with Forward Move
            if (flightRunnerMovementSettings.V_AccelMag > 0)
            {
                flightRunnerMovementSettings.V_AccelMag = flightRunnerCmd.breakPedal > 0 ? flightRunnerMovementSettings.V_AccelMag -= 20 * flightRunnerCmd.breakPedal * DeltaTime : flightRunnerMovementSettings.V_AccelMag -= 4 * DeltaTime;
            }
            //Break or Friction with Reverse Move
            else if (flightRunnerMovementSettings.V_AccelMag < 0)
            {
                flightRunnerMovementSettings.V_AccelMag = flightRunnerCmd.breakPedal > 0 ? flightRunnerMovementSettings.V_AccelMag += 20 * flightRunnerCmd.breakPedal * DeltaTime : flightRunnerMovementSettings.V_AccelMag += 4 * DeltaTime;
            }
            //Idle
            else
            {
                flightRunnerMovementSettings.V_AccelMag = 0;
            }
        }
    }

    private void FlightRunner_SpinWheels()
    {
        foreach (GameObject Wheel in FlightRunnerObjectComponents.Wheels_Front)
        {
            Wheel.transform.rotation *= (Quaternion.Euler(0, flightRunnerMovementSettings.V_AccelMag * .35f, 0));
        }
        foreach (GameObject Wheel in FlightRunnerObjectComponents.Wheels_Rear)
        {
            Wheel.transform.rotation *= (Quaternion.Euler(0, flightRunnerMovementSettings.V_AccelMag * .35f, 0));
        }

    }

    private void FlightRunner_RotateWheels()
    {
        foreach (GameObject Wheel in FlightRunnerObjectComponents.Axel_Front)
        {
            Wheel.transform.localRotation = Quaternion.Euler(0, flightRunnerCmd.steeringWheel, 0);
        }
        if (FlightRunnerDriveType.Equals(FlightRunnerDriveType.FourWheel))
        {
            foreach (GameObject Wheel in FlightRunnerObjectComponents.Axel_Rear)
            {
                Wheel.transform.localRotation = Quaternion.Euler(0, -flightRunnerCmd.steeringWheel, 0);
            }
        }
    }

    private void FlightRunner_RotateChasis(float DeltaTime)
    {
        if (flightRunnerMovementSettings.V_Gear != -1)
        {
            if (FlightRunnerDriveType.Equals(FlightRunnerDriveType.TwoWheel))
            {
                FlightRunnerObjectComponents.HeadingObject.transform.rotation *= Quaternion.Euler(0, flightRunnerCmd.steeringWheel * 2 * DeltaTime, 0);
            }
            else
            {
                FlightRunnerObjectComponents.HeadingObject.transform.rotation *= Quaternion.Euler(0, flightRunnerCmd.steeringWheel * 4 * DeltaTime, 0);
            }
        }
        else
        {
            if (FlightRunnerDriveType.Equals(FlightRunnerDriveType.TwoWheel))
            {
                FlightRunnerObjectComponents.HeadingObject.transform.rotation *= Quaternion.Euler(0, -flightRunnerCmd.steeringWheel * 2 * DeltaTime, 0);
            }
            else
            {
                FlightRunnerObjectComponents.HeadingObject.transform.rotation *= Quaternion.Euler(0, -flightRunnerCmd.steeringWheel * 4 * DeltaTime, 0);
            }
        }
    }

    private void FlightRunner_BladesRotate()
    {
        foreach (GameObject Blade in FlightRunnerObjectComponents.BoosterBlades)
        {
            Blade.transform.rotation *= Quaternion.Euler(0, flightRunnerMovementSettings.V_AccelMag * .7f + 5f, 0);
        }
    }

    private void FlightRunner_GroundCheck()
    {
        flightRunnerMovementSettings.V_Rays_Ground[0] = new Ray(Transform.position, -Transform.up);
        flightRunnerMovementSettings.V_Rays_Ground[1] = new Ray(Transform.position + Transform.forward * 4.0f, -Transform.up);
        flightRunnerMovementSettings.V_Rays_Ground[2] = new Ray(Transform.position - Transform.forward * 4.0f, -Transform.up);
        flightRunnerMovementSettings.V_Rays_Ground[3] = new Ray(Transform.position + Transform.right * 4.0f, -Transform.up);
        flightRunnerMovementSettings.V_Rays_Ground[4] = new Ray(Transform.position - Transform.right * 4.0f, -Transform.up);

        flightRunnerMovementSettings.V_IsGrounded = FlightRunner_GroundRaycast(flightRunnerMovementSettings.V_Rays_Ground, 4.0f);
    }

    private bool FlightRunner_GroundRaycast(Ray[] rays, float dist)
    {
        foreach (Ray ray in rays)
        {
            Debug.DrawRay(Transform.position, -Transform.up, Color.red);
            if (Physics.RaycastNonAlloc(ray, flightRunnerMovementSettings.V_GroundHits, dist, RayCastLayersToHit) > 0)
            {
                //Debug.Log("Grounded: " + V_GravityValue + ", VEL: " + RigidBody.velocity.magnitude);
                return true;
            }
        }
        //Debug.Log("In Air: " + V_GravityValue + ", VEL: " + RigidBody.velocity.magnitude);
        return false;
    }

    private Vector3 FlightRunner_GetSlope(Transform tr)
    {
        Physics.Raycast(tr.position - Vector3.forward * 4.1f - (Vector3.right * 4.1f) + Vector3.up * 1, Vector3.down, out flightRunnerMovementSettings.lr, 8, flightRunnerMovementSettings.SlopeLayer);
        Physics.Raycast(tr.position - Vector3.forward * 4.1f + (Vector3.right * 4.1f) + Vector3.up * 1, Vector3.down, out flightRunnerMovementSettings.rr, 8, flightRunnerMovementSettings.SlopeLayer);
        Physics.Raycast(tr.position + Vector3.forward * 4.1f - (Vector3.right * 4.1f) + Vector3.up * 1, Vector3.down, out flightRunnerMovementSettings.lf, 8, flightRunnerMovementSettings.SlopeLayer);
        Physics.Raycast(tr.position + Vector3.forward * 4.1f + (Vector3.right * 4.1f) + Vector3.up * 1, Vector3.down, out flightRunnerMovementSettings.rf, 8, flightRunnerMovementSettings.SlopeLayer);
        flightRunnerMovementSettings.upDir = (Vector3.Cross(flightRunnerMovementSettings.rr.point - Vector3.up * 1, flightRunnerMovementSettings.lr.point - Vector3.up * 1) +
                 Vector3.Cross(flightRunnerMovementSettings.lr.point - Vector3.up * 1, flightRunnerMovementSettings.lf.point - Vector3.up * 1) +
                 Vector3.Cross(flightRunnerMovementSettings.lf.point - Vector3.up * 1, flightRunnerMovementSettings.rf.point - Vector3.up * 1) +
                 Vector3.Cross(flightRunnerMovementSettings.rf.point - Vector3.up * 1, flightRunnerMovementSettings.rr.point - Vector3.up * 1)
                ).normalized;

        Debug.DrawRay(tr.position - Vector3.forward * 4.1f - (Vector3.right * 4.1f) + Vector3.up * 1, Vector3.down, Color.green);
        Debug.DrawRay(tr.position - Vector3.forward * 4.1f + (Vector3.right * 4.1f) + Vector3.up * 1, Vector3.down, Color.green);
        Debug.DrawRay(tr.position + Vector3.forward * 4.1f - (Vector3.right * 4.1f) + Vector3.up * 1, Vector3.down, Color.green);
        Debug.DrawRay(tr.position + Vector3.forward * 4.1f + (Vector3.right * 4.1f) + Vector3.up * 1, Vector3.down, Color.green);

        flightRunnerMovementSettings.upDir = new Vector3(flightRunnerMovementSettings.upDir.x, tr.up.y, flightRunnerMovementSettings.upDir.z);
        return flightRunnerMovementSettings.upDir;
    }

    private void FlightRunner_TiltSlope()
    {
        flightRunnerMovementSettings.tiltLerpValue = Mathf.Max(0.1f, (0.5f - flightRunnerMovementSettings.V_AccelMag / 60));
        Transform.up = Vector3.Lerp(Transform.up, FlightRunner_GetSlope(Transform), flightRunnerMovementSettings.tiltLerpValue);
    }

    bool V_IsFlightRunnerBoost;
    private void FlightRunner_MoveRigidBody()
    {
        if (flightRunnerMovementSettings.V_IsGrounded)
        {
            //Flight Runner Ground Movement
            if (V_IsFlightRunnerBoost)
            {
                //RigidBody.velocity = (FlightRunnerObjectComponents.HeadingObject.transform.forward * flightRunnerMovementSettings.V_AccelMag + Transform.up * flightRunnerMovementSettings.P_Gravity);
                //flightRunnerMovementSettings.V_AccelMag += flightRunnerMovementSettings.P_Gravity * Time.deltaTime;
                flightRunnerMovementSettings.V_AccelMag = Mathf.Min(flightRunnerMovementSettings.V_AccelMag += 200 * Time.deltaTime, 50);
                
                RigidBody.velocity = (FlightRunnerObjectComponents.HeadingObject.transform.forward * flightRunnerMovementSettings.V_AccelMag);
            }
            else
            {
                RigidBody.velocity = (FlightRunnerObjectComponents.HeadingObject.transform.forward * flightRunnerMovementSettings.V_AccelMag);
            }
        }
        else
        {
            //Flight Runner Air Movement
            if (V_IsFlightRunnerBoost)
            {
                //RigidBody.velocity = (FlightRunnerObjectComponents.HeadingObject.transform.forward * flightRunnerMovementSettings.V_AccelMag + Transform.up * flightRunnerMovementSettings.P_Gravity);
                flightRunnerMovementSettings.V_AccelMag = Mathf.Min(flightRunnerMovementSettings.V_AccelMag += 200 * Time.deltaTime, 50);
                //RigidBody.velocity = (FlightRunnerObjectComponents.HeadingObject.transform.forward * flightRunnerMovementSettings.V_AccelMag - Transform.up * flightRunnerMovementSettings.P_Gravity/4);
                RigidBody.velocity = (FlightRunnerObjectComponents.HeadingObject.transform.forward * flightRunnerMovementSettings.V_AccelMag);
            }
            else
            {
                //RigidBody.velocity = (FlightRunnerObjectComponents.HeadingObject.transform.forward * flightRunnerMovementSettings.V_AccelMag - Transform.up * flightRunnerMovementSettings.P_Gravity);
                RigidBody.velocity = (FlightRunnerObjectComponents.HeadingObject.transform.forward * flightRunnerMovementSettings.V_AccelMag - Transform.up * V_GravityValue);
            }
        }
    }

    /// <summary>
    /// Normalizes inputs to a range of 0 to 1
    /// </summary>
    /// <param name="input"></param>
    /// <param name="inputRange"></param>
    /// <returns></returns>
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

    /* Photon Event Items Below Here. */

    /// <summary>
    /// Add PhotonEvent Callbacks
    /// </summary>
    private void OnEnable()
    {
        PhotonNetwork.OnEventCall += PhotonNetwork_OnEventCall;
    }

    /// <summary>
    /// Remove PhotonEvent Callbacks
    /// </summary>
    private void OnDisable()
    {
        PhotonNetwork.OnEventCall -= PhotonNetwork_OnEventCall;
    }

    /// <summary>
    /// PhotonEvent Callbacks
    /// </summary>
    /// <param name="eventCode"></param>
    /// <param name="content"></param>
    /// <param name="senderId"></param>
    private void PhotonNetwork_OnEventCall(byte eventCode, object content, int senderId)
    {
    }



    //SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS
    //SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS
    //SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS - SCRIPT ENDS


    /* Unused
    [PunRPC]
    public void SetKnockBackOverride(bool val, Vector3 newVelocity)
    {
        playerMovementSettings.V_KnockBackOverride = val;
        playerMovementSettings.V_PlayerVelocity = newVelocity;
    }
    */
}