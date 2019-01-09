using UnityEngine;

public enum InputType
{
    KeyboardAndMouse,
    LogitechG920Wheel
}

public enum DriveType
{
    TwoWheel,
    FourWheel,
    Drift,
}

public enum RearDisplay
{
    Enabled,
    Disabled,
}

public class BootStrap : MonoBehaviour
{
    public GameObject HybridEntityPrefab;
    [HideInInspector] public GameObject hybridEntityObject;

    [Range(30, 120)] public int PhysicsFramesPerSecond;
    public InputType InputType;
    public DriveType DriveType;
    public RearDisplay RearDisplay;
    public BootStrap instance;

    public Mesh mesh;
    public Material material;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (QualitySettings.GetQualityLevel() == 5)
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
        }
        if (QualitySettings.GetQualityLevel() == 4)
        {
            Application.targetFrameRate = 90;
            QualitySettings.vSyncCount = 0;
        }
        if (QualitySettings.GetQualityLevel() == 3)
        {
            Application.targetFrameRate = 144;
            QualitySettings.vSyncCount = 0;
        }
        if (QualitySettings.GetQualityLevel() == 2)
        {
            Application.targetFrameRate = 240;
            QualitySettings.vSyncCount = 0;
        }
        if (QualitySettings.GetQualityLevel() == 1)
        {
            Application.targetFrameRate = 400;
            QualitySettings.vSyncCount = 0;
        }
        if (QualitySettings.GetQualityLevel() == 0)
        {
            Application.targetFrameRate = 999;
            QualitySettings.vSyncCount = 0;
        }

        SpawnCar();
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void SpawnCar()
    {
        hybridEntityObject = GameObject.Instantiate(HybridEntityPrefab, transform.position, Quaternion.identity);
        hybridEntityObject.GetComponent<CarComponent>().instance.Health = 100;
    }

    private void RespawnCar()
    {
        hybridEntityObject.transform.position = new Vector3(100, 0.5f, 100);
        hybridEntityObject.transform.rotation = Quaternion.identity;
        hybridEntityObject.GetComponent<CarComponent>().instance.Health = 100;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (InputType.Equals(InputType.KeyboardAndMouse))
            {
                InputType = InputType.LogitechG920Wheel;
            }
            else
            {
                InputType = InputType.KeyboardAndMouse;
            }
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (DriveType.Equals(DriveType.TwoWheel))
            {
                DriveType = DriveType.FourWheel;
            }
            else if (DriveType.Equals(DriveType.FourWheel))
            {
                DriveType = DriveType.Drift;
            }
            else if (DriveType.Equals(DriveType.Drift))
            {
                DriveType = DriveType.TwoWheel;
            }
        }

        if (hybridEntityObject.GetComponent<CarComponent>().instance.Health <= 0)
        {
            RespawnCar();
        }

    }
}