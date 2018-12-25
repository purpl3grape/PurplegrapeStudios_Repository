using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkBumperCarMovement : Photon.MonoBehaviour
{

    private PhotonView PhotonView;
    private Vector3 TargetPosition;
    private Quaternion TargetRotation;
    public float Health;




    Transform tr;
    float currentDistance = 0f;
    float fullDistance = 0f;
    float progress = 0f;
    Vector3 startPosition = Vector3.zero;
    Vector3 networkedPosition = Vector3.zero;
    Quaternion networkedRotation = Quaternion.identity;
    Vector3 networkedVelocity = Vector3.zero;
    bool receivedNetworkedGrounded = false;
    bool receivedNetworkedFloorDetected = false;
    float receivedNetworked_Assault_AimAngle;
    int receivedNetworked_Assault_StateID;
    int currentNetworked_Assault_StateID;
    int receivedNetworked_LegStateID;
    bool receivedNetworked_JumpState;
    float myCurrentAssault_AimAngle;
    int myNetworked_LegStateID;
    int myFirstPerson_UpperBodyAssaultStateID;
    bool myNetworked_JumpState;

    float lerpTime = 1f;
    float currLerpTime = 0f;
    float lastPackageReceivedTimeStamp = 0f;
    float syncTime = 0f;
    float syncDelay = 0f;
    public float velocityPredictionValue = 0f;
    public float clientPredictionValue = 15f;
    public float syncPrecictionValue = 0f;
    public float syncDistanceValue = 100f;
    public float syncYAxisValue = 0f;

    float lerpValue;
    float heightLerp;
    Vector3 HeightAdjustmentVector;

    public float finalSyncDistance;
    public float finalSyncMultiplier;
    private float exponentialMultiplier;


    bool gotFirstUpdate = false;

    float pingInSeconds = 0f;
    float timeSinceLastUpdate = 0f;
    float totalTimePassedSinceLastUpdate = 0f;
    bool isMoving = false;

    public Animator anim;
    public Animator animAssaultRifle;
    PlayerMovement pMovement;
    public MouseLook mLook;

    //ANIMATOR PARAMETERS (FOR HASHING)
    int AssaultRifleAnimationState;
    int PlayerLegState;
    int JumpBool;
    int AssaultRifle_State;
    int AssaultRifleAnimationJumpBool;
    int AssaultRifle_Angle;

    GameObject[] portalCameras;
    GameObject[] portalTeleporters;
    public Transform playerCameraTransform;

    private void Awake()
    {
        PhotonView = GetComponent<PhotonView>();
        pMovement = GetComponent<PlayerMovement>();
        tr = GetComponent<Transform>();

        //AssaultRifleAnimationState = Animator.StringToHash("AssaultRifleAnimationState");
        //PlayerLegState = Animator.StringToHash("PlayerLegState");
        //JumpBool = Animator.StringToHash("JumpBool");
        //AssaultRifle_State = Animator.StringToHash("AssaultRifle_State");
        //AssaultRifleAnimationJumpBool = Animator.StringToHash("AssaultRifleAnimationJumpBool");
        //AssaultRifle_Angle = Animator.StringToHash("AssaultRifle_Angle");

        if (PhotonView.isMine)
        {
            portalCameras = GameObject.FindGameObjectsWithTag("PortalCamera");
            foreach (GameObject portalCamera in portalCameras)
            {
                portalCamera.GetComponent<PortalCamera>().playerCamera = playerCameraTransform;
            }

            portalTeleporters = GameObject.FindGameObjectsWithTag("PortalCollider");
            foreach (GameObject portalTeleporter in portalTeleporters)
            {
                portalTeleporter.GetComponent<PortalTeleporter>().player = tr;
            }
        }

        NetworkSettingsDisplay.Instance.InitNetworkSettings();
    }

    bool playerInitNetworkSettings;
    void Update()
    {
        clientPredictionValue = NetworkSettingsDisplay.Instance.PredictionMultiplierValue;
        velocityPredictionValue = NetworkSettingsDisplay.Instance.VelocityMultiplierValue;
        syncPrecictionValue = NetworkSettingsDisplay.Instance.SyncMultiplierValue;
        syncDistanceValue = NetworkSettingsDisplay.Instance.SyncDistanceValue;
        syncYAxisValue = NetworkSettingsDisplay.Instance.SyncYAxisValue;
        finalSyncDistance = NetworkSettingsDisplay.Instance.FinalSyncDistanceValue;
        finalSyncMultiplier = NetworkSettingsDisplay.Instance.FinalSyncMultiplierValue;


        if (PhotonView.isMine)
        {
            //Our Animation
            //TestAnimationInputs();
            //

            if (Cursor.visible)
            {
                GameCanvas.Instance.gameObject.SetActive(true);
            }
            else
            {
                GameCanvas.Instance.gameObject.SetActive(false);
            }
        }
        else
        {

            SmoothMoove();

            /*/Received Networked Animations
            myCurrentAssault_AimAngle = Mathf.Lerp(myCurrentAssault_AimAngle, receivedNetworked_Assault_AimAngle, .2f);
            SetNetworkedAssaultRifleAngle(myCurrentAssault_AimAngle / 75);

            if (!receivedNetworkedFloorDetected)
            {
                if (receivedNetworked_JumpState != true)
                {
                    receivedNetworked_JumpState = true;
                    SetNetworkedJumpAnimationState(receivedNetworked_JumpState);
                }
            }
            else
            {
                if (receivedNetworked_JumpState != false)
                {
                    receivedNetworked_JumpState = false;
                    SetNetworkedJumpAnimationState(false);
                }
                if (networkedVelocity.magnitude > 15)
                {
                    if (receivedNetworked_LegStateID != 1)
                    {
                        receivedNetworked_LegStateID = 1;
                        SetNetworkedLegAnimationState(receivedNetworked_LegStateID);   //Networked PlayerLeg Mesh Anim (Received)
                    }
                }
                else if (networkedVelocity.magnitude <= 15 && networkedVelocity.magnitude > 0)
                {
                    if (receivedNetworked_LegStateID != 1)
                    {
                        receivedNetworked_LegStateID = 1;
                        SetNetworkedLegAnimationState(receivedNetworked_LegStateID);   //Networked PlayerLeg Mesh Anim (Received)
                    }
                }
                else
                {
                    if (receivedNetworked_LegStateID != 0)
                    {
                        receivedNetworked_LegStateID = 0;
                        SetNetworkedLegAnimationState(receivedNetworked_LegStateID);   //Networked PlayerLeg Mesh Anim (Received)
                    }
                }
            }

            //To Make Class for PlayerShooting
            if (currentNetworked_Assault_StateID != receivedNetworked_Assault_StateID)
            {
                currentNetworked_Assault_StateID = receivedNetworked_Assault_StateID;
                SetNetworkedAssaultRifleAnimationState(currentNetworked_Assault_StateID);   //Networked PlayerArm Mesh Anim (Received)
            }
            /*/

        }

    }

    private void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {


        if (stream.isWriting)
        {
            //We are sending data
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(pMovement.playerVelocity);
            //stream.SendNext(pMovement.IsGrounded);
            stream.SendNext(pMovement.floorDetected);

            //stream.SendNext(mLook.GetCameraRotationY());
            //stream.SendNext(myFirstPerson_UpperBodyAssaultStateID);
        }
        else
        {
            //We are reading incoming data
            networkedPosition = (Vector3)stream.ReceiveNext();
            networkedRotation = (Quaternion)stream.ReceiveNext();
            networkedVelocity = (Vector3)stream.ReceiveNext();
            //receivedNetworkedGrounded = (bool)stream.ReceiveNext();
            receivedNetworkedFloorDetected = (bool)stream.ReceiveNext();

            //receivedNetworked_Assault_AimAngle = (float)stream.ReceiveNext();
            //receivedNetworked_Assault_StateID = (int)stream.ReceiveNext();

            //When we receive an update here, our syncTime gets re-initialized to 0
            syncTime = 0f;
            syncDelay = Time.time - lastPackageReceivedTimeStamp;
            lastPackageReceivedTimeStamp = Time.time;

            //Velocity Prediction
            networkedPosition = networkedPosition + (networkedVelocity * syncDelay / 1000) * velocityPredictionValue;

            if (gotFirstUpdate == false)
            {
                tr.position = networkedPosition;
                tr.rotation = networkedRotation;
                gotFirstUpdate = true;
            }

        }
    }



    private void SmoothMoove()
    {
        //transform.position = Vector3.Lerp(transform.position, TargetPosition, 0.25f);
        //transform.rotation = Quaternion.RotateTowards(transform.rotation, TargetRotation, 500 * Time.deltaTime);

        pingInSeconds = (float)PhotonNetwork.GetPing() * 0.001f;
        timeSinceLastUpdate = (float)(PhotonNetwork.time - lastPackageReceivedTimeStamp);
        totalTimePassedSinceLastUpdate = pingInSeconds + timeSinceLastUpdate;

        currentDistance = Vector3.Distance(tr.position, networkedPosition);
        fullDistance = Vector3.Distance(startPosition, networkedPosition);
        if (fullDistance != 0)
        {
            progress = currentDistance / fullDistance;
            lerpValue = progress;
        }

        if (clientPredictionValue != 0)
        {
            progress /= clientPredictionValue;
            //progress = Mathf.Pow(progress, clientPredictionValue);
        }
        if (syncPrecictionValue != 0)
        {
            lerpValue /= syncPrecictionValue;
        }

        syncTime += Time.deltaTime;

        //lerpValue = Mathf.Pow(syncTime / syncDelay, 5);
        if (currentDistance > syncDistanceValue)
        {
            tr.position = networkedPosition;
            tr.rotation = networkedRotation;
            //Or a more aggressive smoothing move

        }
        else
        {
            if ((int)networkedVelocity.magnitude == 0)
            {
                if (syncPrecictionValue != 0)
                {

                    tr.rotation = Quaternion.Lerp(tr.rotation, networkedRotation, syncTime / syncDelay);
                    tr.position = Vector3.Lerp(tr.position, networkedPosition, lerpValue);
                    //print("Sync1" + syncPrecictionValue);
                    exponentialMultiplier = Mathf.Clamp(Vector3.Distance(tr.position, networkedPosition), 0.0001f, 1);
                    if (exponentialMultiplier < finalSyncDistance)
                    {
                        tr.position = Vector3.LerpUnclamped(tr.position, networkedPosition, progress * finalSyncMultiplier / exponentialMultiplier);
                    }
                    if (syncYAxisValue != 0)
                    {
                        heightLerp = tr.position.y;
                        heightLerp = Mathf.Lerp(tr.position.y, networkedPosition.y, syncYAxisValue);
                        HeightAdjustmentVector = new Vector3(tr.position.x, heightLerp, tr.position.z);
                        tr.position = HeightAdjustmentVector;
                    }
                }
                else
                {
                    tr.position = networkedPosition;
                    tr.rotation = networkedRotation;
                    //print("Sync2");
                }
            }
            else
            {
                if (syncYAxisValue != 0)
                {
                    heightLerp = tr.position.y;
                    heightLerp = Mathf.Lerp(tr.position.y, networkedPosition.y, syncYAxisValue);
                    HeightAdjustmentVector = new Vector3(tr.position.x, heightLerp, tr.position.z);
                    tr.position = HeightAdjustmentVector;
                }
                tr.rotation = Quaternion.Lerp(tr.rotation, networkedRotation, syncTime / syncDelay);
                tr.position = Vector3.Lerp(tr.position, networkedPosition, progress);
            }
        }

    }

    //Our AR Arms and Upperbody State to Send across Network
    //Local Player AR Arms Animated here

    int frameLimiterAnimations = 0; //Do once every 2 updates
    void TestAnimationInputs()
    {
        //frameLimiterAnimations++;
        //if (frameLimiterAnimations > 1)
        //{
        //    frameLimiterAnimations = 0;
        //    return;
        //}


        if (InputManager.Instance.GetKey(InputCode.Shoot))
        {
            if (InputManager.Instance.GetKey(InputCode.Aim))
            {
                if (myFirstPerson_UpperBodyAssaultStateID != 3)
                {
                    myFirstPerson_UpperBodyAssaultStateID = 3;
                    SetLocalAssaultRifleAnimationState(myFirstPerson_UpperBodyAssaultStateID);  //Local AR AimFire
                }
            }
            else
            {
                if (myFirstPerson_UpperBodyAssaultStateID != 2)
                {
                    myFirstPerson_UpperBodyAssaultStateID = 2;
                    SetLocalAssaultRifleAnimationState(myFirstPerson_UpperBodyAssaultStateID);  //Local AR Fire
                }
            }
        }
        else if (InputManager.Instance.GetKey(InputCode.Aim))
        {
            if (myFirstPerson_UpperBodyAssaultStateID != 1)
            {
                myFirstPerson_UpperBodyAssaultStateID = 1;
                SetLocalAssaultRifleAnimationState(myFirstPerson_UpperBodyAssaultStateID);  //Local AR Aim
            }
        }
        else
        {
            if (myNetworked_LegStateID != 0)
            {
                myNetworked_LegStateID = 0;
            }

            if (myFirstPerson_UpperBodyAssaultStateID != 0)
            {
                myFirstPerson_UpperBodyAssaultStateID = 0;
                SetLocalAssaultRifleAnimationState(myFirstPerson_UpperBodyAssaultStateID);  //Local AR Idle
            }
            if (pMovement.floorDetected)
            {
                if (myNetworked_JumpState != false)
                {
                    myNetworked_JumpState = false;
                }

                if (pMovement.playerVelocity.magnitude > 15)
                {
                    if (myNetworked_LegStateID != 1)
                    {
                        myNetworked_LegStateID = 1;
                    }

                    if (myFirstPerson_UpperBodyAssaultStateID != 5)
                    {
                        myFirstPerson_UpperBodyAssaultStateID = 5;   //Networked AR Run
                        SetLocalAssaultRifleAnimationState(myFirstPerson_UpperBodyAssaultStateID);  //Local AR Run
                    }
                }
                else if (pMovement.playerVelocity.magnitude <= 15 && pMovement.playerVelocity.magnitude > 0)
                {
                    if (myNetworked_LegStateID != 1)
                    {
                        myNetworked_LegStateID = 1;
                    }

                    if (myFirstPerson_UpperBodyAssaultStateID != 4)
                    {
                        myFirstPerson_UpperBodyAssaultStateID = 4;   //Networked AR Walk
                        SetLocalAssaultRifleAnimationState(myFirstPerson_UpperBodyAssaultStateID);  //Local AR Walk
                    }
                }
                else
                {
                    if (myNetworked_LegStateID != 0)
                    {
                        myNetworked_LegStateID = 0;
                    }

                    if (myFirstPerson_UpperBodyAssaultStateID != 0)
                    {
                        myFirstPerson_UpperBodyAssaultStateID = 0;   //Networked AR Idle
                        //SetLocalAssaultRifleAnimationState(myFirstPerson_UpperBodyAssaultStateID);  //Local AR Idle
                    }
                }
            }
            else
            {
                //currentUpperBodyAssaultState = 6;
                if (myNetworked_JumpState != true)
                {
                    myNetworked_JumpState = true;
                }

            }

        }

    }


    void SetLocalAssaultRifleAnimationState(int val)
    {
        animAssaultRifle.SetInteger(AssaultRifleAnimationState, val);
        myNetworked_LegStateID = val;
    }

    void SetNetworkedLegAnimationState(int val)
    {
        anim.SetFloat(PlayerLegState, val);
    }

    void SetNetworkedJumpAnimationState(bool val)
    {
        anim.SetBool(JumpBool, val);
    }

    void SetNetworkedAssaultRifleAnimationState(int val)
    {
        anim.SetInteger(AssaultRifle_State, val);
    }


    void SetNetworkedAssaultRifleJumpState(bool val)
    {
        anim.SetBool(AssaultRifleAnimationJumpBool, val);
    }

    void SetNetworkedAssaultRifleAngle(float val)
    {
        anim.SetFloat(AssaultRifle_Angle, val);
    }

}
