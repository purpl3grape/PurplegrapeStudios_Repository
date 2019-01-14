/**SUMMARY
 *EventManager Class contains methods to InGame Related events, such as: Kills / Deaths / Damage Dealt / Damage Received /
 *NOTE that in Offline Mode, Photon Events are not called. So that scenario is added to the method that is called by scripts regardless of Photon Connectivity. This will clean up future references of these methods.
 * 
 * TODO: To move away from the [PunRPC] calls, due to events replacing these methods, which are more efficient in terms of computation overhead, as events are the more efficient way of sending a network package.
 */


using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public enum PhotonEventCodes
{
    InitPlayerPosition = 1,
    HealthReset = 2,
    HealthUpdate = 3,
    DamageDealt = 4,
    DamageReceived = 5,
    Kills = 6,
    Deaths = 7,
    RespawnPlayer = 8,
    SetRespawnFalse = 9,
    SetRespawnTrue = 10,
    SpawnBulletEvent = 11,
    FireBulletFX = 12,
    FireProjectileEvent = 13,
    SetBulletParticleActiveEvent = 14,
    SetBulletParticleHitPointEvent = 15,
    BallMovement = 16,
    ColorChange = 17,
    ChangeMovementType = 18,
    DustFX = 19,
}

public enum PlayerStatCodes
{
    Kills = 1,
    Deaths = 2,
    Health = 3,
    DamageDealt = 4,
    DamageReceived = 5,
    KillStreak = 6,
    NameOfLastPlayerKilled = 7,
}

public enum SpawnCharacterType
{
    Player,
    FlightRunner,
}

public class EventManager : MonoBehaviour
{

    Dictionary<string, Dictionary<PlayerStatCodes, int>> playerScores;
    Dictionary<string, Dictionary<PlayerStatCodes, string>> playerKilledLast;
    int changeCounter = 0;

    public static EventManager Instance;


    private void Awake()
    {
        Instance = this;
    }

    void Init()
    {
        if (playerScores != null)
        {
            return;
        }
        playerScores = new Dictionary<string, Dictionary<PlayerStatCodes, int>>();

        if (playerKilledLast != null)
        {
            return;
        }
        playerKilledLast = new Dictionary<string, Dictionary<PlayerStatCodes, string>>();
    }

    void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        RemovePlayer(player.NickName);
    }

    public void RemovePlayer(string playerKey)
    {
        Debug.Log("Player: " + playerKey + " disconnected.");
        if (playerScores != null)
        {
            playerScores.Remove(playerKey);
        }
    }

    public int GetScore(string username, PlayerStatCodes scoreType)
    {
        Init();
        if (playerScores.ContainsKey(username).Equals(false))
        {
            //We have no score record at all for this username
            return 0;
        }

        if (playerScores[username].ContainsKey(scoreType).Equals(false))
        {
            return 0;
        }

        return playerScores[username][scoreType];
    }

    public string GetKilledLast(string username, PlayerStatCodes scoreType)
    {
        Init();
        if (playerKilledLast.ContainsKey(username).Equals(false))
        {
            //We have no score record at all for this username
            return string.Empty;
        }
        if (playerKilledLast[username].ContainsKey(scoreType).Equals(false))
        {
            return string.Empty;
        }
        return playerKilledLast[username][scoreType];
    }

    /// <summary>
    /// Sets the score.
    /// </summary>
    /// <param name="username">Username.</param>
    /// <param name="scoreType">Score type.</param>
    /// <param name="value">Value.</param>
    [PunRPC]
    public void PlayerStatsResetScore(string username)
    {
        Init();
        changeCounter++;
        //KILLS
        if (playerScores.ContainsKey(username).Equals(false))
        {
            playerScores[username] = new Dictionary<PlayerStatCodes, int>();
        }
        playerScores[username][PlayerStatCodes.Kills] = 0;

        //DEATHS
        if (playerScores.ContainsKey(username).Equals(false))
        {
            playerScores[username] = new Dictionary<PlayerStatCodes, int>();
        }
        playerScores[username][PlayerStatCodes.Deaths] = 0;

        //KILLSTREAKS
        if (playerScores.ContainsKey(username).Equals(false))
        {
            playerScores[username] = new Dictionary<PlayerStatCodes, int>();
        }
        playerScores[username][PlayerStatCodes.KillStreak] = 0;

    }

    [PunRPC]
    public void SetScore(string username, PlayerStatCodes scoreType, int value)
    {
        Init();
        changeCounter++;
        if (playerScores.ContainsKey(username).Equals(false))
        {
            playerScores[username] = new Dictionary<PlayerStatCodes, int>();
        }
        playerScores[username][scoreType] = value;
    }

    [PunRPC]
    public void SetScore(string username, PlayerStatCodes scoreType, string value)
    {
        Init();
        changeCounter++;
        if (playerKilledLast.ContainsKey(username).Equals(false))
        {
            playerKilledLast[username] = new Dictionary<PlayerStatCodes, string>();
        }
        playerKilledLast[username][scoreType] = value;
    }

    [PunRPC]
    public void UpdateStatsForKiller(string killerName, int killAmount, string deadPlayerName)
    {
        Init();
        if (playerKilledLast.ContainsKey(killerName).Equals(false))
        {
            playerKilledLast[killerName] = new Dictionary<PlayerStatCodes, string>();
        }
        playerKilledLast[killerName][PlayerStatCodes.NameOfLastPlayerKilled] = deadPlayerName;

        int currScore = GetScore(killerName, PlayerStatCodes.Kills);
        //KILL
        if (playerScores.ContainsKey(killerName).Equals(false))
        {
            playerScores[killerName] = new Dictionary<PlayerStatCodes, int>();
        }
        playerScores[killerName][PlayerStatCodes.Kills] = (currScore + killAmount);

        currScore = GetScore(killerName, PlayerStatCodes.KillStreak);
        //KILLSTREAK
        if (playerScores.ContainsKey(killerName).Equals(false))
        {
            playerScores[killerName] = new Dictionary<PlayerStatCodes, int>();
        }
        playerScores[killerName][PlayerStatCodes.KillStreak] = (currScore + killAmount);
    }

    [PunRPC]
    public void SetLastKilled(string username, PlayerStatCodes scoreType, string value)
    {
        Init();
        if (playerKilledLast.ContainsKey(username).Equals(false))
        {
            playerKilledLast[username] = new Dictionary<PlayerStatCodes, string>();
        }
        playerKilledLast[username][scoreType] = value;
    }

    [PunRPC]
    public void ChangeScore(string username, PlayerStatCodes scoreType, int amount)
    {
        Init();

        int currScore = GetScore(username, scoreType);
        GetComponent<PhotonView>().RPC("SetScore", PhotonTargets.AllBuffered, username, scoreType, currScore + amount);
        //SetScore (username, scoreType, currScore + amount);
    }

    [PunRPC]
    public void ResetScore(string username, string scoreType)
    {
        Init();
        GetComponent<PhotonView>().RPC("SetScore", PhotonTargets.AllBuffered, username, scoreType, 0);
        //SetScore (username, scoreType, currScore + amount);
    }
    [PunRPC]
    public void ResetLastKilled(string username, string scoreType)
    {
        Init();
        GetComponent<PhotonView>().RPC("SetLastKilled", PhotonTargets.AllBuffered, username, scoreType, "");
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

        if (code.Equals(PhotonEventCodes.HealthReset))
        {
            object[] datas = content as object[];
            if (datas.Length.Equals(3))
            {
                SetStat((string)datas[0], (PlayerStatCodes)datas[1], (int)datas[2]);
            }
        }
        else if (code.Equals(PhotonEventCodes.HealthUpdate))
        {
            object[] datas = content as object[];
            if (datas.Length.Equals(3))
            {
                UpdateStat((string)datas[0], (PlayerStatCodes)datas[1], (int)datas[2]);
            }
        }
        else if (code.Equals(PhotonEventCodes.DamageDealt))
        {
            object[] datas = content as object[];
            if (datas.Length.Equals(3))
            {
                UpdateStat((string)datas[0], (PlayerStatCodes)datas[1], (int)datas[2]);
            }
        }
        else if (code.Equals(PhotonEventCodes.DamageReceived))
        {
            object[] datas = content as object[];
            if (datas.Length.Equals(3))
            {
                UpdateStat((string)datas[0], (PlayerStatCodes)datas[1], (int)datas[2]);
            }
        }
        else if (code.Equals(PhotonEventCodes.Kills))
        {
            object[] datas = content as object[];
            if (datas.Length.Equals(3))
            {
                UpdateStat((string)datas[0], (PlayerStatCodes)datas[1], (int)datas[2]);
            }
        }
        else if (code.Equals(PhotonEventCodes.Deaths))
        {
            object[] datas = content as object[];
            if (datas.Length.Equals(3))
            {
                UpdateStat((string)datas[0], (PlayerStatCodes)datas[1], (int)datas[2]);
            }
        }
        else if (code.Equals(PhotonEventCodes.DustFX))
        {
            object[] datas = content as object[];
            if (datas.Length.Equals(1))
            {
                DustFX((bool)datas[0]);
            }
        }
    }

    public void SetStat(string username, PlayerStatCodes scoreType, int value)
    {
        Init();
        changeCounter++;
        if (playerScores.ContainsKey(username).Equals(false))
        {
            playerScores[username] = new Dictionary<PlayerStatCodes, int>();
        }
        playerScores[username][scoreType] = value;
    }
    public void UpdateStat(string username, PlayerStatCodes scoreType, int amount)
    {
        Init();

        int currScore = GetScore(username, scoreType);
        SetStat(username, scoreType, currScore + amount);
    }


    //SPAWNBARRY: Called via SPAWN BUTTON
    [SerializeField] PlayerObjectComponents playerObjectComponents;
    [SerializeField] GameObject player;
    [SerializeField] int PlayerViewID;

    public void SpawnBarry()
    {
        float randomValue = Random.Range(-30f, 30f);
        player = PhotonNetwork.Instantiate(Path.Combine("Prefabs", "NewPlayer"), new Vector3(randomValue, 120, randomValue), Quaternion.identity, 0);
        playerObjectComponents = player.GetComponent<PlayerObjectComponents>();
        PlayerViewID = playerObjectComponents.GetComponent<PhotonView>().viewID;

        playerObjectComponents.ThirdPersonPlayer.SetActive(false);
        //SetLayerRecursively(playerObjectComponents.ThirdPersonPlayer, 10);    //Layer: Player
        if (PhotonNetwork.offlineMode)
        {
            GameObject.FindObjectOfType<CullArea>().gameObject.SetActive(false);
            playerObjectComponents.networkCullingHandler.enabled = false;
        }


        playerObjectComponents.PlayerCamera.SetActive(true);
        playerObjectComponents.FirstPersonPlayer.SetActive(true);

        player.layer = LayerMask.NameToLayer("LocalPlayer");
        player.GetComponent<Rigidbody>().isKinematic = false;

        EventManager.Instance.SetStat_Health(PhotonNetwork.player.NickName, PlayerStatCodes.Health, 100);
        EventManager.Instance.SetStat_Health(PhotonNetwork.player.NickName, PlayerStatCodes.Kills, 0);
        EventManager.Instance.SetStat_Health(PhotonNetwork.player.NickName, PlayerStatCodes.Deaths, 0);
        EventManager.Instance.SetStat_Health(PhotonNetwork.player.NickName, PlayerStatCodes.DamageDealt, 0);
        EventManager.Instance.SetStat_Health(PhotonNetwork.player.NickName, PlayerStatCodes.DamageReceived, 0);

        SpawnPlayerCanvas.Instance.gameObject.SetActive(false);
        CameraManager.Instance.SpectatorCamera.SetActive(false);

    }


    public void DustFX(bool val)
    {
        playerObjectComponents.DustPrefab.SetActive(val);
    }




    //CALLED BY PLAYERS
    //Movement Events
    public void DustFX_Event(bool val)
    {
        object[] datas = new object[] { val };

        RaiseEventOptions options = new RaiseEventOptions()
        {
            CachingOption = EventCaching.DoNotCache,
            Receivers = ReceiverGroup.All
        };

        PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.DustFX, datas, false, options);
    }
    public void DustFX_Local(bool val)
    {
        playerObjectComponents.DustPrefab.SetActive(val);
    }
    //Health updates
    public void SetStat_Health(string username, PlayerStatCodes scoreType, int value)
    {
        object[] datas = new object[] { username, scoreType, value };

        if (PhotonNetwork.offlineMode)
        {
            SetStat(username, scoreType, value);
        }
        else {
            RaiseEventOptions options = new RaiseEventOptions()
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.All
            };

            PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.HealthReset, datas, false, options);
        }
    }
    public void UpdateStat_Health(string username, PlayerStatCodes scoreType, int amount)
    {
        object[] datas = new object[] { username, scoreType, amount };

        if (PhotonNetwork.offlineMode)
        {
            UpdateStat(username, scoreType, amount);
        }
        else
        {
            RaiseEventOptions options = new RaiseEventOptions()
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.All
            };

            PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.HealthUpdate, datas, false, options);
        }
    }
    //Damage updates
    public void SetStat_DamageDealt(string username, PlayerStatCodes scoreType, int value)
    {
        object[] datas = new object[] { username, scoreType, value };

        if (PhotonNetwork.offlineMode)
        {
            SetStat(username, scoreType, value);
        }
        else
        {
            RaiseEventOptions options = new RaiseEventOptions()
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.All
            };

            PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.DamageDealt, datas, false, options);
        }
    }
    public void UpdateStat_DamageDealt(string username, PlayerStatCodes scoreType, int amount)
    {
        object[] datas = new object[] { username, scoreType, amount };

        if (PhotonNetwork.offlineMode)
        {
            SetStat(username, scoreType, amount);
        }
        else
        {
            RaiseEventOptions options = new RaiseEventOptions()
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.All
            };

            PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.DamageDealt, datas, false, options);
        }
    }
    public void SetStat_DamageReceived(string username, PlayerStatCodes scoreType, int value)
    {
        object[] datas = new object[] { username, scoreType, value };

        if (PhotonNetwork.offlineMode)
        {
            SetStat(username, scoreType, value);
        }
        else
        {
            RaiseEventOptions options = new RaiseEventOptions()
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.All
            };

            PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.DamageReceived, datas, false, options);
        }
    }
    public void UpdateStat_DamageReceived(string username, PlayerStatCodes scoreType, int amount)
    {
        object[] datas = new object[] { username, scoreType, amount };

        if (PhotonNetwork.offlineMode)
        {
            SetStat(username, scoreType, amount);
        }
        else
        {
            RaiseEventOptions options = new RaiseEventOptions()
            {
                CachingOption = EventCaching.DoNotCache,
                Receivers = ReceiverGroup.All
            };

            PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.DamageReceived, datas, false, options);
        }
    }
    //KD updates
    public void SetStat_Kills(string username, PlayerStatCodes scoreType, int value)
    {
        object[] datas = new object[] { username, scoreType, value };

        RaiseEventOptions options = new RaiseEventOptions()
        {
            CachingOption = EventCaching.DoNotCache,
            Receivers = ReceiverGroup.All
        };

        PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.Kills, datas, true, options);
    }
    public void UpdateStat_Kills(string username, PlayerStatCodes scoreType, int amount)
    {
        object[] datas = new object[] { username, scoreType, amount };

        RaiseEventOptions options = new RaiseEventOptions()
        {
            CachingOption = EventCaching.DoNotCache,
            Receivers = ReceiverGroup.All
        };

        PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.Kills, datas, true, options);
    }
    public void SetStat_Deaths(string username, PlayerStatCodes scoreType, int value)
    {
        object[] datas = new object[] { username, scoreType, value };

        RaiseEventOptions options = new RaiseEventOptions()
        {
            CachingOption = EventCaching.DoNotCache,
            Receivers = ReceiverGroup.All
        };

        PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.Deaths, datas, true, options);
    }
    public void UpdateStat_Deaths(string username, PlayerStatCodes scoreType, int amount)
    {
        object[] datas = new object[] { username, scoreType, amount };

        RaiseEventOptions options = new RaiseEventOptions()
        {
            CachingOption = EventCaching.DoNotCache,
            Receivers = ReceiverGroup.All
        };

        PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.Deaths, datas, true, options);
    }

    //GETTER FUNCTIONS//
    public string[] GetPlayerNames()
    {
        Init();
        return playerScores.Keys.ToArray();
    }

    public string[] GetPlayerNames(PlayerStatCodes sortingScoreType)
    {
        Init();

        return playerScores.Keys.OrderByDescending(n => GetScore(n, sortingScoreType)).ToArray();
    }

    public int GetChangeCounter()
    {
        return changeCounter;
    }


}
