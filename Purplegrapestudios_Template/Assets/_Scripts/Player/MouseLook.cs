// original by asteins
// adapted by @torahhorse
// http://wiki.unity3d.com/index.php/SmoothMouseLook

// Instructions:
// There should be one MouseLook script on the Player itself, and another on the camera
// player's MouseLook should use MouseX, camera's MouseLook should use MouseY

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum RotationAxes { MouseX = 1, MouseY = 2 }

public class MouseLook : MonoBehaviour
{
    float sensitivityXY = 0f;

    public RotationAxes axes = RotationAxes.MouseX;
    public bool invertY = false;

    public float sensitivityX = 10F;
    public float sensitivityY = 10F;

    public float minimumX = -60F;
    public float maximumX = 60F;

    public float minimumY = -80F;
    public float maximumY = 80F;

    float rotationX = 0F;
    float rotationY = 0F;

    private List<float> rotArrayX = new List<float>();
    float rotAverageX = 0F;

    private List<float> rotArrayY = new List<float>();
    float rotAverageY = 0F;

    public float framesOfSmoothing = 1;

    Quaternion originalRotation;
    Rigidbody rbody;
    public PlayerMovement playerMovement;
    public PlayerAnimation playerAnimation;


    void Start()
    {

        //sensitivityX = settingsManager.m_sens / 3f;
        //sensitivityY = settingsManager.m_sens / 3f;
        rbody = GetComponent<Rigidbody>();
        if (rbody)
        {
            rbody.freezeRotation = true;
        }

        originalRotation = transform.localRotation;
    }


    void Update()
    {
        if (CameraManager.Instance.spectateMode == CameraManager.SpectateMode.Free)
            return;

        if (playerMovement != null)
        {
            if (playerAnimation.playerCameraView.Equals(PlayerCameraView.ThirdPerson))
            {
                //transform.localRotation = originalRotation;
                //return;
            }
            if (playerMovement.movementType.Equals(MovementType.BumperCar))
            {
                minimumY = -30;
                maximumY = 30;
                //transform.localRotation = originalRotation;
                //return;
            }
            else
            {
                minimumY = -90;
                maximumY = 90;
            }
        }

        if (axes == RotationAxes.MouseX)
        {
            rotAverageX = 0f;

            rotationX += Input.GetAxis("Mouse X") * (50 / 30f) * Time.timeScale;

            rotArrayX.Add(rotationX);

            if (rotArrayX.Count >= framesOfSmoothing)
            {
                rotArrayX.RemoveAt(0);
            }
            for (int i = 0; i < rotArrayX.Count; i++)
            {
                rotAverageX += rotArrayX[i];
            }
            rotAverageX /= rotArrayX.Count;
            rotAverageX = ClampAngle(rotAverageX, minimumX, maximumX);

            Quaternion xQuaternion = Quaternion.AngleAxis(rotAverageX, Vector3.up);
            transform.localRotation = originalRotation * xQuaternion;


        }
        else
        {
            rotAverageY = 0f;

            float invertFlag = 1f;
            if (invertY)
            {
                invertFlag = -1f;
            }
            rotationY += Input.GetAxis("Mouse Y") * (50 / 3f) * 0.2f * invertFlag * Time.timeScale;

            rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

            rotArrayY.Add(rotationY);

            if (rotArrayY.Count >= framesOfSmoothing)
            {
                rotArrayY.RemoveAt(0);
            }
            for (int j = 0; j < rotArrayY.Count; j++)
            {
                rotAverageY += rotArrayY[j];
            }
            rotAverageY /= rotArrayY.Count;

            Quaternion yQuaternion = Quaternion.AngleAxis(rotAverageY, Vector3.left);
            transform.localRotation = originalRotation * yQuaternion;
        }
    }

    public void SetSensitivity(float s)
    {
        sensitivityX = s;
        sensitivityY = s;
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

    public float GetCameraRotationY()
    {
        return rotationY;
    }
}