using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MovementType
{
    Player,
    ThirdPersonPlayer,
    BumperCar,
}

public class PlayerMovement : MonoBehaviour {

    public GameObject CameraConainter;

    public MovementType movementType;
    PlayerObjectComponents playerObjectComponents;
    PlayerAnimation playerAnimation;
    public GameObject LocalArms;
    public GameObject PlayerObject;
    public GameObject BumperCarObject;
    public GameObject CameraObject;

    private PhotonView PhotonView;
    private Vector3 TargetPosition;
    private Quaternion TargetRotation;
    public float Health;

    //
    Transform tr;
    float currentDistance = 0f;
    float fullDistance = 0f;
    float progress = 0f;
    Vector3 startPosition = Vector3.zero;
    Vector3 networkedPlayerPosition = Vector3.zero;
    Quaternion networkedPlayerRotation = Quaternion.identity;

    float lerpTime = 1f;
    float currLerpTime = 0f;
    Vector3 networkedPlayerVelocity = Vector3.zero;
    float lastPackageReceivedTimeStamp = 0f;
    float syncTime = 0f;
    float syncDelay = 0f;
    float velocityPredictionValue = 0f;
    float clientPredictionValue = 0f;
    float syncDistanceValue = 0f;
    bool gotFirstUpdate = false;

    float pingInSeconds = 0f;
    float timeSinceLastUpdate = 0f;
    float totalTimePassedSinceLastUpdate = 0f;
    bool isMoving = false;

    PlayerMovement pMovement;
    //
    string speedRamp = "SpeedRamp";
    string bouncePad = "BouncePad";

    /* Player view stuff */
    Transform playerView;  // Must be a camera

    public float playerViewYOffset = 0.6f; // The height at which the camera is bound to
    public float xMouseSensitivity = 30.0f;
    public float yMouseSensitivity = 30.0f;

    /* Frame occuring factors */
    public float gravity = 20.0f;
    public float friction = 6;                // Ground friction

    /* Player stuff */
    public float playerHeight = 0f;

    /* Movement stuff */
    public float moveSpeed = 7.0f;  // Ground move speed
    public float runAcceleration = 14;   // Ground accel
    public float runDeacceleration = 10;   // Deacceleration that occurs when running on the ground
    public float airAcceleration = 2.0f;  // Air accel
    public float airDeacceleration = 2.0f;    // Deacceleration experienced when opposite strafing
    public float airControl = 0.3f;  // How precise air control is
    public float sideStrafeAcceleration = 50;   // How fast acceleration occurs to get up to sideStrafeSpeed when side strafing
    public float sideStrafeSpeed = 1;    // What the max speed to generate when side strafing
    public float jumpSpeed = 8.0f;  // The speed at which the character's up axis gains when hitting jump
    public float moveScale = 1.0f;
    public float walkSoundRate = 0.15f;
    public float maxSpeed = 150f;
    public int jumpAttempts = 2;
    float mouseSensitivityValue = 100f;

    public bool hasJumped = false;
    public bool isStealthWalk = false;

    public float doubleJumpDeltaTime = 0.25f;
    public bool hasDoubleJumped = false;
    /* Sound stuff */

    /* FPS Stuff */
    public float fpsDisplayRate = 4.0f;  // 4 updates per sec.

    /* Prefabs */

    private float frameCount = 0;
    private float dt = 0.0f;
    public float fps = 0.0f;

    private Rigidbody controller;

    // Camera rotationals
    private float rotX = 0.0f;
    private float rotY = 0.0f;

    private Vector3 moveDirection = Vector3.zero;
    private Vector3 moveDirectionNorm = Vector3.zero;
    private Vector3 _playerVelocity = Vector3.zero;
    private float playerTopVelocity = 0.0f;

    // If true then the player is fully on the ground
    private bool grounded = false;
    public bool IsGrounded
    {
        get { return grounded; }
    }
    private bool landed = true;
    public bool IsLanded
    {
        get { return landed; }
    }

    //If in Air, allow Air jump once. Reset when grounded.
    private bool airjump = false;

    // Q3: players can queue the next jump just before he hits the ground
    private bool wishJump = false;

    // Used to display real time friction values
    private float playerFriction = 0.0f;

    // Contains the command the user wishes upon the character
    class Cmd
    {
        public float forwardmove;
        public float rightmove;
        public float upmove;
        public float rotatemove;
    }
    private Cmd cmd; // Player commands, stores wish commands that the player asks for (Forward, back, jump, etc)

    /* Player statuses */

    private Vector3 playerSpawnPos;
    private Quaternion playerSpawnRot;

    public Vector3 playerVelocity
    {
        get
        {
            return _playerVelocity;
        }
        set
        {
            _playerVelocity = value;
        }
    }


    Transform myCamTr;
    PhotonView pv;

    private void Start()
    {
        pv = GetComponent<PhotonView>();
        playerObjectComponents = GetComponent<PlayerObjectComponents>();
        playerAnimation = GetComponent<PlayerAnimation>();
        if (!pv.isMine) return;

        controller = GetComponent<Rigidbody>();
        tr = GetComponent<Transform>();
        myCamTr = GetComponentInChildren<Camera>().transform;
        cmd = new Cmd();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        groundHits = new RaycastHit[255];
        ceilingHits = new RaycastHit[255];
        collisionHits = new RaycastHit[25];
    }


    string AimAngle = "AimAngle";
    void adjustVerticalAimAngle()
    {
        float myAimAngle = 0.0f;
        myAimAngle = myCamTr.rotation.eulerAngles.x <= 90 ? -1 * myCamTr.rotation.eulerAngles.x : 360 - myCamTr.rotation.eulerAngles.x;

        //Set parameter on animator component during runtime
        if (myAimAngle >= 90f)
            myAimAngle = 89f;
        else if (myAimAngle <= -90f)
            myAimAngle = -89f;
    }


    private List<float> rotArrayY = new List<float>();
    float rotAverageX = 0F;
    float playerSmoothingFrames = 0f;
    float bumperCarRotationAcceleration = 0f;

    string inputAxis_Mouse_X = "Mouse X";
    string inputAxis_Joy1_Axis_1 = "Joy1 Axis 1";
    string inputAxis_Joy1_Axis_2 = "Joy1 Axis 2";


    string FPS = "FPS\t";
    string MS = "PING\t";
    string VEL = "SPEED\t";
    string MPS = " m/s";

    private void Update ()
    {
        if (pv.isMine)
        {
            if (InputManager.Instance.GetKeyDown(InputCode.Settings))
            {
                if (Cursor.lockState == CursorLockMode.None)
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    //inputManager.KeybindDialogBox.hasAppliedSettings = true;
                }
                else if (Cursor.lockState == CursorLockMode.Locked)
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                    //inputManager.KeybindDialogBox.hasAppliedSettings = false;
                }
            }

            if (InputManager.Instance.GetKeyDown(InputCode.ToggleBumperCar))
            {
                if (movementType.Equals(MovementType.Player))
                {
                    //playerAnimation.SwitchCameraPerspective();
                    PlayerEvents_ChangeMovementType(pv.owner.ID, MovementType.BumperCar);
                }else
                {
                    //playerAnimation.SwitchCameraPerspective();
                    PlayerEvents_ChangeMovementType(pv.owner.ID, MovementType.Player);
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
                PlayerScreenDisplay.Instance.Text_PlayerVelocity.text = ((int)_playerVelocity.magnitude).ToString() + MPS;
            }

            if (CameraManager.Instance.spectateMode == CameraManager.SpectateMode.Free)
                return;

            /* Using the Mouse Senitivity Inputs DIRECTLY from SettingsManager */
            //Zoom_Vs_NoZoom

            if (EventManager.Instance.GetScore(pv.owner.NickName, PlayerStatCodes.Health) > 0)
            {

                if (movementType.Equals(MovementType.Player))
                {
                    rotY += Input.GetAxis(inputAxis_Mouse_X) * (mouseSensitivityValue / 3f) * 0.1f * Time.timeScale;
                }
                else
                {
                    if (cmd.rotatemove != 0)
                    {
                        rotY += (mouseSensitivityValue / 3f) * bumperCarRotationAcceleration * Mathf.Min(_playerVelocity.magnitude, 40) * Time.timeScale;
                    }

                }
            }

            //Smoothing over X number of Frames
            rotArrayY.Add(rotY);
            if (rotArrayY.Count >= 5)
            {
                rotArrayY.RemoveAt(0);
                //print(tr.rotation);
            }
            for (int i = 0; i < rotArrayY.Count; i++)
            {
                rotAverageX += rotArrayY[i];
            }
            rotAverageX /= rotArrayY.Count;
            //tr.rotation = Quaternion.Euler(0, rotAverageX, 0); // Rotates the collider
        
            tr.rotation = Quaternion.Euler(0, rotY, 0); // Rotates the collider



            detectCollision(2.5f);
            GroundCheck(2.1f);
            CeilingCheck();
            QueueJump();


            if (!playerObjectComponents.playerShooting.isRespawnPlayer)
            {


                GetComponent<CapsuleCollider>().radius = .5f;
                GetComponent<CapsuleCollider>().height = 2f;

                DoDustFX(_playerVelocity, grounded);

                if (grounded)
                {
                    if (landed == false)
                    {
                        //					pv.RPC ("LandFX", PhotonTargets.All);
                        landed = true;
                    }
                    hasJumped = false;
                    GroundMove();
                }
                else if (!grounded)
                {
                    AirMove();
                }

                //FUNCTIONS THAT DO NOT DEPEND ON:
                //-CHAT PANEL
                //-SETTINGS PANEL

                speedLimiter();
            }
            else
            {
                //DEAD SO JUST DROP WITH GRAVITY
                GetComponent<CapsuleCollider>().height = .25f;
                GetComponent<CapsuleCollider>().radius = .25f;
                _playerVelocity.y -= gravity * Time.deltaTime;
                detectCollision(.5f);
                CeilingCheck();
                GroundCheck(1f);
                if (grounded)
                {
                    ApplyFriction(.5f);
                }
            }

        }
        else
        {
            if (playerObjectComponents.networkPlayerMovement.otherPlayerHealth > 0)
            {
                DoDustFX(playerObjectComponents.networkPlayerMovement.otherPlayerVelocity, playerObjectComponents.networkPlayerMovement.otherPlayerFloorDetected);
            }
        }


    }

    void FixedUpdate()
    {

        if (!pv.isMine)
            return;

        //if (Cursor.visible) return;

        if (controller.velocity.magnitude > 0)
        {
            controller.velocity = Vector3.zero;
        }

        adjustVerticalAimAngle();

        /* Movement, here's the important part */
        controller.MovePosition(controller.position + _playerVelocity * Time.fixedDeltaTime);

        /* Calculate top velocity */
        Vector3 udp = _playerVelocity;
        udp.y = 0.0f;
        if (_playerVelocity.magnitude > playerTopVelocity)
            playerTopVelocity = _playerVelocity.magnitude;

    }


    public bool knockBackOverride = false;

    [PunRPC]
    public void SetKnockBackOverride(bool val, Vector3 newVelocity)
    {
        knockBackOverride = val;
        _playerVelocity = newVelocity;
    }


    Vector3 tempVelocity;
    Vector3 boostVelocity;
    public bool isBoosted;
    public bool isSliding;
    public bool floorDetected;
    int scannedFloorType = -1;
    Ray[] rays = new Ray[5];
    Ray ray0, ray1, ray2, ray3, ray4;
    RaycastHit groundHit;
    RaycastHit[] groundHits;
    Transform groundHitTransform;
    float tempJumpSpeed;
    float tempRampJumpSpeed;
    string groundHitTransformName;
    public LayerMask RayCastLayersToHit;
    int nonItemLayerMask = ~(1 << 9);  //Do not hit invisible layer
    void GroundCheck(float distance)
    {

        if (knockBackOverride)
        {
            grounded = false;
            return;
        }

        tempJumpSpeed = jumpSpeed;
        tempRampJumpSpeed = jumpSpeed * 10;
        if (wishJump) { tempJumpSpeed *= 10; tempRampJumpSpeed *= 2; }
        rays[0] = new Ray(tr.position, -tr.up);
        rays[1] = new Ray(tr.position + tr.forward, -tr.up);
        rays[2] = new Ray(tr.position - tr.forward, -tr.up);
        rays[3] = new Ray(tr.position + tr.right, -tr.up);
        rays[4] = new Ray(tr.position - tr.right, -tr.up);
        //DO NOT WANT NOT BEING ABLE TO JUMP UNEXPECTEDLY. MUST RAYCAST AT LEAST >= PLAYER HEIGHT=4

        if (!GroundRayCast(rays, distance))
        {
            floorDetected = false;
            grounded = false;
            if (playerVelocity.y < -100f)
            {
                landed = false;
            }
        }
    }

    private bool GroundRayCast(Ray[] rays, float dist)
    {
        foreach (Ray ray in rays)
        {
            if (Physics.RaycastNonAlloc(ray, groundHits, dist, RayCastLayersToHit) > 0)
            {
                foreach (RaycastHit hit in groundHits)
                {

                    floorDetected = true;
                    groundHit = hit;
                    groundHitTransform = groundHit.transform;
                    groundHitTransformName = groundHitTransform.name;

                    if (hit.transform.CompareTag(speedRamp))
                    {
                        grounded = false;
                        if (!isBoosted)
                        {
                            boostVelocity = Vector3.Cross(groundHit.normal, -groundHit.transform.right) * 75 + tempRampJumpSpeed * tr.up / 10;
                            _playerVelocity = boostVelocity;
                            isBoosted = true;
                            isSliding = true;
                        }
                    }
                    else if (hit.transform.CompareTag(bouncePad))
                    {
                        grounded = false;
                        if (!isBoosted)
                        {
                            boostVelocity = groundHit.normal * 50 + tempJumpSpeed * tr.up / 10 + new Vector3(_playerVelocity.x, 0, _playerVelocity.z);
                            _playerVelocity = boostVelocity;
                            isBoosted = true;
                            isSliding = false;
                        }

                    }
                    else
                    {
                        if (!BouncePadWallDetected)
                        {
                            isBoosted = false;
                            grounded = true;
                            _playerVelocity.y = 0;
                        }
                        if (isSliding) isSliding = false;
                    }
                    return true;
                }

            }
        }
        return false;
    }


    bool isHitCeiling = false;
    Ray ray5;
    RaycastHit[] ceilingHits;
    void CeilingCheck()
    {
        ray5 = new Ray(tr.position, tr.up);

        //DO NOT WANT NOT BEING ABLE TO JUMP UNEXPECTEDLY. MUST RAYCAST AT LEAST >= PLAYER HEIGHT=4
        if (Physics.RaycastNonAlloc(ray5, ceilingHits, 4f + .1f, RayCastLayersToHit) > 0)
        {
            foreach (RaycastHit hit in ceilingHits)
            {
                isHitCeiling = true;
                if (_playerVelocity.y > 0)
                    _playerVelocity.y *= -1;
            }
        }
        else
        {
            isHitCeiling = false;
        }
    }


    /**
 * Sets the movement direction based on player input
 */
    //set the animation of (run forward/strafe left/strafe right) here. We have an animation parameter of type Float, called "HorizontalMovement"

    string animVerticalMovement = "VerticalMovement";
    string animHorizontalMovement = "HorizontalMovement";
    string animJump = "Jump";
    void SetMovementDir()
    {
        if (EventManager.Instance.GetScore(pv.owner.NickName, PlayerStatCodes.Health) <= 0)
            return;

        if (CameraManager.Instance.spectateMode.Equals(CameraManager.SpectateMode.Free))
        {
            cmd.forwardmove = 0;
            cmd.rightmove = 0;
            return;
        }

        if (isSliding) return;

 
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
            if (movementType.Equals(MovementType.Player))
            {
                cmd.rightmove = -1;
            }
            else
            {
                if (_playerVelocity.magnitude!=0f)
                {
                    //if (cmd.rotatemove == 1)
                    //{
                    //    bumperCarRotationAcceleration = 0f;
                    //}

                    cmd.rotatemove = -1;
                    if (bumperCarRotationAcceleration < 0.1f)
                    {
                        bumperCarRotationAcceleration -= 0.00005f;
                    }
                    else
                    {
                        bumperCarRotationAcceleration = 0.1f;
                    }
                }
                else {
                    if (bumperCarRotationAcceleration > 0)
                    {
                        bumperCarRotationAcceleration += 0.001f;
                    }
                    else
                    {
                        bumperCarRotationAcceleration = 0;
                    }
                }
            }
        }
        else if (InputManager.Instance.GetKey(InputCode.Right).Equals(true))
        {
            if (movementType.Equals(MovementType.Player))
            {
                cmd.rightmove = 1;
            }
            else
            {
                if (_playerVelocity.magnitude != 0f)
                {
                    //if (cmd.rotatemove == -1)
                    //{
                    //    bumperCarRotationAcceleration = 0f;
                    //}

                    cmd.rotatemove = 1;
                    if (bumperCarRotationAcceleration > -0.1f)
                    {
                        bumperCarRotationAcceleration += 0.00005f;
                    }
                    else
                    {
                        bumperCarRotationAcceleration = -0.1f;
                    }
                }
                else
                {
                    if (bumperCarRotationAcceleration > 0)
                    {
                        bumperCarRotationAcceleration -= 0.001f;
                    }
                    else
                    {
                        bumperCarRotationAcceleration = 0;
                    }
                }

            }
        }
        else
        {
            cmd.rightmove = 0;
            cmd.rotatemove = 0;
            bumperCarRotationAcceleration = 0f;
        }

        if (movementType.Equals(MovementType.Player))
        {
            if (InputManager.Instance.GetKey(InputCode.Run).Equals(true))
            {
                moveSpeed = 20;
            }
            else
            {
                moveSpeed = 10;
            }
        }
        else
        {
            moveSpeed = 40;
        }
        

        //cmd.forwardmove = Input.GetAxis("Vertical");
        //cmd.rightmove   = Input.GetAxis("Horizontal");
    }

    /**
 * Queues the next jump just like in Q3
 */
    void QueueJump()
    {
        if (CameraManager.Instance.spectateMode == CameraManager.SpectateMode.Free)
            return;


        //if (Input.GetKey(KeyCode.Space))
        if (InputManager.Instance.GetKey(InputCode.Jump))
        {
            wishJump = true;
        }
        else
        {
            wishJump = false;
        }
    }

    /**
 * Execs when the player is in the air
 */
    void AirMove()
    {
        Vector3 wishdir;
        float wishvel = airAcceleration;
        float accel;

        float scale = CmdScale();

        knockBackOverride = false;
        isBoosted = false;

        SetMovementDir();

        wishdir = new Vector3(cmd.rightmove, 0, cmd.forwardmove);
        wishdir = tr.TransformDirection(wishdir);

        float wishspeed = wishdir.magnitude;
        wishspeed *= moveSpeed;

        wishdir.Normalize();
        moveDirectionNorm = wishdir;
        wishspeed *= scale;

        // CPM: Aircontrol
        float wishspeed2 = wishspeed;
        if (Vector3.Dot(_playerVelocity, wishdir) < 0)
            accel = airDeacceleration;
        else
            accel = airAcceleration;
        // If the player is ONLY strafing left or right
        if (cmd.forwardmove == 0 && cmd.rightmove != 0)
        {
            if (wishspeed > sideStrafeSpeed)
                wishspeed = sideStrafeSpeed;
            accel = sideStrafeAcceleration;
        }

        Accelerate(wishdir, wishspeed, accel);
        if (airControl > 0)
        {
            AirControl(wishdir, wishspeed2);
        }// !CPM: Aircontrol

        //		if (!airjump) {
        //			if (kMgr.GetKeyDownPublic(keyType.jump)) {
        //				_playerVelocity.y = jumpSpeed;
        //				pv.RPC("JumpFX",PhotonTargets.All, _playerVelocity.magnitude);
        //				airjump = true;
        //			}
        //		}

        // Apply gravity
        _playerVelocity.y -= gravity * Time.deltaTime;
    }

    void AirControl(Vector3 wishdir, float wishspeed)
    {
        float zspeed;
        float speed;
        float dot;
        float k;
        int i;

        // Can't control movement if not moving forward or backward
        if (cmd.forwardmove == 0 || wishspeed == 0)
            return;

        zspeed = _playerVelocity.y;
        _playerVelocity.y = 0;
        /* Next two lines are equivalent to idTech's VectorNormalize() */
        speed = _playerVelocity.magnitude;
        _playerVelocity.Normalize();

        dot = Vector3.Dot(_playerVelocity, wishdir);
        k = 32;
        k *= airControl * dot * dot * Time.deltaTime;

        // Change direction while slowing down
        if (dot > 0)
        {
            _playerVelocity.x = _playerVelocity.x * speed + wishdir.x * k;
            _playerVelocity.y = _playerVelocity.y * speed + wishdir.y * k;
            _playerVelocity.z = _playerVelocity.z * speed + wishdir.z * k;

            _playerVelocity.Normalize();
            moveDirectionNorm = _playerVelocity;
        }

        _playerVelocity.x *= speed;
        _playerVelocity.y = zspeed; // Note this line
        _playerVelocity.z *= speed;

    }

    /**
 * Called every frame when the engine detects that the player is on the ground
 */
    void GroundMove()
    {
        Vector3 wishdir;
        Vector3 wishvel;


        //set airjump to false again to allow an Extra Jump in Airmove()
        airjump = false;

        // Do not apply friction if the player is queueing up the next jump
        if (!wishJump)
        {
            if (scannedFloorType == 1)
            {
                ApplyFriction(0f);
            }
            else
            {
                ApplyFriction(1f);

            }
            //			if (_playerVelocity.x < -maxSpeed/2) {
            //				_playerVelocity.x = -maxSpeed/2;
            //			}
            //			else if (_playerVelocity.x > maxSpeed/2) {
            //				_playerVelocity.x = maxSpeed/2;
            //			}
            //			if (_playerVelocity.z < -maxSpeed/2) {
            //				_playerVelocity.z = -maxSpeed/2;
            //			}
            //			else if (_playerVelocity.z > maxSpeed/2) {
            //				_playerVelocity.z = maxSpeed/2;
            //			}
        }
        else
        {
            ApplyFriction(0);
        }

        float scale = CmdScale();

        SetMovementDir();

        wishdir = new Vector3(cmd.rightmove, 0, cmd.forwardmove);
        wishdir = tr.TransformDirection(wishdir);
        wishdir.Normalize();
        moveDirectionNorm = wishdir;
        float wishspeed = wishdir.magnitude;
        wishspeed *= moveSpeed;

        Accelerate(wishdir, wishspeed, runAcceleration);

        // Reset the gravity velocity		
        _playerVelocity.y = 0;

        //SingleJump
        if (wishJump && !isHitCeiling)
        {

            hasJumped = true;
            _playerVelocity.y = jumpSpeed;
            hasDoubleJumped = false;

            wishJump = false;

            landed = false;
            //			pv.RPC("JumpFX",PhotonTargets.All, _playerVelocity.magnitude);
            //			doubleJumpDeltaTime = 0f;
        }


    }

    [HideInInspector] public bool doDustFX;
    public void DoDustFX(Vector3 vel, bool onGround)
    {
        if (vel.magnitude > 0 && onGround)
        {
            if (!doDustFX)
            {
                doDustFX = true;
                //EventManager.Instance.DustFX_Event(doDustFX);
                DustFX_Local(doDustFX);
            }
        }
        else
        {
            if (doDustFX)
            {
                doDustFX = false;
                //EventManager.Instance.DustFX_Event(doDustFX);
                DustFX_Local(doDustFX);
            }
        }
    }

    public void DustFX_Local(bool val)
    {
        playerObjectComponents.DustPrefab.SetActive(val);
    }

    /**
 * Applies friction to the player, called in both the air and on the ground
 */
    public void ApplyFriction(float t)
    {
        if (isSliding) return;

        Vector3 vec = _playerVelocity; // Equivalent to: VectorCopy();
        float vel;
        float speed;
        float newspeed;
        float control;
        float drop;

        vec.y = 0.0f;
        speed = vec.magnitude;
        drop = 0.0f;

        /* Only if the player is on the ground then apply friction */
        if (grounded)
        {
            control = speed < runDeacceleration ? runDeacceleration : speed;
            drop = control * friction * Time.deltaTime * t;
        }

        newspeed = speed - drop;
        playerFriction = newspeed;
        if (newspeed < 0)
            newspeed = 0;
        if (speed > 0)
            newspeed /= speed;

        _playerVelocity.x *= newspeed;
        // playerVelocity.y *= newspeed;
        _playerVelocity.z *= newspeed;
    }


    /**
 * Calculates wish acceleration based on player's cmd wishes i.e. WASD movement
 */
    void Accelerate(Vector3 wishdir, float wishspeed, float accel)
    {
        float addspeed;
        float accelspeed;
        float currentspeed;

        if (_playerVelocity.magnitude >= 100)
        {
            return;
        }


        currentspeed = Vector3.Dot(_playerVelocity, wishdir);
        addspeed = wishspeed - currentspeed;
        if (addspeed <= 0)
            return;
        accelspeed = accel * Time.deltaTime * wishspeed;
        if (accelspeed > addspeed)
            accelspeed = addspeed;

        _playerVelocity.x += accelspeed * wishdir.x;
        _playerVelocity.z += accelspeed * wishdir.z;

    }

    float CmdScale()
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
        scale = moveSpeed * max / (moveScale * total);

        return scale;
    }


    Ray ray6;
    Vector3 speedReduction = Vector3.zero;
    bool BouncePadWallDetected;
    Vector3 horizontalPlaneVelocity;
    float speedReductionMultiplier = 1.1f;
    RaycastHit[] collisionHits;
    RaycastHit collisionHit;

    string s_Level = "Level";
    void detectCollision(float dist)
    {
        ray6 = new Ray(tr.position, playerVelocity);
        Debug.DrawRay(tr.position, playerVelocity, Color.red);
        speedReduction = Vector3.zero;

        if (Physics.Raycast(ray6, out collisionHit, dist, RayCastLayersToHit))
        {
            //foreach (RaycastHit hit in collisionHits)
            //{
                if (collisionHit.transform.CompareTag(s_Level))
                {

                    speedReduction = collisionHit.normal;

                    float xMag = Mathf.Abs(_playerVelocity.x);
                    float yMag = Mathf.Abs(_playerVelocity.y);
                    float zMag = Mathf.Abs(_playerVelocity.z);

                    speedReduction.x = speedReduction.x * xMag * 1.1f;
                    //speedReduction.y = speedReduction.y * yMag * 1.1f;
                    speedReduction.z = speedReduction.z * zMag * 1.1f;

                    _playerVelocity.x += speedReduction.x;
                    //_playerVelocity.y += speedReduction.y;
                    _playerVelocity.z += speedReduction.z;

                }

                if (collisionHit.transform.CompareTag(bouncePad))
                {
                    //Debug.Log("HitBouncePadWall");
                    grounded = false;
                    //_playerVelocity.y = jumpSpeed * 4;
                    if (!BouncePadWallDetected)
                    {
                        boostVelocity = collisionHit.normal * 50 + jumpSpeed * tr.up;
                        _playerVelocity = boostVelocity;
                        BouncePadWallDetected = true;
                    }

                }
                else
                {
                    BouncePadWallDetected = false;
                }
            //}

        }
        else
        {
            BouncePadWallDetected = false;
        }

    }

    void speedLimiter()
    {
        if (isBoosted || isSliding)
        {
            return;
        }

        if (_playerVelocity.x < -maxSpeed)
        {
            _playerVelocity.x = -maxSpeed;
        }
        if (_playerVelocity.x > maxSpeed)
        {
            _playerVelocity.x = maxSpeed;
        }
        if (_playerVelocity.z < -maxSpeed)
        {
            _playerVelocity.z = -maxSpeed;
        }
        if (_playerVelocity.z > maxSpeed)
        {
            _playerVelocity.z = maxSpeed;
        }

    }


    private void OnEnable()
    {
        PhotonNetwork.OnEventCall += PhotonNetwork_OnEventCall;
    }

    private void OnDisable()
    {
        PhotonNetwork.OnEventCall -= PhotonNetwork_OnEventCall;
    }

    private void PhotonNetwork_OnEventCall(byte eventCode, object content, int senderId)
    {
        PhotonEventCodes code = (PhotonEventCodes)eventCode;

        if (code == PhotonEventCodes.ChangeMovementType)
        {
            object[] datas = content as object[];
            if (datas.Length == 2)
            {
                ChangeMovementType((int)datas[0], (MovementType)datas[1]);
            }
        }

    }

    private void ChangeMovementType(int photonOwnerID, MovementType value)
    {
        if (photonOwnerID == pv.ownerId)
        {
            movementType = value;
            if (movementType.Equals(MovementType.Player))
            {
                GetComponent<CapsuleCollider>().radius = 0.5f;

                if (playerAnimation.playerCameraView.Equals(PlayerCameraView.FirstPerson))
                {
                    playerObjectComponents.ThirdPersonPlayer.SetActive(false);
                    playerObjectComponents.FirstPersonPlayer.SetActive(true);
                }
                else {
                    playerObjectComponents.ThirdPersonPlayer.SetActive(true);
                    playerObjectComponents.FirstPersonPlayer.SetActive(false);
                }
                BumperCarObject.SetActive(false);

                CameraObject.transform.localPosition = new Vector3(0, .75f, 0);
                CameraObject.transform.localRotation = Quaternion.identity;
                CameraObject.transform.Rotate(new Vector3(0, 0, 0));

                maxSpeed = 40;
                
                friction = 2;
                runAcceleration = 120;
                runDeacceleration = 120;
                airAcceleration = 60;
                airDeacceleration = 60;

            }
            else if (movementType.Equals(MovementType.BumperCar))
            {
                GetComponent<CapsuleCollider>().radius = 1f;

                PlayerObject.SetActive(false);
                LocalArms.SetActive(false);

                BumperCarObject.SetActive(true);

                CameraObject.transform.localPosition = new Vector3(0, 1.5f, -1.5f);
                CameraObject.transform.localRotation = Quaternion.identity;
                CameraObject.transform.Rotate(new Vector3(30, 0, 0));

                maxSpeed = 75;

                friction = 1f;
                runAcceleration = 1;
                runDeacceleration = 1;
                airAcceleration = 1;
                airDeacceleration = 1;
            }
        }
    }


    //Called By Player
    public void PlayerEvents_ChangeMovementType(int photonOwnerID, MovementType movementType)
    {
        object[] datas = new object[] { photonOwnerID, movementType };

        RaiseEventOptions options = new RaiseEventOptions()
        {
            CachingOption = EventCaching.DoNotCache,
            Receivers = ReceiverGroup.All
        };

        PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.ChangeMovementType, datas, true, options);
    }

}
