using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class BulletBehavior : MonoBehaviour
{

    Coroutine CoroutineMoveFromTo;
    Transform tr;
    [SerializeField] Rigidbody rbody;
    Vector3 lastPosition;
    int nonItemLayerMask = ~(1 << 9);  //Do not hit itemLayer
    Vector3 direction;
    Ray ray;
    RaycastHit[] bulletHits;
    Transform hitTransform;

    public float rayCastDistance = 5f;
    public float bulletSpeed = 10f;
    public LayerMask LayersToHit;

    [SerializeField] private Vector3 targetPosition;

    [SerializeField] private string ownerName;

    // Use this for initialization
    void Start()
    {
        tr = GetComponent<Transform>();
        rbody = GetComponent<Rigidbody>();
        lastPosition = tr.position;
        bulletHits = new RaycastHit[255];
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        ray = new Ray(lastPosition, direction);


        if (Physics.RaycastNonAlloc(ray, bulletHits, rayCastDistance, nonItemLayerMask) > 0)
        {
            foreach (RaycastHit hit in bulletHits)
            {
                if (hit.collider == null) continue;

                hitTransform = hit.transform;
                if (hitTransform.CompareTag("Player"))
                {
                    if (hitTransform.GetComponent<PhotonView>().owner.NickName != ownerName)
                    {
                        ObjectPoolManager.Instance.DestroyFXPrefab(gameObject, ObjectPoolManager.Instance.bulletList);
                    }
                    else
                    {
                        rbody.MovePosition(rbody.position + direction.normalized * bulletSpeed * Time.fixedDeltaTime);
                    }
                }
                else
                {
                    ObjectPoolManager.Instance.DestroyFXPrefab(gameObject, ObjectPoolManager.Instance.bulletList);
                }
            }
        }
        else
        {
            rbody.MovePosition(rbody.position + direction.normalized * bulletSpeed * Time.fixedDeltaTime);
        }
        lastPosition = rbody.position;
    }

    public void SetBulletDirection(Vector3 dir)
    {
        direction = dir;
    }

    public void SetOwnerName(string name)
    {
        ownerName = name;
    }
}
