using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public Vector3 s1;
    public Vector3 s2;


    // Start is called before the first frame update
    void Start()
    {
        Instantiate(NetworkingManager.tank1, s1, Quaternion.identity);
        Instantiate(NetworkingManager.tank2, s1, Quaternion.AngleAxis(180, Vector3.up));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
