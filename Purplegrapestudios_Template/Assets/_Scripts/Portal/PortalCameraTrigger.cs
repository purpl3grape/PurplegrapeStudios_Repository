using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalCameraTrigger : MonoBehaviour {

    public GameObject localPlayerObject;
    public GameObject renderPlaneObject;
    public Camera portalCamera;

    private void Start()
    {
        portalCamera.enabled = false;
        renderPlaneObject.SetActive(false);
    }

    // Update is called once per frame
    void Update () {
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!other.GetComponent<PhotonView>().isMine)
                return;
            localPlayerObject = other.gameObject;
            portalCamera.enabled = true;
            renderPlaneObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!other.GetComponent<PhotonView>().isMine)
                return;
            localPlayerObject = null;
            portalCamera.enabled = false;
            renderPlaneObject.SetActive(false);
        }
    }

}
