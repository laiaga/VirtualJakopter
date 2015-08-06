using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDPSend : MonoBehaviour
{
    public GameObject manager;

    private Thread sendThread;
    private UdpClient client;
    private CmdManager cmdManager;
    private NavdataManager navdataManager;

    //5554 is the default port on wich navigation datas are sent to Jakopter
    private int port = 5554;
    private System.Object sendLock = new System.Object();
    private IPEndPoint anyIP = null;

    private bool stop = false;

    // start from unity3d
    public void Start()
    {
        navdataManager = manager.GetComponent<NavdataManager>();
        cmdManager = manager.GetComponent<CmdManager>();
        anyIP = new IPEndPoint(IPAddress.Any, port);
        if (navdataManager == null) Debug.Log("navdata");
        if (manager == null) Debug.Log("manager");
        client = new UdpClient(port);
        init();
    }


    // init
    private void init()
    {
        sendThread = new Thread(new ThreadStart(SendData));
        sendThread.IsBackground = true;
        sendThread.Start();
    }

    // receive thread
    private void SendData()
    {
        byte[] data;
        NavdataInit();

        while (!stop)
        {
            print("derp");
            lock (sendLock)
            {
                try
                {
                    if (navdataManager.navdataQueue.Count > 0)
                    {
                        data = ToByteArray(navdataManager.navdataQueue.Dequeue());
                        client.Send(data, data.Length, anyIP);
                        client.Receive(ref anyIP);//each time jakopter will receive navdatas, it will send back a ping to maintain the connection active
                    }
                }
                catch (Exception err)
                {
                    print(err.ToString());
                }
            }
        }
    }

    byte[] ToByteArray(navdata_demo obj)
    {
        int len = Marshal.SizeOf(obj);

        byte[] arr = new byte[len];

        IntPtr ptr = Marshal.AllocHGlobal(len);

        Marshal.StructureToPtr(obj, ptr, true);

        Marshal.Copy(ptr, arr, 0, len);

        Marshal.FreeHGlobal(ptr);

        return arr;
    }

    /// <summary>
    /// Navdata handshack that initializes the connection with Jakopter ; cf ARDrone developer guide p.40-41
    /// </summary>
    bool NavdataInit()
    {
        byte[] data;
        byte[] ping;
        string text = null;
        ping = client.Receive(ref anyIP);
        text = Encoding.UTF8.GetString(ping);
        if(!text.Equals("\x01"))
        {
            Debug.LogWarning("[UDPSend.cs/NavdataInit] didn't receive ping from Jakopter");
            return false;
        }
        print("un");
        try
        {
            data = System.Text.Encoding.ASCII.GetBytes("\x00");
            client.Send(data, data.Length, anyIP);
        }
        catch(Exception err)
        {
            Debug.LogWarning("[UDPSend.cs/NavdataInit] couldn't send ping ack to Jakopter : " + err.ToString());
        }

        try
        {
            data = ToByteArray(navdataManager.navdataQueue.Dequeue());
        }
        catch(Exception err)
        {
            Debug.LogWarning("[UDPSend.cs/NavdataInit] couldn't send first navdatas packet to Jakopter  : " + err.ToString());
            return false;
        }
        print("deux");
        while (!cmdManager.configCmdReceived) { }
        print("trois");
        try
        {
            data = ToByteArray(navdataManager.navdataQueue.Dequeue());
            client.Send(data, data.Length, anyIP);
        }
        catch (Exception err)
        {
            Debug.LogWarning("[UDPSend.cs/NavdataInit] couldn't send second navdatas packet to Jakopter " + err.ToString());
            return false;
        }
        print("quatre");
        while (!cmdManager.ctrlCmdReceived) { }
        print("cinq");
        return true;
    }

    void OnDisable()
    {
        stop = true;
    }
}