using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicManager : MonoBehaviour
{
    public GameObject realTank;
    public GameObject dummyTank;
    public GameObject bullet;

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkingManager.MY_ID == 0)
        {
            NetworkingManager.tank1 = realTank;
            NetworkingManager.tank2 = dummyTank;
        }
        else
        {
            NetworkingManager.tank2 = realTank;
            NetworkingManager.tank1 = dummyTank;
        }

        NetworkingManager.bullet = bullet;
    }

    // Update is called once per frame
    void Update()
    {
        NetworkingManager.ProcessPackets();
    }
}
