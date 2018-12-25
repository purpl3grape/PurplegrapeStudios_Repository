using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

    public static CameraManager Instance;

    public GameObject SpectatorCameraPrefab;
    public GameObject SpectatorCapsule;
    public GameObject SpectatorCamera;

    public GameObject PlayerCamera;
    GameObject PlayerObject;

    public enum SpectateMode { None, Free };
    public SpectateMode spectateMode;

    // Spectate Camera Variables
    public float smoothing = 7f;
    private Vector3 _target;
    public Vector3 Target;

    Coroutine MoveSpectatorCamera;

    private void Awake()
    {
        Instance = this;

        spectateMode = new SpectateMode();
        SpectatorCapsule = GameObject.Instantiate(SpectatorCameraPrefab);
        SpectatorCamera = SpectatorCapsule.GetComponentInChildren<Camera>().gameObject;
        originalRotation = SpectatorCamera.transform.rotation;
        //SpectatorCamera.SetActive(false);
        SpectatorCamera.transform.position = Vector3.zero;
    }

    // Update is called once per frame
    void Update () {
        if (InputManager.Instance.GetKeyDown(InputCode.Spectate))
        {
            GetPlayerCamera();

            if (spectateMode == SpectateMode.None)
            {
                spectateMode = SpectateMode.Free;
                PlayerCamera.SetActive(false);
                SpectatorCamera.SetActive(true);
                if (MoveSpectatorCamera != null) { StopCoroutine(MoveSpectatorCamera); }
                MoveSpectatorCamera = StartCoroutine(C_MoveInTime(PlayerObject.transform.position + transform.up * 2 - transform.forward * 5));
            }
            else if (spectateMode == SpectateMode.Free)
            {
                spectateMode = SpectateMode.None;
                PlayerCamera.SetActive(true);
                SpectatorCamera.SetActive(false);
            }
        }

        CameraMovement();
        UpdateCameraRotation();

    }

    void GetPlayerCamera()
    {
        if (PlayerCamera == null)
        {
            GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject Player in Players)
            {
                if (Player.GetComponent<PhotonView>().isMine)
                {
                    PlayerCamera = Player.GetComponentInChildren<Camera>().gameObject;
                    PlayerObject = Player;
                }
            }
        }
    }

    void CameraMovement()
    {
        if (spectateMode == SpectateMode.Free)
        {
            if (InputManager.Instance.GetKey(InputCode.Forward))
            {
                SpectatorCapsule.transform.position += SpectatorCamera.transform.forward * 40 * Time.deltaTime;
            }
            else if (InputManager.Instance.GetKey(InputCode.Back))
            {
                SpectatorCapsule.transform.position -= SpectatorCamera.transform.forward * 40 * Time.deltaTime;
            }
            if (InputManager.Instance.GetKey(InputCode.Right))
            {
                SpectatorCapsule.transform.position += SpectatorCapsule.transform.right * 40 * Time.deltaTime;
            }
            else if (InputManager.Instance.GetKey(InputCode.Left))
            {
                SpectatorCapsule.transform.position -= SpectatorCapsule.transform.right * 40 * Time.deltaTime;
            }

            if (InputManager.Instance.GetKey(InputCode.Jump))
            {
                SpectatorCapsule.transform.position += SpectatorCapsule.transform.up * 40 * Time.deltaTime;
            }
        }

    }

    private Vector3 rotateValueX;
    private Vector3 rotateValueY;
    Quaternion originalRotation;
    float rotX, rotY;

    private void UpdateCameraRotation()
    {
        if (spectateMode == SpectateMode.Free)
        {
            rotateValueY = new Vector3(Input.GetAxis("Mouse X") * -1, 0, 0);
            rotateValueX = new Vector3(0, Input.GetAxis("Mouse Y"), 0);
            rotY += Input.GetAxis("Mouse X") * (50 / 3f) * 0.1f * Time.timeScale;
            
            SpectatorCapsule.transform.rotation = Quaternion.Euler(0, rotY, 0); // Rotates the collider

            rotX += Input.GetAxis("Mouse Y") * (50 / 3f) * 0.1f * Time.timeScale;
            rotX = Mathf.Clamp(rotX, -80, 80);
            Quaternion xQuaternion = Quaternion.AngleAxis(rotX, Vector3.left);
            SpectatorCamera.transform.localRotation = originalRotation * xQuaternion;
        }
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        angle = angle % 360;
        if ((angle >= -360F) && (angle <= 360F))
        {
            if (angle < -360F)
            {
                angle += 360F;
            }
            if (angle > 360F)
            {
                angle -= 360F;
            }
        }
        return Mathf.Clamp(angle, min, max);
    }

    IEnumerator C_MoveInTime(Vector3 targetVec)
    {
        while (Vector3.Distance(SpectatorCapsule.transform.position, targetVec) > 0.05f)
        {
            SpectatorCapsule.transform.position = Vector3.Lerp(SpectatorCapsule.transform.position, targetVec, smoothing * Time.fixedDeltaTime);
            yield return null;
        }
        yield break;
    }

    IEnumerator C_MoveWithSpeed(Transform objectToMove, Vector3 a, Vector3 b, float speed)
    {
        float step = (speed / (a - b).magnitude) * Time.fixedDeltaTime;
        float t = 0;
        while (t <= 1.0f)
        {
            t += step; // Goes from 0 to 1, incrementing by step each time
            objectToMove.position = Vector3.Lerp(a, b, t); // Move objectToMove closer to b
            yield return new WaitForFixedUpdate();         // Leave the routine and return here in the next frame
        }
        objectToMove.position = b;
        yield break;
    }
}
