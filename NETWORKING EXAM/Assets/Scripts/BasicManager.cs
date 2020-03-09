using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicManager : MonoBehaviour
{
    public static int[] score = new int[2];

    public GameObject realTank;
    public GameObject dummyTank;
    public GameObject bullet;

    public static GameObject T1, T2;

    // Start is called before the first frame update
    void Awake()
    {
        score[0] = 0;
        score[1] = 0;

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
        if (NetworkingManager.MY_ID == 0)
        {
            NetworkingManager.ProcessPackets(T2);
        }
        else
        {
            NetworkingManager.ProcessPackets(T1);
        }
    }
}
