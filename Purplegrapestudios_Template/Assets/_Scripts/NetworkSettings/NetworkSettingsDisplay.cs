using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkSettingsDisplay : MonoBehaviour {

    public static NetworkSettingsDisplay Instance;

    private string PredictionMultiplyerPlayerPref = "PredictionMultiplierValue";
    private string VelocityPredictionValuePlayerPref = "VelocityPredictionValue";
    private string SyncMultiplierValuePlayerPref = "SyncMultiplierValue";
    private string SyncDistancePlayerPref = "SyncDistance";
    private string SyncYAxisValuePlayerPref = "SyncYAxisValue";

    public Slider PredictionMultiplierSlider;
    public Slider VelocityMultiplierSlider;
    public Slider syncMultiplierSlider;
    public Slider syncDistanceSlider;
    public Slider SyncYAxisSlider;
    public Slider FinalSyncDistanceSlider;
    public Slider FinalSyncMultiplierSlider;
    public Slider FrameRateSlider;
    public Slider VsyncSlider;

    public Text Text_PredictionMultiplier;
    public Text Text_VelocityMultiplier;
    public Text Text_syncMultiplier;
    public Text Text_syncDistance;
    public Text Text_SyncYAxis;
    public Text Text_FinalSyncDistance;
    public Text Text_FinalSyncMultiplier;
    public Text Text_FrameRate;
    public Text Text_VsyncCount;

    [SerializeField]
    float _predictionMultiplierValue;
    public float PredictionMultiplierValue { get { return _predictionMultiplierValue; } set { _predictionMultiplierValue = value; } }
    [SerializeField]
    float _velocityMultiplierValue;
    public float VelocityMultiplierValue { get { return _velocityMultiplierValue; } set { _velocityMultiplierValue = value; } }
    [SerializeField]
    float _syncMultiplierValue;
    public float SyncMultiplierValue { get { return _syncMultiplierValue; } set { _syncMultiplierValue = value; } }
    [SerializeField]
    float _syncDistanceValue;
    public float SyncDistanceValue { get { return _syncDistanceValue; } set { SyncDistanceValue = value; } }
    [SerializeField]
    float _syncYAxisValue;
    public float SyncYAxisValue { get { return _syncYAxisValue; } set { _syncYAxisValue = value; } }

    [SerializeField]
    float _finalSyncDistanceValue;
    public float FinalSyncDistanceValue { get { return _finalSyncDistanceValue; } set { _finalSyncDistanceValue = value; } }

    [SerializeField]
    float _finalSyncMultiplierValue;
    public float FinalSyncMultiplierValue { get { return _finalSyncMultiplierValue; } set { _finalSyncMultiplierValue = value; } }

    [SerializeField]
    float _frameRate;
    public float FrameRate { get { return _frameRate; } set { _frameRate = value; } }

    [SerializeField]
    float _vsyncCount;
    public float VsyncCount { get { return _vsyncCount; } set { _vsyncCount = value; } }

    private void Awake()
    {
        Instance = this;
    }

    public void OnSlider_PredictionMultiplierChange(float value)
    {
        _predictionMultiplierValue = value;
        Text_PredictionMultiplier.text = _predictionMultiplierValue.ToString();
        //PlayerPrefs.SetFloat(PredictionMultiplyerPlayerPref, value);
        //PlayerPrefs.Save();
    }

    public void OnSlider_VelocityMultiplierChange(float value)
    {
        _velocityMultiplierValue = value;
        Text_VelocityMultiplier.text = _velocityMultiplierValue.ToString();
        //PlayerPrefs.SetFloat(VelocityPredictionValuePlayerPref, value);
        //PlayerPrefs.Save();
    }

    public void OnSlider_SyncMultiplierChange(float value)
    {
        _syncMultiplierValue = value;
        Text_syncMultiplier.text = _syncMultiplierValue.ToString();
        //PlayerPrefs.SetFloat(SyncMultiplierValuePlayerPref, value);
        //PlayerPrefs.Save();
    }

    public void OnSlider_SyncDistanceChange(float value)
    {
        _syncDistanceValue = value;
        Text_syncDistance.text = _syncDistanceValue.ToString();
        //PlayerPrefs.SetFloat(SyncDistancePlayerPref, value);
        //PlayerPrefs.Save();
    }

    public void OnSlider_SyncYAxisChange(float value)
    {
        _syncYAxisValue = value;
        Text_SyncYAxis.text = _syncYAxisValue.ToString();
        //PlayerPrefs.SetFloat(SyncYAxisValuePlayerPref, value);
        //PlayerPrefs.Save();
    }

    public void OnSlider_FinalSyncDistanceChange(float value)
    {
        _finalSyncDistanceValue = value;
        Text_FinalSyncDistance.text = _finalSyncDistanceValue.ToString();
        //PlayerPrefs.SetFloat(SyncYAxisValuePlayerPref, value);
        //PlayerPrefs.Save();
    }

    public void OnSlider_FinalSyncMultiplierChange(float value)
    {
        _finalSyncMultiplierValue = value;
        Text_FinalSyncMultiplier.text = _finalSyncMultiplierValue.ToString();
        //PlayerPrefs.SetFloat(SyncYAxisValuePlayerPref, value);
        //PlayerPrefs.Save();
    }

    public void OnSlider_FrameRateChange(float value)
    {
        _frameRate = value;
        NetworkSettingsDisplay.Instance.Text_FrameRate.text = _frameRate.ToString();

        Application.targetFrameRate = (int)_frameRate;
        //Time.fixedDeltaTime = 1 / (float)Application.targetFrameRate;

        //PlayerPrefs.SetFloat(SyncYAxisValuePlayerPref, value);
        //PlayerPrefs.Save();
    }

    public void OnSlider_VsyncChange(float value)
    {
        QualitySettings.vSyncCount = (int)value;
        NetworkSettingsDisplay.Instance.VsyncSlider.value = (int)value;
        NetworkSettingsDisplay.Instance.Text_VsyncCount.text = ((int)value).ToString();

    }

    public void InitNetworkSettings()
    {
        _predictionMultiplierValue = 1f;
        NetworkSettingsDisplay.Instance.PredictionMultiplierSlider.value = PredictionMultiplierValue;
        NetworkSettingsDisplay.Instance.Text_PredictionMultiplier.text = PredictionMultiplierValue.ToString();

        _velocityMultiplierValue = 1;
        NetworkSettingsDisplay.Instance.VelocityMultiplierSlider.value = VelocityMultiplierValue;
        NetworkSettingsDisplay.Instance.Text_VelocityMultiplier.text = VelocityMultiplierValue.ToString();

        _syncMultiplierValue = 1;
        NetworkSettingsDisplay.Instance.syncMultiplierSlider.value = _syncMultiplierValue;
        NetworkSettingsDisplay.Instance.Text_syncMultiplier.text = SyncMultiplierValue.ToString();

        _syncDistanceValue = 100;
        NetworkSettingsDisplay.Instance.syncDistanceSlider.value = SyncDistanceValue;
        NetworkSettingsDisplay.Instance.Text_syncDistance.text = SyncDistanceValue.ToString();

        _syncYAxisValue = 0.1f;
        NetworkSettingsDisplay.Instance.SyncYAxisSlider.value = SyncYAxisValue;
        NetworkSettingsDisplay.Instance.Text_SyncYAxis.text = SyncYAxisValue.ToString();

        _finalSyncDistanceValue = 5f;
        NetworkSettingsDisplay.Instance.FinalSyncDistanceSlider.value = _finalSyncDistanceValue;
        NetworkSettingsDisplay.Instance.Text_FinalSyncDistance.text = _finalSyncDistanceValue.ToString();

        _finalSyncMultiplierValue = 1f;
        NetworkSettingsDisplay.Instance.FinalSyncMultiplierSlider.value = _finalSyncMultiplierValue;
        NetworkSettingsDisplay.Instance.Text_FinalSyncMultiplier.text = _finalSyncMultiplierValue.ToString();

        _frameRate = 60f;
        Application.targetFrameRate = (int)_frameRate;
        Time.fixedDeltaTime = 1 / (float)Application.targetFrameRate;
        NetworkSettingsDisplay.Instance.FrameRateSlider.value = _frameRate;
        NetworkSettingsDisplay.Instance.Text_FrameRate.text = _frameRate.ToString();

        _vsyncCount = 0;
        QualitySettings.vSyncCount = 0;
        NetworkSettingsDisplay.Instance.VsyncSlider.value = _vsyncCount;
        NetworkSettingsDisplay.Instance.Text_VsyncCount.text = _vsyncCount.ToString();
    }
}
