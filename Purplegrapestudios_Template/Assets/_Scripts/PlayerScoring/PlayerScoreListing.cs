using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScoreListing : MonoBehaviour {

    public PhotonPlayer PhotonPlayer { get; private set; }

    [SerializeField]
    private Text _playerName;
    public Text PlayerName { get { return _playerName; } set { _playerName = value; } }

    [SerializeField]
    private Text _playerHealth;
    public Text PlayerHealth { get { return _playerHealth; } set { _playerHealth = value; } }

    [SerializeField]
    private Text _playerKills;
    public Text PlayerKills { get { return _playerKills; } set { _playerKills = value; } }

    [SerializeField]
    private Text _playerDeaths;
    public Text PlayerDeaths { get { return _playerDeaths; } set { _playerDeaths = value; } }

    [SerializeField]
    private Text _playerDamageDealt;
    public Text PlayerDamageDealt { get { return _playerDamageDealt; } set { _playerDamageDealt = value; } }

    [SerializeField]
    private Text _playerDamageReceived;
    public Text PlayerDamageReceived { get { return _playerDamageReceived; } set { _playerDamageReceived = value; } }

}
