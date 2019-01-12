using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ObjectPoolManager : Photon.PunBehaviour {

    public static ObjectPoolManager Instance;

    public GameObject BulletPrefab, BulletImpactPrefab, MuzzleFlashPrefab, BloodSplatterPrefab;
    PhotonView PhotonView;
    [HideInInspector] public List<GameObject> bulletList, bulletImpactList, muzzleFlashList, bloodSplatterList;

    [HideInInspector] public Coroutine FireBulletCoroutine;

    private List<GameObject> tempList;

    PhotonPlayer[] photonPlayerList;

    private void Awake()
    {
        Instance = this;

        PhotonView = GetComponent<PhotonView>();

        PoolPrefab(250, MuzzleFlashPrefab, out muzzleFlashList);
        PoolPrefab(250, BulletPrefab, out bulletList);
        PoolPrefab(250, BulletImpactPrefab, out bulletImpactList);
        PoolPrefab(250, BloodSplatterPrefab, out bloodSplatterList);

        photonPlayerList = PhotonNetwork.playerList;
    }

    //*OBJECT POOL MANAGER METHODS*//
    public void PoolPrefab(int size, GameObject prefab, out List<GameObject> list)
    {
        list = new List<GameObject>();
        for (int i = 0; i < size; i++)
        {
            GameObject obj = (GameObject)Instantiate(prefab, Vector3.zero, Quaternion.identity);
            obj.SetActive(false);
            list.Add(obj);
        }
    }

    public List<GameObject> GetFXPrefab(List<GameObject> list, out GameObject obj)
    {
        tempList = list;
        obj = null;
        if (tempList.Count > 0)
        {
            obj = tempList[0];
            tempList.RemoveAt(0);
            obj.SetActive(true);
            return tempList;
        }
        return tempList;
    }

    public List<GameObject> DestroyFXPrefab(GameObject obj, List<GameObject> list)
    {
        list.Add(obj);
        obj.SetActive(false);
        return list;
    }
    /**/


    //*PHOTON EVENT CALLBACKS*//
    private void OnEnable()
    {
        PhotonNetwork.OnEventCall += PhotonNetwork_OnEventCall;
    }

    private void OnDisable()
    {
        PhotonNetwork.OnEventCall -= PhotonNetwork_OnEventCall;
    }

    public override void OnOwnershipRequest(object[] viewAndPlayer)
    {
        PhotonView view = viewAndPlayer[0] as PhotonView;
        PhotonPlayer requestingPlayer = viewAndPlayer[1] as PhotonPlayer;
        base.photonView.TransferOwnership(requestingPlayer);
    }

    private void PhotonNetwork_OnEventCall(byte eventCode, object content, int senderId)
    {
        PhotonEventCodes code = (PhotonEventCodes)eventCode;

        if (code == PhotonEventCodes.SpawnBulletEvent)
        {
            object[] datas = content as object[];
            if (datas.Length == 4)
            {
                SpawnBullet((Vector3)datas[0], (Vector3)datas[1], (HitTargets)datas[2], (string)datas[3]);
            }
        }
    }

    private void SpawnBullet(Vector3 startPos, Vector3 endPos, HitTargets hitTarget, string photonOwnerNickName)
    {
        if (BulletPrefab != null)
        {
            GameObject currentBullet;
            GetFXPrefab(bulletList, out currentBullet);
            if (currentBullet == null)
                return;
            currentBullet.transform.rotation = Quaternion.LookRotation(endPos - startPos);
            currentBullet.transform.position = startPos;
            currentBullet.GetComponent<BulletBehavior>().SetBulletDirection(endPos - startPos);
            currentBullet.GetComponent<BulletBehavior>().SetOwnerName(photonOwnerNickName);
        }
        if (BulletImpactPrefab != null && hitTarget == HitTargets.Environment)
        {
            GameObject currentBulletImpact;
            GetFXPrefab(bulletImpactList, out currentBulletImpact);
            if (currentBulletImpact == null)
                return;
            currentBulletImpact.transform.rotation = Quaternion.LookRotation(endPos - startPos);
            currentBulletImpact.transform.position = endPos;
        }
        if(BloodSplatterPrefab != null && hitTarget == HitTargets.Player)
        {
            GameObject currentBloodSplatter;
            GetFXPrefab(bloodSplatterList, out currentBloodSplatter);
            if (currentBloodSplatter == null)
                return;
            currentBloodSplatter.transform.rotation = Quaternion.LookRotation(endPos - startPos);
            currentBloodSplatter.transform.position = endPos;
        }
    }
    /**/


    //EVENTS CALLED BY PLAYERS
    public void SpawnBullet_Network(Vector3 startPos, Vector3 endPos, HitTargets hitTarget, string photonOwnerNickName)
    {
        object[] datas = new object[] { startPos, endPos, hitTarget, photonOwnerNickName };

        RaiseEventOptions options = new RaiseEventOptions()
        {
            CachingOption = EventCaching.DoNotCache,
            Receivers = ReceiverGroup.Others
        };

        PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.SpawnBulletEvent, datas, false, options);
    }

    public void SpawnBullet_Local(Vector3 startPos, Vector3 endPos, HitTargets hitTarget, string photonOwnerNickName)
    {
        if (BulletPrefab != null)
        {
            GameObject currentBullet;
            GetFXPrefab(bulletList, out currentBullet);
            if (currentBullet == null)
                return;
            currentBullet.transform.rotation = Quaternion.LookRotation(endPos - startPos);
            currentBullet.transform.position = startPos;
            currentBullet.GetComponent<BulletBehavior>().SetBulletDirection(endPos - startPos);
            currentBullet.GetComponent<BulletBehavior>().SetOwnerName(photonOwnerNickName);
        }
        if (BulletImpactPrefab != null && hitTarget == HitTargets.Environment)
        {
            GameObject currentBulletImpact;
            GetFXPrefab(bulletImpactList, out currentBulletImpact);
            if (currentBulletImpact == null)
                return;
            currentBulletImpact.transform.rotation = Quaternion.LookRotation(endPos - startPos);
            currentBulletImpact.transform.position = endPos;
        }
        if (BloodSplatterPrefab != null && hitTarget == HitTargets.Player)
        {
            GameObject currentBloodSplatter;
            GetFXPrefab(bloodSplatterList, out currentBloodSplatter);
            if (currentBloodSplatter == null)
                return;
            currentBloodSplatter.transform.rotation = Quaternion.LookRotation(endPos - startPos);
            currentBloodSplatter.transform.position = endPos;
        }
    }
    /**/
}
