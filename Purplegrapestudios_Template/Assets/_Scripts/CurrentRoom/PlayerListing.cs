using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListing : MonoBehaviour {

    public PhotonPlayer PhotonPlayer { get; private set; }

    [SerializeField]
    private Text _playerName;
    private Text PlayerName { get { return _playerName; } }

    [SerializeField]
    private Text _playerPing;
    private Text PlayerPing { get { return _playerPing; } }
    private Coroutine ShowPingCoroutine;

    public void ApplyPhotonPlayer(PhotonPlayer photonPlayer)
    {
        PhotonPlayer = photonPlayer;
        PlayerName.text = photonPlayer.NickName;
        //if (ShowPingCoroutine != null)
        //    StopCoroutine(ShowPingCoroutine);
        //ShowPingCoroutine = StartCoroutine(C_ShowPing());
        StartCoroutine(C_ShowPing());
    }

    private IEnumerator C_ShowPing()
    {
        while (PhotonNetwork.connected)
        {
            int ping = (int)PhotonNetwork.player.CustomProperties["Ping"];
            _playerPing.text = ping.ToString();
            yield return new WaitForSeconds(2f);
        }
        yield break;

    }

}
