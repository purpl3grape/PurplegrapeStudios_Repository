using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerManagement : MonoBehaviour {

    public static PlayerManagement Instance;
    private PhotonView PhotonView;

    [SerializeField]
    private GameObject _playerScoreListingPrefab;
    private GameObject PlayerScoreListingPrefab { get { return _playerScoreListingPrefab; } }

    public GameObject PlayerScoreLayoutGroup;

    [SerializeField]
    private Text _playerNameLabel;
    private Text PlayerNameLabel { get { return _playerNameLabel; } }

    [SerializeField] private GameObject[] _playerScoreCache;
    private GameObject[] PlayerScoreCache { get { return _playerScoreCache; } set { _playerScoreCache = value; } }
    int playerScoreListCounter = 0;

    IEnumerator RefreshPlayerStatsCoroutine;

    private void Awake () {

        if (!PhotonNetwork.connected) {
            SceneManager.LoadScene(0);
        }

        Instance = this;
        PhotonView = GetComponent<PhotonView>();
        _playerNameLabel.text = PhotonNetwork.playerName;

	}

    private void InitializePlayerStats(string name)
    {

        GameObject go;

        go = (GameObject)Instantiate(_playerScoreListingPrefab);
        PlayerScoreCache[playerScoreListCounter] = go;
        playerScoreListCounter++;

        go.transform.SetParent(PlayerScoreLayoutGroup.transform, false);
        go.GetComponent<PlayerScoreListing>().PlayerName.text = name;
        go.GetComponent<PlayerScoreListing>().PlayerHealth.text = EventManager.Instance.GetScore(name, PlayerStatCodes.Health).ToString();
        go.GetComponent<PlayerScoreListing>().PlayerKills.text = EventManager.Instance.GetScore(name, PlayerStatCodes.Kills).ToString();
        go.GetComponent<PlayerScoreListing>().PlayerDeaths.text = EventManager.Instance.GetScore(name, PlayerStatCodes.Deaths).ToString();
        go.GetComponent<PlayerScoreListing>().PlayerDamageDealt.text = EventManager.Instance.GetScore(name, PlayerStatCodes.DamageDealt).ToString();
        go.GetComponent<PlayerScoreListing>().PlayerDamageReceived.text = EventManager.Instance.GetScore(name, PlayerStatCodes.DamageReceived).ToString();
    }

    public GameCanvas GameCanvas;

    bool displayStats;
    public void Update()
    {

        if (GameCanvas.gameObject.GetActive() && !displayStats)
        {
            displayStats = true;

            PhotonPlayer[] photonPlayers = PhotonNetwork.playerList;
            string[] names = EventManager.Instance.GetPlayerNames();

            PlayerScoreCache = new GameObject[names.Length];

            Transform c;
            while (PlayerScoreLayoutGroup.transform.childCount > 0)
            {
                c = PlayerScoreLayoutGroup.transform.GetChild(0);
                c.SetParent(null);
                //Destroy(c.gameObject);
                c.gameObject.SetActive(false);
            }

            playerScoreListCounter = 0;

            if (playerScoreListCounter == 0)
            {
                foreach (string name in names)
                {
                    if (name == null)
                    {
                        Debug.Log("No Photon Players Found");
                        Debug.Break();
                    }
                    InitializePlayerStats(name);
                }
            }
            if (RefreshPlayerStatsCoroutine != null)
                StopCoroutine(RefreshPlayerStatsCoroutine);
            RefreshPlayerStatsCoroutine = RefreshPlayerStats_CO(names);
            StartCoroutine(RefreshPlayerStatsCoroutine);

        }
        else if (!GameCanvas.gameObject.GetActive())
        {
            displayStats = false;
        }
    }

    IEnumerator RefreshPlayerStats_CO(string[] names)
    {
        while (displayStats)
        {
            yield return new WaitForSeconds(1f);

            int id = 0;
            foreach (string name in names)
            {
                PlayerScoreCache[id].GetComponent<PlayerScoreListing>().PlayerName.text = name;
                PlayerScoreCache[id].GetComponent<PlayerScoreListing>().PlayerHealth.text = EventManager.Instance.GetScore(name, PlayerStatCodes.Health).ToString();
                PlayerScoreCache[id].GetComponent<PlayerScoreListing>().PlayerKills.text = EventManager.Instance.GetScore(name, PlayerStatCodes.Kills).ToString();
                PlayerScoreCache[id].GetComponent<PlayerScoreListing>().PlayerDeaths.text = EventManager.Instance.GetScore(name, PlayerStatCodes.Deaths).ToString();
                PlayerScoreCache[id].GetComponent<PlayerScoreListing>().PlayerDamageDealt.text = EventManager.Instance.GetScore(name, PlayerStatCodes.DamageDealt).ToString();
                PlayerScoreCache[id].GetComponent<PlayerScoreListing>().PlayerDamageReceived.text = EventManager.Instance.GetScore(name, PlayerStatCodes.DamageReceived).ToString();
                id++;
            }
        }

        RefreshPlayerStatsCoroutine = null;
    }

}

