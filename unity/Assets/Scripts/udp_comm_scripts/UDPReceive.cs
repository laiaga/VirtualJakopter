using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

//et s'il y a lpusieurs commandes dans un packet udp on fait comment ?
public class UDPReceive : MonoBehaviour
{

    public GameObject manager;
    private Queue<string> cmdQueue = null;

    private Thread receiveThread;
    private UdpClient client;
    private System.Object receiveLock = new System.Object();

    //5556 is the default port on wich commands are sent by Jakopter
    private int port = 5556;
    private IPEndPoint anyIP = null;

    // start from unity3d
    public void Start()
    {
        cmdQueue = manager.GetComponent<CmdManager>().cmdQueue;
        anyIP = new IPEndPoint(IPAddress.Any, port);
        init();
    }

    
    // init
    private void init()
    {
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    // receive thread
    private void ReceiveData()
    {
        client = new UdpClient(port);
        while (true)
        {
            lock (receiveLock)
            {
                try
                {
                    byte[] data = client.Receive(ref anyIP);
                    string text = "";
                    text = Encoding.UTF8.GetString(data);
                    cmdQueue.Enqueue(text);
                }
                catch (Exception err)
                {
                    Debug.LogWarning(err.ToString());
                }
            }
        }
    }

    void OnDisable()
    {
        if (receiveThread != null)
        {
            receiveThread.Abort();
        }
        if (client != null)
        {
            client.Close();
        }
    }
}