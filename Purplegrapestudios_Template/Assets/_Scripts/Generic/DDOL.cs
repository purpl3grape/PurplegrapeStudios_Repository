using UnityEngine;
using UnityEngine.SceneManagement;

public class DDOL : MonoBehaviour
{

    private static DDOL _instance = null;
    private static DDOL Instance
    {
        get
        {
            if (_instance == null)
            {

                return new GameObject("DDOL").AddComponent<DDOL>();
            }
            else { return _instance; }
        }
    }

    // Use this for initialization
    private void Awake()
    {

        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        if (gameObject.GetComponentInChildren<PlayerNetwork>() != null)
            DontDestroyOnLoad(this);

    }


}
