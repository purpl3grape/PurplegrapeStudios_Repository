/*
 * using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

public class ChangePostProcess : MonoBehaviour {

    public PostProcessingProfile normal, water;
    public PostProcessingBehaviour cameraPostProcessingBehavior;

    public GameObject WaterObject;
    public GameObject WaterBehaviorObject;

    public Material waterMaterial;
    private int waveNumber;
    public float distanceX, distanceZ;
    public float[] waveAmplitude;
    public float magnitudeDivider;
    public Vector2[] impactPos;
    public float[] distance;
    public float speedWaveSpread;

    Mesh mesh;


    PhotonView pv;
	// Use this for initialization
	void Start () {
        pv = GetComponent<PhotonView>();
        WaterObject = GameObject.FindGameObjectWithTag("Water");
        WaterBehaviorObject = GameObject.FindObjectOfType<CollisionScript>().gameObject;
        waterMaterial = WaterBehaviorObject.GetComponent<MeshRenderer>().material;
        mesh = WaterObject.GetComponent<MeshFilter>().mesh;
    }

    private void Update()
    {
        for (int i = 0; i < 8; i++)
        {

            waveAmplitude[i] = waterMaterial.GetFloat("_WaveAmplitude" + (i + 1));
            if (waveAmplitude[i] > 0)

            {
                distance[i] += speedWaveSpread;
                waterMaterial.SetFloat("_Distance" + (i + 1), distance[i]);
                waterMaterial.SetFloat("_WaveAmplitude" + (i + 1), waveAmplitude[i] * 0.98f);
            }
            if (waveAmplitude[i] < 0.01)
            {
                waterMaterial.SetFloat("_WaveAmplitude" + (i + 1), 0);
                distance[i] = 0;
            }

        }

    }

    void SplashTrigger()
    {
        waveNumber++;
        if (waveNumber == 9)
        {
            waveNumber = 1;
        }
        waveAmplitude[waveNumber - 1] = 0;
        distance[waveNumber - 1] = 0;

        distanceX = WaterObject.transform.position.x - transform.position.x;
        distanceZ = WaterObject.transform.position.z - transform.position.z;
        impactPos[waveNumber - 1].x = transform.position.x;
        impactPos[waveNumber - 1].y = transform.position.z;

        waterMaterial.SetFloat("_xImpact" + waveNumber, transform.position.x);
        waterMaterial.SetFloat("_zImpact" + waveNumber, transform.position.z);

        waterMaterial.SetFloat("_OffsetX" + waveNumber, distanceX / mesh.bounds.size.x * 2.5f);
        waterMaterial.SetFloat("_OffsetZ" + waveNumber, distanceZ / mesh.bounds.size.z * 2.5f);

        waterMaterial.SetFloat("_WaveAmplitude" + waveNumber, 50 * magnitudeDivider);

    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Water") && pv.isMine)
        {
            cameraPostProcessingBehavior.profile = water;
            //GetComponent<PlayerMovement>().jumpSpeed = 22.5f * 3;
            SplashTrigger();
        }
        //collisionScript.collidedTransform = collider.transform;
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("Water") &&pv.isMine)
        {
            cameraPostProcessingBehavior.profile = normal;
            //GetComponent<PlayerMovement>().jumpSpeed = 22.5f;
            SplashTrigger();
        }
        //collisionScript.collidedTransform = null;
    }
}
*/