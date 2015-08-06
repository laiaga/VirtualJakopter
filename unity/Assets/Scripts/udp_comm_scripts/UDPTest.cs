using System.Collections.Generic;
using UnityEngine;


public class UDPTest : MonoBehaviour
{
    public GameObject manager;

    private Queue<string> cmdQueue = null;
    //int count = 0;
    int i = 0;
    public void Start()
    {
        cmdQueue = manager.GetComponent<CmdManager>().cmdQueue;

    }
    
    public void Update()
    {
        if(i<200)
        {
            cmdQueue.Enqueue("AT*REF=0,290718208");//takeoff
            i++;
        }
        else if (i < 400)
        {
            cmdQueue.Enqueue("AT*PCMD=131,1,0,0,0,0");
            i++;
        }
        else if(i<500)
        {
            cmdQueue.Enqueue("AT*PCMD=1,1,0,0,0,-1082130432");
            i++;
        }
        else if (i<500)
        {
            cmdQueue.Enqueue("AT*PCMD=1,1,0,0,0,0");
            i++;
        }
        else if(i<700)
        {
            cmdQueue.Enqueue("AT*REF=0,290717696");//land
            i++;
        }
    }
}