using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NetworkPlayerMovement : Photon.MonoBehaviour
{
    public SpawnCharacterType SpawnCharacterType;

    //Networked Players Received Variables for Interpolating Movement, as well as Animating
    private SpawnCharacterType NetworkSpawnCharacterType;
    private Quaternion NetworkPlayerRotation = Quaternion.identity;
    private Vector3 NetworkPlayerPosition = Vector3.zero;
    [HideInInspector] public Vector3 NetworkPlayerVelocity = Vector3.zero;    //PlayerAnimation.cs uses this variable
    [HideInInspector] public bool NetworkPlayerFloorDetected = false;         //PlayerAnimation.cs uses this variable
    [HideInInspector] public int NetworkPlayerHealth;                         //PlayerAnimation.cs uses this variable
    [HideInInspector] public float NetworkPlayerAimAngle;                     //PlayerAnimation.cs uses this variable
    [HideInInspector] public float NetworkPlayerCurrentAimAngle;              //PlayerAnimation.cs uses this variable
    [HideInInspector] public MovementType NetworkMovementType;              //PlayerAnimation.cs uses this variable: Player Movement Type, (More to come)
    private Vector3 NetworkPlayerStartPosition = Vector3.zero;          //Determining Move Interpolation Progress
    [HideInInspector] public Vector3 NetworkPlayerCurrentPosition;      //Determining Spawn Bullet Position (For any Networked Player)



    //Variables for Calculating Package Delay
    private float lastPackageReceivedTimeStamp = 0f;
    private float syncTime = 0f;
    private float syncDelay = 0f;

    //Variables for Interpolating Networked Player Movements
    private Vector3 HeightAdjustmentVector;
    private float heightLerp;
    private float currentDistance = 0f;
    private float fullDistance = 0f;
    private float progress = 0f;
    private float lerpValue;
    private bool gotFirstUpdate = false;

    //Variables for Player to adjust network synchronization values
    private float velocityPredictionValue;  //Obtained values from NetworkSettingsDisplay.cs
    private float clientPredictionValue;    //Obtained values from NetworkSettingsDisplay.cs
    private float syncPrecictionValue;      //Obtained values from NetworkSettingsDisplay.cs
    private float syncDistanceValue;        //Obtained values from NetworkSettingsDisplay.cs
    private float syncYAxisValue;           //Obtained values from NetworkSettingsDisplay.cs
    private float finalSyncDistance;         //Calculated values
    private float finalSyncMultiplier;       //Calculated values
    private float exponentialMultiplier;    //Calculated values

    //Player Components: Required Components for Multiplayer Character object
    private PhotonView PhotonView;
    private Transform Transform;
    //Player Components: Specific Components for Multiplayer Character object
    [SerializeField]private PlayerMovement PlayerMovement;
    private PlayerShooting PlayerShooting;
    private PlayerAnimation PlayerAnimation;
    private MouseLook PlayerMouseLook;


    //Assign our 'PlayerCamerTransform' to Portal Cameras' variable: 'PlayerCamera'
    GameObject[] PortalCameras;
    GameObject[] PortalTeleporters;
    public Transform PlayerCameraTransform;

    private void Awake()
    {
        PhotonView = GetComponent<PhotonView>();
        Transform = GetComponent<Transform>();

        if (SpawnCharacterType.Equals(SpawnCharacterType.Player))
        {
            Awake_Player();
        }
        else
        {
            Awake_Car();
        }

        if (PhotonView.isMine)
        {
            PortalCameras = GameObject.FindGameObjectsWithTag("PortalCamera");
            foreach (GameObject portalCamera in PortalCameras)
            {
                portalCamera.GetComponent<PortalCamera>().playerCamera = PlayerCameraTransform;
            }

            PortalTeleporters = GameObject.FindGameObjectsWithTag("PortalCollider");
            foreach (GameObject portalTeleporter in PortalTeleporters)
            {
                portalTeleporter.GetComponent<PortalTeleporter>().player = Transform;
            }
        }

        NetworkSettingsDisplay.Instance.InitNetworkSettings();
    }

    public void Awake_Player()
    {
        PlayerMovement = GetComponent<PlayerMovement>();
        PlayerShooting = GetComponent<PlayerShooting>();
        PlayerAnimation = GetComponent<PlayerAnimation>();
        PlayerMouseLook = GetComponent<PlayerObjectComponents>().PlayerCamera.GetComponent<MouseLook>();

    }

    private void Awake_Car()
    {

    }


    //Here temporarily for Health GUI purposes
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
            stream.SendNext(SpawnCharacterType);
            stream.SendNext(PlayerMovement.MovementType);

            stream.SendNext(Transform.position);
            stream.SendNext(Transform.rotation);
            stream.SendNext(PlayerMovement.playerMovementSettings.V_PlayerVelocity);
            stream.SendNext(PlayerMovement.playerMovementSettings.V_IsFloorDetected);
            stream.SendNext(EventManager.Instance.GetScore(photonView.owner.NickName, PlayerStatCodes.Health));

            if (SpawnCharacterType.Equals(SpawnCharacterType.Player))
                stream.SendNext(PlayerMouseLook.GetCameraRotationY());
        }
        else
        {
            //To capture the 'start' position
            NetworkPlayerStartPosition = NetworkPlayerPosition;

            //We are reading incoming data
            NetworkSpawnCharacterType = (SpawnCharacterType)stream.ReceiveNext();
            NetworkMovementType = (MovementType)stream.ReceiveNext();
            NetworkPlayerPosition = (Vector3)stream.ReceiveNext();
            NetworkPlayerRotation = (Quaternion)stream.ReceiveNext();
            NetworkPlayerVelocity = (Vector3)stream.ReceiveNext();
            NetworkPlayerFloorDetected = (bool)stream.ReceiveNext();
            NetworkPlayerHealth = (int)stream.ReceiveNext();

            if (NetworkSpawnCharacterType.Equals(SpawnCharacterType.Player))
                NetworkPlayerAimAngle = (float)stream.ReceiveNext();


            //When we receive an update here, our syncTime gets re-initialized to 0
            syncTime = 0f;
            syncDelay = Time.time - lastPackageReceivedTimeStamp;
            lastPackageReceivedTimeStamp = Time.time;

            //Velocity Prediction
            NetworkPlayerPosition = NetworkPlayerPosition + (NetworkPlayerVelocity * syncDelay / 1000) * velocityPredictionValue;

            if (gotFirstUpdate == false)
            {
                Transform.position = NetworkPlayerPosition;
                Transform.rotation = NetworkPlayerRotation;
                gotFirstUpdate = true;
            }

        }
    }



    private void SmoothMoove()
    {
        currentDistance = Vector3.Distance(Transform.position, NetworkPlayerPosition);
        fullDistance = Vector3.Distance(NetworkPlayerStartPosition, NetworkPlayerPosition);
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
            Transform.position = NetworkPlayerPosition;
            Transform.rotation = NetworkPlayerRotation;
            //Or a more aggressive smoothing move

        }
        else
        {
            if ((int)NetworkPlayerVelocity.magnitude == 0)
            {
                if (syncPrecictionValue != 0)
                {

                    Transform.rotation = Quaternion.Lerp(Transform.rotation, NetworkPlayerRotation, syncTime / syncDelay);
                    Transform.position = Vector3.Lerp(Transform.position, NetworkPlayerPosition, lerpValue);

                    exponentialMultiplier = Mathf.Clamp(Vector3.Distance(Transform.position, NetworkPlayerPosition), 0.0001f, 1);
                    if (exponentialMultiplier < finalSyncDistance)
                    {
                        Transform.position = Vector3.LerpUnclamped(Transform.position, NetworkPlayerPosition, progress * finalSyncMultiplier / exponentialMultiplier);
                    }
                    if (syncYAxisValue != 0)
                    {
                        heightLerp = Transform.position.y;
                        heightLerp = Mathf.Lerp(Transform.position.y, NetworkPlayerPosition.y, syncYAxisValue);
                        HeightAdjustmentVector = new Vector3(Transform.position.x, heightLerp, Transform.position.z);
                        Transform.position = HeightAdjustmentVector;
                    }
                }
                else
                {
                    Transform.position = NetworkPlayerPosition;
                    Transform.rotation = NetworkPlayerRotation;
                }
            }
            else
            {
                if (syncYAxisValue != 0)
                {
                    heightLerp = Transform.position.y;
                    heightLerp = Mathf.Lerp(Transform.position.y, NetworkPlayerPosition.y, syncYAxisValue);
                    HeightAdjustmentVector = new Vector3(Transform.position.x, heightLerp, Transform.position.z);
                    Transform.position = HeightAdjustmentVector;
                }
                Transform.rotation = Quaternion.Lerp(Transform.rotation, NetworkPlayerRotation, syncTime / syncDelay);
                Transform.position = Vector3.Lerp(Transform.position, NetworkPlayerPosition, progress);
            }
        }

        NetworkPlayerCurrentPosition = Transform.position;

    }

}