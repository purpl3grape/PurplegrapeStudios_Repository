using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class ObtainableBall : Photon.PunBehaviour {

    public Material BallMaterial;
    public Rigidbody RigidBody;
    public bool isBallMoving;
    Coroutine BallMoveCoroutine;

    private void OnKeyDownEnter()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            base.photonView.RequestOwnership();
            NewColor();
        }
    }

    private void OnKeyBallMovement()
    {
        if (Input.GetKeyDown(KeyCode.I) && !isBallMoving)
        {
            isBallMoving = true;

            base.photonView.RequestOwnership();
            MoveBall(transform.position, transform.position + Vector3.forward * 40, 10f);
        }else if (Input.GetKeyDown(KeyCode.K) && !isBallMoving)
        {
            isBallMoving = true;

            base.photonView.RequestOwnership();
            MoveBall(transform.position, transform.position - Vector3.forward * 40, 10f);
        }
        else if (Input.GetKeyDown(KeyCode.J) && !isBallMoving)
        {
            isBallMoving = true;

            base.photonView.RequestOwnership();
            MoveBall(transform.position, transform.position - Vector3.right * 40, 10f);
        }
        else if (Input.GetKeyDown(KeyCode.L) && !isBallMoving)
        {
            isBallMoving = true;

            base.photonView.RequestOwnership();
            MoveBall(transform.position, transform.position + Vector3.right * 40, 10f);
        }
    }

    IEnumerator C_MoveFromTo(Vector3 a, Vector3 b, float speed)
    {

        float step = (speed / (a - b).magnitude) * Time.fixedDeltaTime;
        float t = 0;
        while (t <= 1.0f)
        {
            t += step; // Goes from 0 to 1, incrementing by step each time
            transform.position = Vector3.Lerp(a, b, t); // Move objectToMove closer to b
            yield return new WaitForFixedUpdate();         // Leave the routine and return here in the next frame
        }
        transform.position = b;
        isBallMoving = false;
    }

    private void OnEnable()
    {
        PhotonNetwork.OnEventCall += PhotonNetwork_OnEventCall;
    }

    private void OnDisable()
    {
        PhotonNetwork.OnEventCall -= PhotonNetwork_OnEventCall;
    }

    private void PhotonNetwork_OnEventCall(byte eventCode, object content, int senderId)
    {
        PhotonEventCodes code = (PhotonEventCodes)eventCode;
        if (code == PhotonEventCodes.ColorChange)
        {
            object[] datas = content as object[];
            if (datas.Length == 4)
            {
                if ((int)datas[0] == base.photonView.viewID)
                {
                    BallMaterial.color = new Color((float)datas[1], (float)datas[2], (float)datas[3]);
                }
            }
        }
        if (code == PhotonEventCodes.BallMovement)
        {
            object[] datas = content as object[];
            if (datas.Length == 4)
            {
                if ((int)datas[0] == base.photonView.viewID)
                {
                    BallMoveCoroutine = StartCoroutine(C_MoveFromTo((Vector3)datas[1], (Vector3)datas[2], (float)datas[3]));
                    //float vertical = Input.GetAxis("Vertical");
                    //float horizontal = Input.GetAxis("Horizontal");
                    //transform.position += transform.forward * (vertical * 40 * Time.deltaTime) + transform.right * (horizontal * 40 * Time.deltaTime);
                }
            }
        }
    }

    public override void OnOwnershipRequest(object[] viewAndPlayer)
    {
        PhotonView view = viewAndPlayer[0] as PhotonView;
        PhotonPlayer requestingPlayer = viewAndPlayer[1] as PhotonPlayer;
        base.photonView.TransferOwnership(requestingPlayer);
    }

    private void Update()
    {
        OnKeyBallMovement();

        if (!Cursor.visible) return;
        OnKeyDownEnter();
    }

    private void NewColor() {

        float r, g, b;
        r = Random.Range(0f, 1f);
        g = Random.Range(0f, 1f);
        b = Random.Range(0f, 1f);

        object[] datas = new object[] { base.photonView.viewID, r, g, b };

        RaiseEventOptions options = new RaiseEventOptions()
        {
            CachingOption = EventCaching.DoNotCache,
            Receivers = ReceiverGroup.All
        };

        PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.ColorChange, datas, false, options);

    }

    //Use Photon Events to move the ball around.

    private void MoveBall(Vector3 start, Vector3 end, float speed)
    {

        object[] datas = new object[] { base.photonView.viewID, start, end, speed };

        RaiseEventOptions options = new RaiseEventOptions()
        {
            CachingOption = EventCaching.DoNotCache,
            Receivers = ReceiverGroup.All
        };

        PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.BallMovement, datas, false, options);

    }

}
