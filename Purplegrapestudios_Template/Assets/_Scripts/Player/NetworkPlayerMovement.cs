using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayerMovement : Photon.MonoBehaviour
{

    private PhotonView PhotonView;
    private Vector3 TargetPosition;
    private Quaternion TargetRotation;
    public float Health;


    public MovementType networkedMovementType;


    Transform tr;
    float currentDistance = 0f;
    float fullDistance = 0f;
    float progress = 0f;
    bool receivedNetworkedGrounded = false;

    Vector3 otherPlayerStartPosition = Vector3.zero;

    //RECEIVED VARIABLES FROM OTHER PLAYERS
    public Quaternion otherPlayerRotation = Quaternion.identity;
    public Vector3 otherPlayerPosition = Vector3.zero;
    public Vector3 otherPlayerVelocity = Vector3.zero;
    public bool otherPlayerFloorDetected = false;
    public bool otherPlayerisRespawnPlayer = false;
    public int otherPlayerHealth;
    public float otherPlayerAimAngle;
    public float otherPlayerCurrentAimAngle;

    [HideInInspector] public Vector3 otherPlayerCurrentPosition;

    //STATE VARIABLES FOR PLAYERANIMATOR PARAMETERS
    public int receivedNetworked_Assault_StateID;
    public int currentNetworked_Assault_StateID;
    public int receivedNetworked_LegRunningStateID;
    public int receivedNetworked_LegStateID;
    public bool receivedNetworked_JumpState;
    public bool receivedNetworked_DeathState;

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

    public PlayerMovement pMovement;
    public PlayerShooting playerShooting;
    public PlayerAnimation playerAnimation;
    public MouseLook mLook;

    //ANIMATOR PARAMETERS (FOR HASHING)
    int AssaultRifleAnimationState;
    int PlayerLegRunningState;
    int PlayerLegState;
    int JumpBool;
    int DeathBool;
    int AssaultRifle_State;
    int AssaultRifleAnimationJumpBool;
    int AssaultRifle_Angle;

    GameObject[] portalCameras;
    GameObject[] portalTeleporters;
    public Transform playerCameraTransform;

    private void Awake()
    {
        PhotonView = GetComponent<PhotonView>();
        tr = GetComponent<Transform>();

        if (pMovement.movementType.Equals(MovementType.Player))
        {
            AssaultRifleAnimationState = Animator.StringToHash("AssaultRifleAnimationState");
            PlayerLegRunningState = Animator.StringToHash("PlayerLegRunningState");
            PlayerLegState = Animator.StringToHash("PlayerLegState");
            JumpBool = Animator.StringToHash("JumpBool");
            DeathBool = Animator.StringToHash("DeathBool");
            AssaultRifle_State = Animator.StringToHash("AssaultRifle_State");
            AssaultRifleAnimationJumpBool = Animator.StringToHash("AssaultRifleAnimationJumpBool");
            AssaultRifle_Angle = Animator.StringToHash("AssaultRifle_Angle");
        }

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
    private int currentHealth;
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
            if (currentHealth != EventManager.Instance.GetScore(photonView.owner.NickName, PlayerStatCodes.Health))
            {
                currentHealth = EventManager.Instance.GetScore(photonView.owner.NickName, PlayerStatCodes.Health);
                PlayerHealthBar.Instance.SetHealth(EventManager.Instance.GetScore(photonView.owner.NickName, PlayerStatCodes.Health));
            }

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
        }

    }

    private void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {


        if (stream.isWriting)
        {
            //We are sending data
            stream.SendNext(pMovement.movementType);

            stream.SendNext(tr.position);
            stream.SendNext(tr.rotation);
            stream.SendNext(pMovement.playerVelocity);
            stream.SendNext(pMovement.floorDetected);
            stream.SendNext(EventManager.Instance.GetScore(photonView.owner.NickName, PlayerStatCodes.Health));

            stream.SendNext(mLook.GetCameraRotationY());

        }
        else
        {
            //We are reading incoming data
            networkedMovementType = (MovementType)stream.ReceiveNext();
            otherPlayerPosition = (Vector3)stream.ReceiveNext();
            otherPlayerRotation = (Quaternion)stream.ReceiveNext();
            otherPlayerVelocity = (Vector3)stream.ReceiveNext();
            otherPlayerFloorDetected = (bool)stream.ReceiveNext();
            otherPlayerHealth = (int)stream.ReceiveNext();

            otherPlayerAimAngle = (float)stream.ReceiveNext();
            //When we receive an update here, our syncTime gets re-initialized to 0
            syncTime = 0f;
            syncDelay = Time.time - lastPackageReceivedTimeStamp;
            lastPackageReceivedTimeStamp = Time.time;

            //Velocity Prediction
            otherPlayerPosition = otherPlayerPosition + (otherPlayerVelocity * syncDelay / 1000) * velocityPredictionValue;

            if (gotFirstUpdate == false)
            {
                tr.position = otherPlayerPosition;
                tr.rotation = otherPlayerRotation;
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

        currentDistance = Vector3.Distance(tr.position, otherPlayerPosition);
        fullDistance = Vector3.Distance(otherPlayerStartPosition, otherPlayerPosition);
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
            tr.position = otherPlayerPosition;
            tr.rotation = otherPlayerRotation;
            //Or a more aggressive smoothing move

        }
        else
        {
            if ((int)otherPlayerVelocity.magnitude == 0)
            {
                if (syncPrecictionValue != 0)
                {

                    tr.rotation = Quaternion.Lerp(tr.rotation, otherPlayerRotation, syncTime / syncDelay);
                    tr.position = Vector3.Lerp(tr.position, otherPlayerPosition, lerpValue);

                    exponentialMultiplier = Mathf.Clamp(Vector3.Distance(tr.position, otherPlayerPosition), 0.0001f, 1);
                    if (exponentialMultiplier < finalSyncDistance)
                    {
                        tr.position = Vector3.LerpUnclamped(tr.position, otherPlayerPosition, progress * finalSyncMultiplier / exponentialMultiplier);
                    }
                    if (syncYAxisValue != 0)
                    {
                        heightLerp = tr.position.y;
                        heightLerp = Mathf.Lerp(tr.position.y, otherPlayerPosition.y, syncYAxisValue);
                        HeightAdjustmentVector = new Vector3(tr.position.x, heightLerp, tr.position.z);
                        tr.position = HeightAdjustmentVector;
                    }
                }
                else
                {
                    tr.position = otherPlayerPosition;
                    tr.rotation = otherPlayerRotation;
                }
            }
            else
            {
                if (syncYAxisValue != 0)
                {
                    heightLerp = tr.position.y;
                    heightLerp = Mathf.Lerp(tr.position.y, otherPlayerPosition.y, syncYAxisValue);
                    HeightAdjustmentVector = new Vector3(tr.position.x, heightLerp, tr.position.z);
                    tr.position = HeightAdjustmentVector;
                }
                tr.rotation = Quaternion.Lerp(tr.rotation, otherPlayerRotation, syncTime / syncDelay);
                tr.position = Vector3.Lerp(tr.position, otherPlayerPosition, progress);
            }
        }

        otherPlayerCurrentPosition = tr.position;

    }

}
