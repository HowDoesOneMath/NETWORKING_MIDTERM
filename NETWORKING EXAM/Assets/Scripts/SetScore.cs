using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SetScore : MonoBehaviour
{
    Text myText;
    int myID = -1;
    static int compareID = -1;

    // Start is called before the first frame update
    void Start()
    {
        ++compareID;
        myID = compareID;
        myText = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        myText.text = "P" + (myID + 1) + ": " + BasicManager.score[myID];
    }
}
