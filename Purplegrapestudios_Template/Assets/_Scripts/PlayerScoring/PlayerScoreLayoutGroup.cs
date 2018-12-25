using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScoreLayoutGroup : MonoBehaviour {

    [SerializeField]
    private GameObject _playerScoreListingPrefab;
    private GameObject PlayerScoreListingPrefab { get { return _playerScoreListingPrefab; } }
}
