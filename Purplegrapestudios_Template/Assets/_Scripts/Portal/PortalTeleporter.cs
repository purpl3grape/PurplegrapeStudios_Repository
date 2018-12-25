using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTeleporter : MonoBehaviour {

    public Transform player;
    public Transform receiver;

    private bool playerIsOverlapping = false;
    private float dotProduct;
    private Vector3 portalToPlayer;
    private float rotationDifference;
    private Vector3 positionOffset;

	// Update is called once per frame
	void Update () {

        if (player == null)
        {
            return;
        }

        if (playerIsOverlapping)
        {
            //Dot Product to find out if the Face (Normal) of Teleporter Plane / Flat Box Collider is Overlapped in the Front, NOT the backside of the teleporter face.
            portalToPlayer = player.position - transform.position;
            dotProduct = Vector3.Dot(transform.up, portalToPlayer);

            //If this is true then the player has moved across the portal from the FRONT side. So Teleport him.
            if (dotProduct < 0)
            {
                rotationDifference = Quaternion.Angle(transform.rotation, receiver.rotation);
                rotationDifference += 180;
                player.Rotate(Vector3.up, rotationDifference);

                positionOffset = Quaternion.Euler(0, rotationDifference, 0) * portalToPlayer;
                player.position = receiver.position + positionOffset;

                playerIsOverlapping = false;
            }
        }

	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.GetComponent<PhotonView>().isMine)
            {
                playerIsOverlapping = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.GetComponent<PhotonView>().isMine)
            {
                playerIsOverlapping = false;
            }
        }
    }
}
