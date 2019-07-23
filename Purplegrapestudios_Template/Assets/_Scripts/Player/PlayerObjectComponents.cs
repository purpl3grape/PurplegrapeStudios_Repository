using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObjectComponents : MonoBehaviour
{
    public GameObject CameraConatiner;
    public GameObject PlayerCamera;
    public GameObject DeathCamera;
    public GameObject ThirdPersonPlayer;
    public GameObject FirstPersonPlayer;
    public GameObject BumperCar;
    public GameObject PlayerLight;
    public GameObject DustPrefab;
    public GameObject MiniMapCammera;

    public NetworkCullingHandler networkCullingHandler;
    public NetworkPlayerMovement networkPlayerMovement;
    public PlayerShooting playerShooting;
    public Animator animator1, animator3;

    private void Awake()
    {
        if (GetComponent<PhotonView>().isMine)
            SetLayerRecursively(ThirdPersonPlayer, 10);    //Layer: Player

    }

    //First Person and Third Person Models are on Different Render Layers.
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (null == obj)
        {
            return;
        }

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (null == child)
            {
                continue;
            }
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    //MiniMap Zooming


}