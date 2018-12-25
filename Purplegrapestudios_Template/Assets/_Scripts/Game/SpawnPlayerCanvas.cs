using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlayerCanvas : MonoBehaviour
{

    public static SpawnPlayerCanvas Instance;

    private void Awake()
    {
        Instance = this;
    }

}
