using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCanvas : MonoBehaviour {

    public static GameCanvas Instance;

    private void Awake()
    {
        Instance = this;
    }

}
