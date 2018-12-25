using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLight : MonoBehaviour {

    private void Awake()
    {
        TimeManager.Instance.playerLight = GetComponent<Light>();
    }

}
