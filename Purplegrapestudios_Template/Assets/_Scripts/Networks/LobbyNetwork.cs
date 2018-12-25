using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyNetwork : MonoBehaviour {

    public bool OfflineMode;
    public bool ConnectedToMaster;
    public bool JoinedLobby;
    public IEnumerator WaitForConnectionCoroutine;
    public enum ConnectionType { SinglePlayer, MultiPlayer };
    public ConnectionType connectionType;


    [SerializeField]
    private Text _ConnectionStatus;
    public Text ConnectionStatus { get { return _ConnectionStatus; } }

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    // Use this for initialization
    private void Start()
    {
        connectionType = ConnectionType.SinglePlayer;
        ConnectionStatus.text = ConnectionType.SinglePlayer.ToString();
        PhotonNetwork.offlineMode = true;
    }

    private void OnConnectedToMaster()
    {
        print("Connected to master.");
        PhotonNetwork.automaticallySyncScene = false;
        PhotonNetwork.playerName = PlayerNetwork.Instance.PlayerName;
        if (!PhotonNetwork.offlineMode)
            PhotonNetwork.JoinLobby(TypedLobby.Default);
    }

    private void OnJoinedLobby()
    {
        print("Joined Lobby");
        //Makes Current Room Canvas Render over other canvases once we join the room.
        if (!PhotonNetwork.inRoom)
            MainCanvasManager.Instance.LobbyCanvas.transform.SetAsLastSibling();
    }
}
