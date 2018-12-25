using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoom : MonoBehaviour {

    [SerializeField]
    private Text _roomName;
    private Text RoomName { get { return _roomName; } }

    public LobbyNetwork LobbyNetwork;

    public void OnClick_ConnectionStatusChange()
    {
        if (LobbyNetwork.connectionType.Equals(LobbyNetwork.ConnectionType.MultiPlayer))
        {
            LobbyNetwork.connectionType = LobbyNetwork.ConnectionType.SinglePlayer;
            LobbyNetwork.ConnectionStatus.text = LobbyNetwork.ConnectionType.SinglePlayer.ToString();
            if (PhotonNetwork.connected)
            {
                print("Single Player ");
                PhotonNetwork.Disconnect();
                PhotonNetwork.LoadLevel(0);
            }
        }
        else
        {
            LobbyNetwork.ConnectionStatus.text = LobbyNetwork.ConnectionType.MultiPlayer.ToString();
            LobbyNetwork.connectionType = LobbyNetwork.ConnectionType.MultiPlayer;
            PhotonNetwork.Disconnect();
            if (!PhotonNetwork.connected)
            {
                print("Connecting to server..");
                PhotonNetwork.ConnectUsingSettings("0.0.0");
            }
        }
    }

    public void OnClick_CreateRoom()
    {
        RoomOptions roomOptions;
        if (PhotonNetwork.offlineMode == true)
        {
            roomOptions = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = 1 };
        }
        else
        {
            roomOptions = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = 10 };
        }

        if (PhotonNetwork.CreateRoom(RoomName.text, roomOptions, TypedLobby.Default))
        {
            print("Create room successfully sent.");
        }
        else
        {
            print("Create Failed to send");
        }
    }

}