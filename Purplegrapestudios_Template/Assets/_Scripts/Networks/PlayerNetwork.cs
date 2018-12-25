using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections;

[RequireComponent(typeof(PhotonView))]
public class PlayerNetwork : MonoBehaviour {

    public static PlayerNetwork Instance;
    public string PlayerName { get; private set; }
    public int PlayerViewID { get; private set; }
    private PhotonView PhotonView;
    private int PlayersInGame = 0;
    private ExitGames.Client.Photon.Hashtable PlayerCustomProperties = new ExitGames.Client.Photon.Hashtable();
    private Coroutine PingCoroutine;

    public PlayerObjectComponents playerObjectComponents;
    GameObject[] PlayerMeshContainer;

    private static PlayerNetwork _playerNetworkInstance = null;
    private static PlayerNetwork PlayerNetworkInstance 
    {
        get
        {
            if (_playerNetworkInstance == null)
            {

                return new GameObject("PlayerNetwork").AddComponent<PlayerNetwork>();
            }
            else { return _playerNetworkInstance; }
        }
    }

    // Use this for initialization
    private void Awake () {

        if (_playerNetworkInstance != null)
        {
            Destroy(gameObject);
            return;
        }
        _playerNetworkInstance = this;
        DontDestroyOnLoad(gameObject);

        Instance = this;
        PhotonView = GetComponent<PhotonView>();
        PhotonView.viewID = 999;
        PlayerName = "Player " + Random.Range(1000, 9999);

        PhotonNetwork.sendRate = 20;
        PhotonNetwork.sendRateOnSerialize = 10;

        SceneManager.sceneLoaded += OnSceneFinishedLoading;
	}

    private void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode)
    {


        if (scene.name == "Game")
        {
            PlayersInGame = 0;
            if (PhotonNetwork.isMasterClient)
            {
                MasterLoadedGame();
            }
            else
            {
                NonMasterLoadedGame();

            }
        }
    }


    private void MasterLoadedGame()
    {
        PhotonView.RPC("RPC_LoadedGameScene", PhotonTargets.MasterClient, PhotonNetwork.player);
        PhotonView.RPC("RPC_LoadgameOthers", PhotonTargets.Others);
    }

    private void NonMasterLoadedGame()
    {
        PhotonView.RPC("RPC_LoadedGameScene", PhotonTargets.MasterClient, PhotonNetwork.player);
    }

    [PunRPC]
    private void RPC_LoadgameOthers()
    {
        PhotonNetwork.LoadLevel(1);
    }

    [PunRPC]
    private void RPC_LoadedGameScene(PhotonPlayer photonPlayer)
    {
        PlayersInGame++;
        if (PlayersInGame == PhotonNetwork.playerList.Length)
        {
            print("All players are in the game scene.");
        }
    }

    private IEnumerator C_SetPing()
    {
        while (PhotonNetwork.connected)
        {
            PlayerCustomProperties["Ping"] = PhotonNetwork.offlineMode ? 0 : PhotonNetwork.GetPing();
            PhotonNetwork.player.SetCustomProperties(PlayerCustomProperties);

            yield return new WaitForSeconds(5f);
        }
        yield break;
    }

    //When connected to the master server (Photon)
    private void OnConnectedToMaster()
    {
        if (PingCoroutine != null)
            StopCoroutine(PingCoroutine);
        PingCoroutine = StartCoroutine(C_SetPing());

    }

}