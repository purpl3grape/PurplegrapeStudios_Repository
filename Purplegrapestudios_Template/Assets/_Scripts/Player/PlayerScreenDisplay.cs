using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScreenDisplay : MonoBehaviour
{
    public static PlayerScreenDisplay Instance;

    public Text Text_CurrentFrameRate;
    public Text Text_Ping;
    public Text Text_PlayerVelocity;
    public Text Text_Timer;
    public Image Image_Crosshair;

    private void Awake()
    {
        Instance = this;
    }
}

