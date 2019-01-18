using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NetworkPlayerMovement : Photon.MonoBehaviour
{
    public SpawnCharacterType SpawnCharacterType;

    private SpawnCharacterType NetworkSpawnCharacterType;

    //Networked Players Received Variables for Interpolating Movement, as well as Animating
    private Quaternion NetworkPlayerRotation = Quaternion.identity;
    private Vector3 NetworkPlayerPosition = Vector3.zero;
    [HideInInspector] public Vector3 NetworkPlayerVelocity = Vector3.zero;    //PlayerAnimation.cs uses this variable
    [HideInInspector] public bool NetworkPlayerFloorDetected = false;         //PlayerAnimation.cs uses this variable
    [HideInInspector] public int NetworkPlayerHealth;                         //PlayerAnimation.cs uses this variable
    [HideInInspector] public float NetworkPlayerAimAngle;                     //PlayerAnimation.cs uses this variable
    [HideInInspector] public float NetworkPlayerCurrentAimAngle;              //PlayerAnimation.cs uses this variable
    [HideInInspector] public MovementType NetworkMovementType;              //PlayerAnimation.cs uses this variable: Player Movement Type, (More to come)

    //FlightRunner Received Variables for Interpolating Movement
    private Quaternion NetworkHeadingRotation = Quaternion.identity;
    private float NetworkSteeringMag;
    private float NetworkAccelMag;
    private bool NetworkIsFourWheelDrive;

    //Shared Interpolating Movement Variables: Progress, Position
    private Vector3 NetworkStartPosition = Vector3.zero;          //Determining Move Interpolation Progress
    [HideInInspector] public Vector3 NetworkCurrentPosition;      //Determining Spawn Bullet Position (For any Networked Player)

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

    private float SmoothSteeringMag;
    private float SmoothAccelMag;
    private float SmoothHeadingRotation;

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
    private PlayerMovement PlayerMovement;
    private Rigidbody Rigidbody;
    //Player Components: Specific Components for Multiplayer Character object
    private PlayerShooting PlayerShooting;
    private PlayerAnimation PlayerAnimation;
    private MouseLook PlayerMouseLook;
    //Flight Runner Components: Specific Components for Multiplayer FlightRunner object
    [SerializeField] private FlightRunnerObjectComponents FlightRunnerObjectComponents;


    //Assign our 'PlayerCamerTransform' to Portal Cameras' variable: 'PlayerCamera'
    GameObject[] PortalCameras;
    GameObject[] PortalTeleporters;
    public Transform PlayerCameraTransform;

    private void Awake()
    {
        PhotonView = GetComponent<PhotonView>();
        Transform = GetComponent<Transform>();
        Rigidbody = GetComponent<Rigidbody>();
        PlayerMovement = GetComponent<PlayerMovement>();


        if (SpawnCharacterType.Equals(SpawnCharacterType.Player))
        {
            Awake_Player();
        }
        else if (SpawnCharacterType.Equals(SpawnCharacterType.FlightRunner))
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
        PlayerShooting = GetComponent<PlayerShooting>();
        PlayerAnimation = GetComponent<PlayerAnimation>();
        PlayerMouseLook = GetComponent<PlayerObjectComponents>().PlayerCamera.GetComponent<MouseLook>();

    }

    private void Awake_Car()
    {
        FlightRunnerObjectComponents = GetComponent<FlightRunnerObjectComponents>();
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
            if (SpawnCharacterType.Equals(SpawnCharacterType.Player))
            {
                SmoothMoove(NetworkPlayerPosition, NetworkPlayerRotation, Quaternion.identity);
            }
            else if (SpawnCharacterType.Equals(SpawnCharacterType.FlightRunner))
            {
                SmoothMoove(NetworkPlayerPosition, NetworkPlayerRotation, NetworkHeadingRotation);
            }
        }

    }

    private void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {


        if (stream.isWriting)
        {
            //We are sending data
            stream.SendNext(SpawnCharacterType);
            // 
            // 
            // stream.SendNext(PlayerMovement.MovementType);
            // 
            // stream.SendNext(Transform.position);
            // stream.SendNext(Transform.rotation);
            // stream.SendNext(PlayerMovement.playerMovementSettings.V_PlayerVelocity);
            // stream.SendNext(PlayerMovement.playerMovementSettings.V_IsFloorDetected);
            // stream.SendNext(EventManager.Instance.GetScore(photonView.owner.NickName, PlayerStatCodes.Health));
            // 
            // if (SpawnCharacterType.Equals(SpawnCharacterType.Player))
            //     stream.SendNext(PlayerMouseLook.GetCameraRotationY());
            if (SpawnCharacterType.Equals(SpawnCharacterType.Player))
            {
                PlayerSendSerializeStream(stream, info);
            }
            else if (SpawnCharacterType.Equals(SpawnCharacterType.FlightRunner))
            {
                FlightRunnerSendSerializeStream(stream, info);
            }
        }
        else
        {
            //To capture the 'start' position
            NetworkStartPosition = NetworkPlayerPosition;

            //We are reading incoming data
            NetworkSpawnCharacterType = (SpawnCharacterType)stream.ReceiveNext();
            // NetworkMovementType = (MovementType)stream.ReceiveNext();
            // NetworkPlayerPosition = (Vector3)stream.ReceiveNext();
            // NetworkPlayerRotation = (Quaternion)stream.ReceiveNext();
            // NetworkPlayerVelocity = (Vector3)stream.ReceiveNext();
            // NetworkPlayerFloorDetected = (bool)stream.ReceiveNext();
            // NetworkPlayerHealth = (int)stream.ReceiveNext();
            // 
            // if (NetworkSpawnCharacterType.Equals(SpawnCharacterType.Player))
            // {
            //     NetworkPlayerAimAngle = (float)stream.ReceiveNext();
            // }
            if (NetworkSpawnCharacterType.Equals(SpawnCharacterType.Player))
            {
                PlayerReceiveSerializeStream(stream, info);
            }
            else if (NetworkSpawnCharacterType.Equals(SpawnCharacterType.FlightRunner))
            {
                FlightRunnerReceiveSerializeStream(stream, info);
            }

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
                if (SpawnCharacterType.Equals(SpawnCharacterType.FlightRunner))
                {
                    FlightRunnerObjectComponents.HeadingObject.transform.rotation = NetworkHeadingRotation;
                }
                gotFirstUpdate = true;
            }

        }

    }

    private void PlayerSendSerializeStream(PhotonStream stream, PhotonMessageInfo info)
    {
        //stream.SendNext(SpawnCharacterType);
        stream.SendNext(PlayerMovement.MovementType);
        stream.SendNext(Transform.position);
        stream.SendNext(Transform.rotation);
        stream.SendNext(PlayerMovement.playerMovementSettings.V_PlayerVelocity);
        stream.SendNext(PlayerMovement.playerMovementSettings.V_IsFloorDetected);
        stream.SendNext(EventManager.Instance.GetScore(photonView.owner.NickName, PlayerStatCodes.Health));
        stream.SendNext(PlayerMouseLook.GetCameraRotationY());
    }

    private void PlayerReceiveSerializeStream(PhotonStream stream, PhotonMessageInfo info)
    {
        //NetworkSpawnCharacterType = (SpawnCharacterType)stream.ReceiveNext();
        NetworkMovementType = (MovementType)stream.ReceiveNext();
        NetworkPlayerPosition = (Vector3)stream.ReceiveNext();
        NetworkPlayerRotation = (Quaternion)stream.ReceiveNext();
        NetworkPlayerVelocity = (Vector3)stream.ReceiveNext();
        NetworkPlayerFloorDetected = (bool)stream.ReceiveNext();
        NetworkPlayerHealth = (int)stream.ReceiveNext();
        NetworkPlayerAimAngle = (float)stream.ReceiveNext();
    }

    private void FlightRunnerSendSerializeStream(PhotonStream stream, PhotonMessageInfo info)
    {
        //stream.SendNext(SpawnCharacterType);
        stream.SendNext(Transform.position);
        stream.SendNext(Transform.rotation);
        stream.SendNext(FlightRunnerObjectComponents.HeadingObject.transform.rotation);
        stream.SendNext(Rigidbody.velocity);

        stream.SendNext(PlayerMovement.flightRunnerMovementSettings.V_SteerMag);
        stream.SendNext(PlayerMovement.flightRunnerMovementSettings.V_AccelMag);
        stream.SendNext(PlayerMovement.flightRunnerMovementSettings.V_IsFourWheelDrive);
    }

    private void FlightRunnerReceiveSerializeStream(PhotonStream stream, PhotonMessageInfo info)
    {
        //NetworkSpawnCharacterType = (SpawnCharacterType)stream.ReceiveNext();
        NetworkPlayerPosition = (Vector3)stream.ReceiveNext();
        NetworkPlayerRotation = (Quaternion)stream.ReceiveNext();
        NetworkHeadingRotation = (Quaternion)stream.ReceiveNext();
        NetworkPlayerVelocity = (Vector3)stream.ReceiveNext();

        NetworkSteeringMag = (float)stream.ReceiveNext();
        NetworkAccelMag = (float)stream.ReceiveNext();
        NetworkIsFourWheelDrive = (bool)stream.ReceiveNext();

    }

    private void SmoothMoove(Vector3 NetworkPosition, Quaternion NetworkRotation, Quaternion NetworkHeadingRotation)
    {
        currentDistance = Vector3.Distance(Transform.position, NetworkPosition);
        fullDistance = Vector3.Distance(NetworkStartPosition, NetworkPosition);
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
            Transform.position = NetworkPosition;
            Transform.rotation = NetworkRotation;
            if (SpawnCharacterType.Equals(SpawnCharacterType.Player))
            {
                Transform.position = NetworkPosition;
                Transform.rotation = NetworkRotation;
            }
            else if (SpawnCharacterType.Equals(SpawnCharacterType.FlightRunner))
            {
                Rigidbody.position = NetworkPosition;
                Rigidbody.rotation = NetworkRotation;
                FlightRunnerObjectComponents.HeadingObject.transform.rotation = this.NetworkHeadingRotation;
                SmoothMove_FlightRunnerWheelsAndBooster();
            }
            //Or a more aggressive smoothing move

        }
        else
        {
            if ((int)NetworkPlayerVelocity.magnitude == 0)
            {
                if (syncPrecictionValue != 0)
                {

                    if (SpawnCharacterType.Equals(SpawnCharacterType.Player))
                    {
                    Transform.position = Vector3.Lerp(Transform.position, NetworkPosition, lerpValue);
                    Transform.rotation = Quaternion.Lerp(Transform.rotation, NetworkRotation, syncTime / syncDelay);
                    }
                    else if (SpawnCharacterType.Equals(SpawnCharacterType.FlightRunner))
                    {
                        Rigidbody.position = Vector3.Lerp(Rigidbody.position, NetworkPosition, lerpValue);
                        //Rigidbody.position = NetworkPosition;
                        SmoothMove_FlightRunnerHeading();
                        SmoothMove_FlightRunnerWheelsAndBooster();
                    }

                    exponentialMultiplier = Mathf.Clamp(Vector3.Distance(Transform.position, NetworkPosition), 0.0001f, 1);
                    if (exponentialMultiplier < finalSyncDistance)
                    {
                        if (SpawnCharacterType.Equals(SpawnCharacterType.Player))
                        {
                            Transform.position = Vector3.LerpUnclamped(Transform.position, NetworkPosition, progress * finalSyncMultiplier / exponentialMultiplier);
                        }
                        else if (SpawnCharacterType.Equals(SpawnCharacterType.FlightRunner))
                        {
                            Rigidbody.position = Vector3.LerpUnclamped(Rigidbody.position, NetworkPosition, progress * finalSyncMultiplier / exponentialMultiplier);
                            //Rigidbody.position = NetworkPosition;
                        }

                    }
                    if (syncYAxisValue != 0)
                    {
                        heightLerp = Transform.position.y;
                        heightLerp = Mathf.Lerp(Transform.position.y, NetworkPosition.y, syncYAxisValue);
                        if (SpawnCharacterType.Equals(SpawnCharacterType.Player))
                        {
                            HeightAdjustmentVector = new Vector3(Transform.position.x, heightLerp, Transform.position.z);
                            Transform.position = HeightAdjustmentVector;
                        }
                        else if (SpawnCharacterType.Equals(SpawnCharacterType.FlightRunner))
                        {
                            HeightAdjustmentVector = new Vector3(Rigidbody.position.x, heightLerp, Rigidbody.position.z);
                            Rigidbody.position = HeightAdjustmentVector;
                        }
                    }
                }
                else
                {
                    if (SpawnCharacterType.Equals(SpawnCharacterType.Player))
                    {
                        Transform.position = NetworkPosition;
                        Transform.rotation = Quaternion.Lerp(Transform.rotation, NetworkRotation, syncTime / syncDelay);
                    }
                    else if (SpawnCharacterType.Equals(SpawnCharacterType.FlightRunner))
                    {
                        Rigidbody.position = NetworkPosition;
                        SmoothMove_FlightRunnerHeading();
                        SmoothMove_FlightRunnerWheelsAndBooster();
                    }
                }
            }
            else
            {
                if (syncYAxisValue != 0)
                {
                    heightLerp = Transform.position.y;
                    heightLerp = Mathf.Lerp(Transform.position.y, NetworkPosition.y, syncYAxisValue);
                    HeightAdjustmentVector = new Vector3(Transform.position.x, heightLerp, Transform.position.z);
                    if (SpawnCharacterType.Equals(SpawnCharacterType.Player))
                    {
                        Transform.position = HeightAdjustmentVector;
                    }
                    else if (SpawnCharacterType.Equals(SpawnCharacterType.Player))
                    {
                        Rigidbody.position = HeightAdjustmentVector;
                    }
                }
                if (SpawnCharacterType.Equals(SpawnCharacterType.Player))
                {
                    Transform.position = Vector3.Lerp(Transform.position, NetworkPosition, progress);
                    Transform.rotation = Quaternion.Lerp(Transform.rotation, NetworkRotation, syncTime / syncDelay);
                }
                else if (SpawnCharacterType.Equals(SpawnCharacterType.FlightRunner))
                {
                    Rigidbody.position = Vector3.Lerp(Rigidbody.position, NetworkPosition, progress);
                    SmoothMove_FlightRunnerHeading();
                    SmoothMove_FlightRunnerWheelsAndBooster();
                }
            }
        }

        NetworkCurrentPosition = Transform.position;

    }

    private void SmoothMove_FlightRunnerHeading()
    {
        Rigidbody.rotation = Quaternion.Lerp(Rigidbody.rotation, NetworkPlayerRotation, syncTime / syncDelay);
        FlightRunnerObjectComponents.HeadingObject.transform.rotation = Quaternion.Lerp(FlightRunnerObjectComponents.HeadingObject.transform.rotation, this.NetworkHeadingRotation, syncTime / syncDelay);
    }

    private void SmoothMove_FlightRunnerWheelsAndBooster()
    {
        SmoothSteeringMag = Mathf.Lerp(SmoothSteeringMag, NetworkSteeringMag, syncTime / syncDelay);
        SmoothAccelMag = Mathf.Lerp(SmoothAccelMag, NetworkAccelMag, syncTime / syncDelay);
        //Rotate Wheels based on Steering (Networked)
        foreach (GameObject Axel in FlightRunnerObjectComponents.Axel_Front)
        {
            Axel.transform.localRotation = Quaternion.Euler(0, SmoothSteeringMag, 0);
        }
        if (NetworkIsFourWheelDrive)
        {
            foreach (GameObject Axel in FlightRunnerObjectComponents.Axel_Rear)
            {
                Axel.transform.localRotation = Quaternion.Euler(0, -SmoothSteeringMag, 0);
            }
        }
        //Spin Wheels (Networked)
        foreach (GameObject Wheel in FlightRunnerObjectComponents.Wheels_Front)
        {
            Wheel.transform.rotation *= Quaternion.Euler(0, SmoothAccelMag * .35f, 0);
        }
        foreach (GameObject Wheel in FlightRunnerObjectComponents.Wheels_Rear)
        {
            Wheel.transform.rotation *= Quaternion.Euler(0, SmoothAccelMag * .35f, 0);
        }

        foreach(GameObject Blade in FlightRunnerObjectComponents.BoosterBlades)
        {
            Blade.transform.rotation *= Quaternion.Euler(0, SmoothAccelMag * .7f + 5, 0);
        }

    }

}