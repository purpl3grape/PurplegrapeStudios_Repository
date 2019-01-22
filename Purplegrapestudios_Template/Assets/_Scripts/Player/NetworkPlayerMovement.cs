using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NetworkPlayerMovement : Photon.MonoBehaviour
{
    #region Public Variables
    public SpawnCharacterType SpawnCharacterType;
    public Transform PlayerCameraTransform;
    #endregion Public Variables

    #region Public Hidden Variables
    [HideInInspector] public Vector3 NetworkCurrentPosition;                    //Determining Spawn Bullet Position (For any Networked Player)

    #region Public Hidden: Player Networked Variables
    [HideInInspector] public Quaternion NetworkPlayerRotation = Quaternion.identity;
    [HideInInspector] public Vector3 NetworkPlayerPosition = Vector3.zero;
    [HideInInspector] public Vector3 NetworkPlayerVelocity = Vector3.zero;      // PlayerAnimation.cs uses this variable
    [HideInInspector] public bool NetworkPlayerFloorDetected = false;           // PlayerAnimation.cs uses this variable
    [HideInInspector] public int NetworkPlayerHealth;                           // PlayerAnimation.cs uses this variable
    [HideInInspector] public float NetworkPlayerAimAngle;                       // PlayerAnimation.cs uses this variable
    [HideInInspector] public float NetworkPlayerCurrentAimAngle;                // PlayerAnimation.cs uses this variable
    [HideInInspector] public MovementType NetworkMovementType;                  // PlayerAnimation.cs uses this variable: Player Movement Type, (More to come)
    #endregion Public Hidden: Player Networked Variables

    #region Public Hidden: FlightRunner Networked Variables
    [HideInInspector] public Quaternion NetworkHeadingRotation = Quaternion.identity;
    [HideInInspector] public float NetworkSteeringMag;
    [HideInInspector] public float NetworkAccelMag;
    [HideInInspector] public bool NetworkIsFourWheelDrive;
    #endregion Public Hidden: FlightRunner Networked Variables

    #endregion Public Hidden Variables

    #region Private Variables

    #region Network Package Data Variables
    private float lastPackageReceivedTimeStamp = 0f;
    private float syncTime = 0f;
    private float syncDelay = 0f;
    #endregion Network Package Data Variables

    #region Smoothing Calculation Variables
    //General Smoothing Variables
    private Vector3 HeightAdjustmentVector;
    private float heightLerp;
    private float currentDistance = 0f;
    private float fullDistance = 0f;
    private Vector3 ProgressStartPosition = Vector3.zero;
    private float progress = 0f;
    private float lerpValue;
    private bool gotFirstUpdate = false;

    //FlightRunner Smoothing Variables
    private float SmoothSteeringMag;
    private float SmoothAccelMag;
    private float SmoothHeadingRotation;
    #endregion Smoothing Calculation Variables

    #region Smoothing Settings
    private float velocityPredictionValue;                          // Obtained values from NetworkSettingsDisplay.cs
    private float clientPredictionValue;                            // Obtained values from NetworkSettingsDisplay.cs
    private float syncPrecictionValue;                              // Obtained values from NetworkSettingsDisplay.cs
    private float syncDistanceValue;                                // Obtained values from NetworkSettingsDisplay.cs
    private float syncYAxisValue;                                   // Obtained values from NetworkSettingsDisplay.cs
    private float finalSyncDistance;                                // Calculated values
    private float finalSyncMultiplier;                              // Calculated values
    private float distanceRemaining;                            // Calculated values
    #endregion Smoothing Settings

    #region Referenced Components

    #region Referenced Components -> General
    private PhotonView PhotonView;
    private Transform Transform;
    private PlayerMovement PlayerMovement;
    private Rigidbody Rigidbody;
    #endregion Referenced Components -> General

    #region Referenced Components -> Player
    private PlayerShooting PlayerShooting;
    private PlayerAnimation PlayerAnimation;
    private MouseLook PlayerMouseLook;
    #endregion Referenced Components -> Player
    
    #region Referenced Components -> FlightRunner
    private FlightRunnerObjectComponents FlightRunnerObjectComponents;
    #endregion Referenced Components -> FlightRunner

    #region Referenced Components -> Portals
    private GameObject[] PortalCameras;
    private GameObject[] PortalTeleporters;
    #endregion Referenced Components -> Portals

    #endregion Referenced Components

    #endregion Private Variables

    #region Initialization Methods
    private void Awake()
    {
        InitGeneralReferences();
        InitPlayerReferences();
        InitFlightRunnerReferences();
        NetworkSettingsDisplay.Instance.InitNetworkSettings();
    }

    private void InitGeneralReferences()
    {
        PhotonView = GetComponent<PhotonView>();
        Transform = GetComponent<Transform>();
        Rigidbody = GetComponent<Rigidbody>();
        PlayerMovement = GetComponent<PlayerMovement>();
    }

    private void InitPlayerReferences()
    {
        if (SpawnCharacterType.Equals(SpawnCharacterType.Player))
        {
            PlayerShooting = GetComponent<PlayerShooting>();
            PlayerAnimation = GetComponent<PlayerAnimation>();
            PlayerMouseLook = GetComponent<PlayerObjectComponents>().PlayerCamera.GetComponent<MouseLook>();
        }
    }

    private void InitFlightRunnerReferences()
    {
        if (SpawnCharacterType.Equals(SpawnCharacterType.FlightRunner))
        {
            FlightRunnerObjectComponents = GetComponent<FlightRunnerObjectComponents>();
        }
    }

    private void InitPortalCameraReferences()
    {
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
    }
    #endregion Initialization Methods

    #region Main Functions

    #region Update Loop
    private int currentHealth;      //Here temporarily for Health GUI purposes
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
                SmoothMove();
            }
            else if (SpawnCharacterType.Equals(SpawnCharacterType.FlightRunner))
            {
                SmoothMove();
            }
        }

    }

    #endregion Update Loop

    #region Send Receive Serialized Data
    private void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {


        if (stream.isWriting)
        {
            //Sending Data
            PlayerSendSerializeStream(stream, info);
            FlightRunnerSendSerializeStream(stream, info);
        }
        else
        {
            //Start position for interpolation progress
            ProgressStartPosition = NetworkPlayerPosition;
            //When we receive package, our syncTime gets re-initialized to 0
            syncTime = 0f;
            syncDelay = Time.time - lastPackageReceivedTimeStamp;
            lastPackageReceivedTimeStamp = Time.time;

            //Receiving Data
            PlayerReceiveSerializeStream(stream, info);
            FlightRunnerReceiveSerializeStream(stream, info);

            //Velocity Prediction
            NetworkPlayerPosition = NetworkPlayerPosition + (NetworkPlayerVelocity * syncDelay / 1000) * velocityPredictionValue;

            CheckStreamInit();
        }
    }

    #region Send Receive Data -> Init
    private void CheckStreamInit()
    {
        if (gotFirstUpdate == false)
        {
            SmoothMove_Player(_smoothAmount: 0);
            SmoothMove_FlightRunner(_smoothAmount: 0);
            gotFirstUpdate = true;
        }
    }
    #endregion Send Receive Data -> Init

    #region Send Receive Serialized Data -> Player
    private void PlayerSendSerializeStream(PhotonStream stream, PhotonMessageInfo info)
    {
        if (SpawnCharacterType.Equals(SpawnCharacterType.Player))
        {
            stream.SendNext(PlayerMovement.MovementType);
            stream.SendNext(Transform.position);
            stream.SendNext(Transform.rotation);
            stream.SendNext(PlayerMovement.PMS.V_PlayerVelocity);
            stream.SendNext(PlayerMovement.PMS.V_IsFloorDetected);
            stream.SendNext(EventManager.Instance.GetScore(photonView.owner.NickName, PlayerStatCodes.Health));
            stream.SendNext(PlayerMouseLook.GetCameraRotationY());
        }
    }

    private void PlayerReceiveSerializeStream(PhotonStream stream, PhotonMessageInfo info)
    {
        if (SpawnCharacterType.Equals(SpawnCharacterType.Player))
        {
            NetworkMovementType = (MovementType)stream.ReceiveNext();
            NetworkPlayerPosition = (Vector3)stream.ReceiveNext();
            NetworkPlayerRotation = (Quaternion)stream.ReceiveNext();
            NetworkPlayerVelocity = (Vector3)stream.ReceiveNext();
            NetworkPlayerFloorDetected = (bool)stream.ReceiveNext();
            NetworkPlayerHealth = (int)stream.ReceiveNext();
            NetworkPlayerAimAngle = (float)stream.ReceiveNext();
        }
    }
    #endregion Send Receive Serialized Data -> Player

    #region Send Receive Serialized Data -> FlightRunner
    private void FlightRunnerSendSerializeStream(PhotonStream stream, PhotonMessageInfo info)
    {
        if (SpawnCharacterType.Equals(SpawnCharacterType.FlightRunner))
        {
            stream.SendNext(Transform.position);
            stream.SendNext(Transform.rotation);
            stream.SendNext(FlightRunnerObjectComponents.HeadingObject.transform.rotation);
            stream.SendNext(Rigidbody.velocity);

            stream.SendNext(PlayerMovement.FMS.V_SteerMag);
            stream.SendNext(PlayerMovement.FMS.V_AccelMag);
            stream.SendNext(PlayerMovement.FMS.V_IsFourWheelDrive);
        }
    }

    private void FlightRunnerReceiveSerializeStream(PhotonStream stream, PhotonMessageInfo info)
    {
        if (SpawnCharacterType.Equals(SpawnCharacterType.FlightRunner))
        {
            NetworkPlayerPosition = (Vector3)stream.ReceiveNext();
            NetworkPlayerRotation = (Quaternion)stream.ReceiveNext();
            NetworkHeadingRotation = (Quaternion)stream.ReceiveNext();
            NetworkPlayerVelocity = (Vector3)stream.ReceiveNext();

            NetworkSteeringMag = (float)stream.ReceiveNext();
            NetworkAccelMag = (float)stream.ReceiveNext();
            NetworkIsFourWheelDrive = (bool)stream.ReceiveNext();
        }
    }
    #endregion Send Receive Serialized Data -> FlightRunner

    #endregion Send Receive Serialized Data

    #region Interpolation Methods
    private void SmoothMove()
    {
        CalculateLerpAndProgress();
        syncTime += Time.deltaTime;

        //Networked Object Too far past Distance treshold
        if (currentDistance > syncDistanceValue)
        {
            SmoothMove_Player(_smoothAmount: 0);
            SmoothMove_FlightRunner(_smoothAmount: 0);
        }
        else
        {
            //Networked Object Stopped Moving
            if ((int)NetworkPlayerVelocity.magnitude == 0)
            {
                if (syncPrecictionValue != 0)
                {
                    SmoothMove_Player(_smoothAmount: lerpValue);
                    SmoothMove_FlightRunner(_smoothAmount: lerpValue);
                    SmoothFinal_General();

                    if (syncYAxisValue != 0) {
                        SmoothVertical_Player();
                    }
                }
                else
                {
                    SmoothMove_Player(_smoothAmount: 0);
                    SmoothMove_FlightRunner(_smoothAmount: 0);
                }
            }
            //Networked Object is Moving
            else
            {
                if (syncYAxisValue != 0)
                {
                    SmoothVertical_Player();
                }
                SmoothMove_Player(_smoothAmount: progress);
                SmoothMove_FlightRunner(_smoothAmount: progress);
            }
        }

        NetworkCurrentPosition = Transform.position;
    }

    #region Interpolation -> Calculate Smoothing Progress
    private void CalculateLerpAndProgress()
    {
        currentDistance = Vector3.Distance(Transform.position, NetworkPlayerPosition);
        fullDistance = Vector3.Distance(ProgressStartPosition, NetworkPlayerPosition);
        if (fullDistance != 0)
        {
            progress = currentDistance / fullDistance;
            lerpValue = progress;
        }

        if (clientPredictionValue != 0)
        {
            progress /= clientPredictionValue;
        }
        if (syncPrecictionValue != 0)
        {
            lerpValue /= syncPrecictionValue;
        }
    }
    #endregion Interpolation -> Calculate Smoothing Progress

    #region Interpolation -> General / Final Smoothing
    private void SmoothVertical_Player()
    {
        heightLerp = Transform.position.y;
        heightLerp = Mathf.Lerp(Transform.position.y, NetworkPlayerPosition.y, syncYAxisValue);
        HeightAdjustmentVector = new Vector3(Transform.position.x, heightLerp, Transform.position.z);
        if (SpawnCharacterType.Equals(SpawnCharacterType.Player))
        {
            Transform.position = HeightAdjustmentVector;
            Rigidbody.position = HeightAdjustmentVector;
        }
    }

    private void SmoothFinal_General()
    {
        distanceRemaining = Mathf.Clamp(Vector3.Distance(Transform.position, NetworkPlayerPosition), 0.0001f, 1);
        if (distanceRemaining < finalSyncDistance)
        {
            if (SpawnCharacterType.Equals(SpawnCharacterType.Player))
            {
                Transform.position = Vector3.LerpUnclamped(Transform.position, NetworkPlayerPosition, progress * finalSyncMultiplier / distanceRemaining);
            }
            else if (SpawnCharacterType.Equals(SpawnCharacterType.FlightRunner))
            {
                Rigidbody.position = Vector3.LerpUnclamped(Rigidbody.position, NetworkPlayerPosition, progress * finalSyncMultiplier / distanceRemaining);
            }

        }
    }
    #endregion Interpolation -> General Additional Vertical Smoothing

    #region Interpolation -> Player Specific
    private void SmoothMove_Player(float _smoothAmount)
    {
        if (SpawnCharacterType.Equals(SpawnCharacterType.Player))
        {
            SmoothRotate_Player();
            if (_smoothAmount > 0)
            {
                Transform.position = Vector3.Lerp(Transform.position, NetworkPlayerPosition, _smoothAmount);
            }
            else
            {
                Transform.position = NetworkPlayerPosition;
            }
        }
    }

    private void SmoothRotate_Player()
    {
        Transform.rotation = Quaternion.Lerp(Transform.rotation, NetworkPlayerRotation, syncTime / syncDelay);
    }
    #endregion Interpolation -> Player Specific

    #region Interpolation -> FlightRunner Specific
    private void SmoothRotate_FlightRunnerHeading()
    {
        Rigidbody.rotation = Quaternion.Lerp(Rigidbody.rotation, NetworkPlayerRotation, syncTime / syncDelay);
        FlightRunnerObjectComponents.HeadingObject.transform.rotation = Quaternion.Lerp(FlightRunnerObjectComponents.HeadingObject.transform.rotation, this.NetworkHeadingRotation, syncTime / syncDelay);
    }

    private void SmoothRotate_FlightRunnerWheelsAndBooster()
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

    private void SmoothMove_FlightRunner(float _smoothAmount)
    {
        if (SpawnCharacterType.Equals(SpawnCharacterType.FlightRunner))
        {
            SmoothRotate_FlightRunnerHeading();
            SmoothRotate_FlightRunnerWheelsAndBooster();

            if (_smoothAmount > 0)
            {
                Rigidbody.position = Vector3.Lerp(Rigidbody.position, NetworkPlayerPosition, _smoothAmount);
            }
            else
            {
                Rigidbody.position = NetworkPlayerPosition;
            }
        }
    }
    #endregion Interpolation -> FlightRunner Specific

    #endregion Interpolation Methods
    
    #endregion Main Functions
}