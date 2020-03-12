﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public Vector3 s1;
    public Vector3 s2;


    // Start is called before the first frame update
    void Start()
    {
        BasicManager.T1 = Instantiate(NetworkingManager.tank1, s1, Quaternion.identity);
        BasicManager.T2 = Instantiate(NetworkingManager.tank2, s2, Quaternion.identity * Quaternion.AngleAxis(180, Vector3.up));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
