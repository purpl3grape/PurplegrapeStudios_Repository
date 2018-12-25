using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCollision : MonoBehaviour {

    public PlayerAnimation playerAnimation;
    PhotonView photonView;
    public Transform PlayerCameraTransform;
    MouseLook playerMouseLook;

    public float minDistance = 1.0f;
    public float maxDistance = 4.0f;
    public float smooth = 10.0f;
    Vector3 dollyDir;
    public Vector3 dollyDirAdjusted;
    public float distance;


    Vector3 desiredCameraPos;
    RaycastHit hit;
    Transform tr;

    Ray rayForward;
    Ray rayLeft;
    RaycastHit rayHit;

    public float modZPos;
    public float modYPos;
    public float modXPos;

    private void Awake()
    {
        photonView = playerAnimation.GetComponent<PhotonView>();
        tr = GetComponent<Transform>();
        playerMouseLook = PlayerCameraTransform.GetComponent<MouseLook>();
        dollyDir = tr.localPosition.normalized;
        distance = tr.localPosition.magnitude;
        //playerAnimation.playerCameraView = PlayerCameraView.FirstPerson;

        RestDollyPosition();
        PlayerCameraTransform.localPosition = new Vector3(modXPos, 0.75f - tr.localPosition.y - modYPos, modZPos);
        PlayerCameraTransform.localPosition = new Vector3(-.5f, .375f / 4, -tr.localPosition.z);
    }

    // Update is called once per frame
    void Update () {

        if (!photonView.isMine)
            return;

        if (!playerAnimation.playerCameraView.Equals(PlayerCameraView.ThirdPerson)) {
            PlayerCameraTransform.localPosition = new Vector3(-.5f, .375f/4, -tr.localPosition.z);
            return;
        }
        else
        {
            if (hitLevel)
            {
                modZPos = (maxDistance - .5f);
                modXPos = modZPos / 2;
            }
            else
            {
                modZPos = Mathf.Abs(playerMouseLook.GetCameraRotationY()) / 90f * (maxDistance - .5f);
                modXPos = modZPos / 2;
            }

            if (playerMouseLook.GetCameraRotationY() > 0)
            {
                if (hitLevel)
                {
                    modYPos = Mathf.Abs(playerMouseLook.GetCameraRotationY()) / 90f * (maxDistance - .5f);
                }
                else
                {
                    modYPos = modZPos;
                }
            }
            else
            {
                modYPos = -maxDistance * modZPos / 2;
            }

            PlayerCameraTransform.localPosition = Vector3.Lerp(PlayerCameraTransform.localPosition, new Vector3(modXPos, 0.75f - tr.localPosition.y - modYPos, modZPos), Time.deltaTime * smooth);
        }
        
        desiredCameraPos = tr.parent.TransformPoint(dollyDir * maxDistance * 2);
        if (Physics.Linecast(tr.parent.position, desiredCameraPos, out hit))
        {
            hitLevel = true;
            distance = Mathf.Clamp(hit.distance, minDistance, maxDistance);
            distance -= .5f;
        }
        else
        {
            hitLevel = false;
            distance = maxDistance;
        }


        tr.localPosition = Vector3.Lerp(tr.localPosition, dollyDir * distance, Time.deltaTime * smooth);
        //PlayerCameraTransform.localPosition = new Vector3(0, -2, 0);

    }

    bool hitLevel;


    public void RestDollyPosition()
    {
        distance = maxDistance;
        tr.localPosition = dollyDir * distance;

    }

}
