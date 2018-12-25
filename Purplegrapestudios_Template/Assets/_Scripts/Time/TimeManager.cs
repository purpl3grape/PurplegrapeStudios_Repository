using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeManager : Photon.PunBehaviour {

    public static TimeManager Instance;

    public Transform sunTransform;
    public Light sunLight;
    public Light playerLight;
    public Color[] SunColors;
    public Color[] fogColor;
    public float[] fogDensity;
    float intensity;
    float playerLightIntensity;
    float time;
    float currentTime;
    public float dayIntervalLength;

    IEnumerator TimerCountDownCoroutine;

    public Text timerText;
    int timeCheckValue;

    public double SecondsBeforeStart = 10.0f;
    private const string TimeToStartProp = "st";
    private double timeToStart = 0.0f;

    public bool IsItTimeYet
    {
        get { return IsTimeToStartKnown && PhotonNetwork.time > this.timeToStart; }
    }

    public bool IsTimeToStartKnown
    {
        get { return this.timeToStart > 0.001f; }
    }

    public double SecondsUntilItsTime
    {
        get
        {
            if (this.IsTimeToStartKnown)
            {
                double delta = 0;
                delta = this.timeToStart - PhotonNetwork.time;
                return (delta > 0.0f) ? delta : 0.0f;
            }
            else
            {
                return SecondsBeforeStart;
            }
        }
        set { this.timeToStart = value; }
    }


    IEnumerator TimerCountDown_CO()
    {
        while (!this.IsTimeToStartKnown)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                if (PhotonNetwork.isMasterClient)
                {
                    
                    //The master client checks if a start time is set. we check a min value
                    if (!this.IsTimeToStartKnown && PhotonNetwork.time > 0.0001f)
                    {
                        //No startTime set for room. calculate and set it as property of this room
                        this.timeToStart = PhotonNetwork.time + SecondsBeforeStart;

                        ExitGames.Client.Photon.Hashtable timeProps = new ExitGames.Client.Photon.Hashtable() { {
                                TimeToStartProp,
                                this.timeToStart
                            } };
                        PhotonNetwork.room.SetCustomProperties(timeProps);
                    }
                }
            }
            yield return true;
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (TimerCountDownCoroutine != null)
        {
            StopCoroutine(TimerCountDownCoroutine);
        }
        TimerCountDownCoroutine = TimerCountDown_CO();
        StartCoroutine(TimerCountDownCoroutine);
    }

    private void FixedUpdate()
    {
        if (timeCheckValue != (int)SecondsUntilItsTime)
        {
            timeCheckValue = (int)SecondsUntilItsTime;
            if ((int)SecondsUntilItsTime % 60 >= 0 && (int)SecondsUntilItsTime % 60 < 10)
            {
                timerText.text = ((int)SecondsUntilItsTime / 60) + ":" + "0" + ((int)SecondsUntilItsTime % 60);
            }
            else
            {
                timerText.text = ((int)SecondsUntilItsTime / 60) + ":" + (int)(SecondsUntilItsTime % 60);
            }
        }
        ChangeTimeOfDay();
        if((int)SecondsUntilItsTime == 0)
        {
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.LoadLevel(0);
        }
    }

    void ChangeTimeOfDay()
    {
        currentTime += Time.fixedDeltaTime;
        time = (float)currentTime % dayIntervalLength * (-86400f / dayIntervalLength) + 86400f;

        if (sunTransform)
            sunTransform.rotation = Quaternion.Euler(new Vector3((time - 21600) / 86400 * 360, 0, 0));

        if (time < 43200)
        {
            intensity = 1 - (43200 - time) / 43200;
            playerLightIntensity = ((43200 - time) / 43200 * 3);
        }
        else
        {
            intensity = 1 - ((43200 - time) / 43200 * -1);
            playerLightIntensity = (43200 - time) / 43200 * -1 * 3;
        }


        sunLight.color = Color.Lerp(SunColors[0], SunColors[1], intensity);
        RenderSettings.fogColor = Color.Lerp(fogColor[0], fogColor[1], intensity);
        RenderSettings.fogDensity = Mathf.Lerp(fogDensity[0], fogDensity[1], intensity);
        if (playerLight)
            playerLight.intensity = playerLightIntensity;
    }


    public override void OnPhotonCustomRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(TimeToStartProp))
        {
            this.timeToStart = (double)propertiesThatChanged[TimeToStartProp];
        }
    }
}
