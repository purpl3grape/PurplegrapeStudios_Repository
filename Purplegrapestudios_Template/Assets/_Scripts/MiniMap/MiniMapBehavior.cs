using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMapBehavior : Photon.MonoBehaviour
{
    public GameObject MiniMapCanvas;
    public RawImage MiniMapImage;
    public GameObject MiniMap;
    public RenderTexture MiniMapRenderTexture;
    

    public PhotonView PhotonView;

    public GameObject MiniMapCamera;

    // Start is called before the first frame update
    void Awake()
    {
        if (!PhotonView.isMine) return;
        MiniMapCanvas.SetActive(true);
        MiniMapCamera.SetActive(true);

        //MiniMapRenderTextureInstance = GameO
    }

    // Update is called once per frame
    void Update()
    {
        //if (!PhotonView.isMine) return;

        if (!PhotonView.isMine) return;
        MiniMapUpdate();
        MiniMapZoomPlus();
        MiniMapZoomMinus();
    }

    private void MiniMapUpdate()
    {
        //Local Client Minimap Blip Update
        if (GetComponent<NetworkPlayerMovement>().SpawnCharacterType.Equals(SpawnCharacterType.Player))
        {
            //MiniMapCamera = GetComponent<PlayerObjectComponents>().MiniMapCammera;
            //GetComponent<PlayerObjectComponents>().MiniMapCammera.GetComponent<Camera>().targetTexture = MiniMapRenderTexture;

        }
        if (GetComponent<NetworkPlayerMovement>().SpawnCharacterType.Equals(SpawnCharacterType.FlightRunner))
        {
            //MiniMapCamera = GetComponent<FlightRunnerObjectComponents>().MiniMapCammera;
            //GetComponent<FlightRunnerObjectComponents>().MiniMapCammera.GetComponent<Camera>().targetTexture = MiniMapRenderTexture;
        }
        MiniMapImage.GetComponent<RawImage>().texture = MiniMapRenderTexture;

    }

    private void MiniMapZoomPlus()
    {
        if (InputManager.Instance.GetKeyDown(InputCode.MiniMapZoomPlus))
        {
            if (MiniMapCamera.GetComponent<Camera>().orthographicSize < 150)
            {
                MiniMapCamera.GetComponent<Camera>().orthographicSize += 5;
            }
        }
    }

    private void MiniMapZoomMinus()
    {
        if (InputManager.Instance.GetKeyDown(InputCode.MiniMapZoomMinus))
        {
            if (MiniMapCamera.GetComponent<Camera>().orthographicSize > 20)
            {
                MiniMapCamera.GetComponent<Camera>().orthographicSize -= 5;
            }
        }
    }
}
