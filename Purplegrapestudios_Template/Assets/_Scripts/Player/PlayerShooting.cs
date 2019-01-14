using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HitTargets
{
    Nothing,
    Player,
    Environment
}

public class PlayerShooting : MonoBehaviour
{

    PhotonView photonView;
    PlayerMovement playerMovement;
    NetworkPlayerMovement networkPlayerMovement;
    PlayerAnimation playerAnimation;
    PlayerObjectComponents playerObjectComponents;
    Transform tr;

    [HideInInspector] public HitTargets hitTarget;

    public LayerMask NormalMask;
    public LayerMask DeathMask;

    public Transform playerCamera;
    public Transform deathCamera;
    public Transform bulletLocationTransformFirstPerson;
    public Transform bulletLocationTransformThirdPerson;



    int nonItemLayerMask = ~(1 << 9);  //Do not hit invisible layer
    Ray ray;
    [HideInInspector] public Vector3 hitPoint;
    RaycastHit[] hits;
    RaycastHit currentHit;

    Transform hitTransform;
    Transform currentHitTransform;
    Transform closestHit;

    public GameObject BulletParticle;
    public GameObject BulletParticleNetworked;
    public float bulletFireRate;
    float currentBulletRateTime;

    public bool isBulletParticleActive;
    public bool isFiringBullet;
    public bool doBulletFX;
    public bool isAiming;
    public bool IsPlayerDead;

    private Vector3 receivedHitPoint;
    private HitTargets receivedHitTarget;
    private string receivedOwnerName;


    private void Start()
    {
        tr = GetComponent<Transform>();
        photonView = GetComponent<PhotonView>();
        playerMovement = GetComponent<PlayerMovement>();
        playerAnimation = GetComponent<PlayerAnimation>();
        networkPlayerMovement = GetComponent<NetworkPlayerMovement>();
        if (photonView.isMine)
        {
            playerObjectComponents = GetComponent<PlayerObjectComponents>();
        }
    }

    private void Update()
    {
        Input_Update();
    }

    private void FixedUpdate()
    {
        if (!photonView.isMine)
        {
            if (doBulletFX)
            {
                ObjectPoolManager.Instance.SpawnBullet_Local(networkPlayerMovement.NetworkPlayerCurrentPosition, receivedHitPoint, receivedHitTarget, receivedOwnerName);
                doBulletFX = false;
            }
        }
        else
        {
            if (isFiringBullet)
            {
                if (currentBulletRateTime >= bulletFireRate)
                {
                    currentBulletRateTime = 0f;

                    PrepareRayCast_FixedUpdate();
                    PrepareHitPoint_FixedUpdate();
                    hitTransform = FindClosestHitObject_FixedUpdate(ray, out hitPoint);
                    FireBullet_FixedUpdate();
                    DealDamage_FixedUpdate(hitTransform);
                }
                else
                {
                    currentBulletRateTime += Time.fixedDeltaTime;
                }
            }
        }
    }

    /// <summary>
    /// UPDATE CALL: 1) Fire 2) Aim. More Combat inputs to come...
    /// </summary>
    private void Input_Update()
    {
        if (!photonView.isMine) return;
        else
        {
            isFiringBullet = InputManager.Instance.GetKey(InputCode.Shoot) ? true : false;
            isAiming = InputManager.Instance.GetKey(InputCode.Aim) ? true : false;
        }
    }

    /// <summary>
    /// FIXED UPDATE CALL
    /// </summary>
    private void PrepareRayCast_FixedUpdate()
    {
        if (playerMovement.MovementType.Equals(MovementType.Player))
        {
            if (playerAnimation.playerCameraView.Equals(PlayerCameraView.FirstPerson))
            {
                ray = new Ray(playerCamera.position, playerCamera.forward);
            }
            else
            {
                ray = new Ray(bulletLocationTransformThirdPerson.position, playerCamera.forward);
            }
        }
        else
        {
            ray = new Ray(playerCamera.position, playerCamera.forward);
        }
    }

    /// <summary>
    /// FIXED UPDATE CALL: Basically just, hitPoint = playerCamera.forward
    /// </summary>
    private void PrepareHitPoint_FixedUpdate()
    {
        hitPoint = playerCamera.forward;
    }

    /// <summary>
    /// FIXED UPDATE CALL
    /// </summary>
    /// <param name="hitTransform"></param>
    private void DealDamage_FixedUpdate(Transform hitTransform)
    {
        if (hitTransform == null) return;
        if (hitTransform.CompareTag("Player"))
        {
            if (hitTransform.GetComponent<PlayerShooting>().enabled)
            {
                if (EventManager.Instance.GetScore(hitTransform.GetComponent<PhotonView>().owner.NickName, PlayerStatCodes.Health) - 10 >= 0)
                {
                    EventManager.Instance.UpdateStat_Health(hitTransform.GetComponent<PhotonView>().owner.NickName, PlayerStatCodes.Health, -10);
                    EventManager.Instance.UpdateStat_DamageReceived(hitTransform.GetComponent<PhotonView>().owner.NickName, PlayerStatCodes.DamageReceived, 10);
                    EventManager.Instance.UpdateStat_DamageDealt(photonView.owner.NickName, PlayerStatCodes.DamageDealt, 10);
                }
                if (EventManager.Instance.GetScore(hitTransform.GetComponent<PhotonView>().owner.NickName, PlayerStatCodes.Health) - 10 <= 0)
                {
                    if (hitTransform.GetComponent<PlayerShooting>().enabled)
                    {
                        hitTransform.GetComponent<PlayerShooting>().PlayerEvents_RespawnPlayer(hitTransform.GetComponent<PhotonView>().ownerId, hitTransform.GetComponent<PhotonView>().owner.NickName, photonView.owner.NickName);
                    }
                }
            }
        }
    }

    /// <summary>
    /// FIXED UPDATE CALL
    /// </summary>
    private void FireBullet_FixedUpdate()
    {
        if (playerMovement.MovementType.Equals(MovementType.Player))
        {
            //The Event is handled PARTIALLY over the network, and partially locally. This is to simulate the Bullet starting point based on 1st or 3rd Person View
            //This is because the Bullet position at the beginning is DIFFERENT for the Local Player v.s. the Networked Player that others see
            //This is why there is a need to separate the Bullet Locally and over the Network.
            //Furthermore, since 1st and 3rd Person 'Gun Positions' are not exactly the same, we further split it up within the IF statements.
            FireBulletFX_Event(true, hitPoint, hitTarget, photonView.owner.NickName);
            if (playerAnimation.playerCameraView.Equals(PlayerCameraView.FirstPerson))
            {
                ObjectPoolManager.Instance.SpawnBullet_Local(bulletLocationTransformFirstPerson.position, hitPoint, hitTarget, photonView.owner.NickName);
            }
            else
            {
                ObjectPoolManager.Instance.SpawnBullet_Local(bulletLocationTransformThirdPerson.position, hitPoint, hitTarget, photonView.owner.NickName);
            }
        }
        else
        {
            //For the Vehicle shooting mode, since the Turrets are not rapidly changing, there is very LITTLE DIFFERENCE between the Networked and Local bullet position initially.
            //So this event is handled TOTALLY over the network. No need for 'bullet position' correction
            ObjectPoolManager.Instance.SpawnBullet_Network(transform.position + new Vector3(0, -0.5f, 1), hitPoint, hitTarget, photonView.owner.NickName);
        }

    }

    /// <summary>
    /// FIXED UPDATE CALL
    /// </summary>
    /// <param name="ray"></param>
    /// <param name="hitPoint"></param>
    /// <returns></returns>
    private Transform FindClosestHitObject_FixedUpdate(Ray ray, out Vector3 hitPoint)
    {


        hits = Physics.RaycastAll(ray, 2000f, nonItemLayerMask);

        closestHit = null;
        float distance = 0;
        hitPoint = Vector3.zero;

        foreach (RaycastHit hit in hits)
        {
            currentHitTransform = hit.transform;
            currentHit = hit;

            if (currentHitTransform != transform && (closestHit == null || hit.distance < distance))
            {
                //we have hit something that is:
                //a) not us
                //b) the first thing we hit (that is not us)
                //c) or, if not b, is at least closer than the previous closest thing

                closestHit = currentHitTransform;
                distance = hit.distance;
                hitPoint = hit.point;

            }
        }
        //closestHit is now either still null (i.e. we hit nothing) OR it contains the closest thing that is valid to hit
        if (closestHit == null)
        {
            hitPoint = playerCamera.position + (playerCamera.forward * 2000f);
            hitTarget = HitTargets.Nothing;
        }
        else
        {
            if (closestHit.CompareTag("Player"))
            {
                hitTarget = HitTargets.Player;
            }
            else if (closestHit.CompareTag("Level"))
            {
                hitTarget = HitTargets.Environment;
            }
            else
            {
                hitTarget = HitTargets.Environment;
            }
        }
        return closestHit;
    }


    private void OnEnable()
    {
        PhotonNetwork.OnEventCall += PhotonNetwork_OnEventCall;
    }

    private void OnDisable()
    {
        PhotonNetwork.OnEventCall -= PhotonNetwork_OnEventCall;
    }

    /// <summary>
    /// CALLBACK: Subcribed upon OnEnable and Unsubsribed upon OnDisable
    /// </summary>
    /// <param name="eventCode"></param>
    /// <param name="content"></param>
    /// <param name="senderId"></param>
    private void PhotonNetwork_OnEventCall(byte eventCode, object content, int senderId)
    {
        PhotonEventCodes code = (PhotonEventCodes)eventCode;

        if (code == PhotonEventCodes.RespawnPlayer)
        {
            object[] datas = content as object[];
            if (datas.Length == 3)
            {
                if (RespawnPlayerCoroutine != null)
                    StopCoroutine(RespawnPlayerCoroutine);
                RespawnPlayerCoroutine = RespawnPlayer((int)datas[0], (string)datas[1], (string)datas[2]);
                StartCoroutine(RespawnPlayerCoroutine);


            }
        }
        else if (code.Equals(PhotonEventCodes.FireBulletFX))
        {
            object[] datas = content as object[];
            if (datas.Length.Equals(4))
            {
                FireBulletFX((bool)datas[0], (Vector3)datas[1], (HitTargets)datas[2], (string)datas[3]);
            }
        }
    }

    /// <summary>
    /// NETWORKED COROUTINE: Respawn Player
    /// </summary>
    /// <param name="photonOwnerID"></param>
    /// <param name="respawnPlayerName"></param>
    /// <param name="killedByPlayerName"></param>
    /// <returns></returns>
    IEnumerator RespawnPlayer(int photonOwnerID, string respawnPlayerName, string killedByPlayerName)
    {
        if (photonOwnerID == photonView.ownerId)
        {
            transform.GetComponent<PlayerShooting>().IsPlayerDead = true;
            EventManager.Instance.UpdateStat(respawnPlayerName, PlayerStatCodes.Deaths, 1);
            if (killedByPlayerName != string.Empty)
            {
                EventManager.Instance.UpdateStat(killedByPlayerName, PlayerStatCodes.Kills, 1);
            }
            transform.GetComponent<PlayerShooting>().enabled = false;
            PlayerCameraView originalPlayerCamerView = GetComponent<PlayerAnimation>().playerCameraView;

            yield return new WaitForSeconds(2f);

            float randomValue = Random.Range(-30f, 30f);
            transform.position = new Vector3(randomValue, 120, randomValue);
            transform.rotation = Quaternion.identity;

            EventManager.Instance.SetStat(respawnPlayerName, PlayerStatCodes.Health, 100);

            yield return new WaitForSeconds(2f);

            transform.GetComponent<PlayerShooting>().enabled = true;
            transform.GetComponent<PlayerShooting>().IsPlayerDead = false;
        }

        RespawnPlayerCoroutine = null;
        yield break;
    }
    IEnumerator RespawnPlayerCoroutine;

    /// <summary>
    /// NETWORKED FUNCTION: FireBullet
    /// </summary>
    /// <param name="value"></param>
    /// <param name="hitPoint"></param>
    /// <param name="hitTarget"></param>
    /// <param name="ownerName"></param>
    public void FireBulletFX(bool value, Vector3 hitPoint, HitTargets hitTarget, string ownerName)
    {
        doBulletFX = value;
        receivedHitPoint = hitPoint;
        receivedHitTarget = hitTarget;
        receivedOwnerName = ownerName;
    }


    /// <summary>
    /// NETWORK EVENT CALLS: Netowrk RespawnPlayer
    /// </summary>
    /// <param name="photonOwnerID"></param>
    /// <param name="respawnPlayerName"></param>
    /// <param name="killedByPlayerName"></param>
    public void PlayerEvents_RespawnPlayer(int photonOwnerID, string respawnPlayerName, string killedByPlayerName)
    {
        object[] datas = new object[] { photonOwnerID, respawnPlayerName, killedByPlayerName };

        if (PhotonNetwork.offlineMode)
        {
            if (RespawnPlayerCoroutine != null)
                StopCoroutine(RespawnPlayerCoroutine);
            RespawnPlayerCoroutine = RespawnPlayer(photonOwnerID, respawnPlayerName, killedByPlayerName);
            StartCoroutine(RespawnPlayerCoroutine);
        }
        else
        {
            RaiseEventOptions options = new RaiseEventOptions()
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.All
            };

            PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.RespawnPlayer, datas, true, options);
        }
    }

    /// <summary>
    /// NETWORK EVENT CALLS: Network FireBullet
    /// </summary>
    /// <param name="value"></param>
    /// <param name="hitPoint"></param>
    /// <param name="hitTarget"></param>
    /// <param name="ownerName"></param>
    public void FireBulletFX_Event(bool value, Vector3 hitPoint, HitTargets hitTarget, string ownerName)
    {
        object[] datas = new object[] { value, hitPoint, hitTarget, ownerName };

        if (PhotonNetwork.offlineMode)
        {
            FireBulletFX(value, hitPoint, hitTarget, ownerName);
        }
        else
        {
            RaiseEventOptions options = new RaiseEventOptions()
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.Others
            };

            PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.FireBulletFX, datas, false, options);
        }
    }

}