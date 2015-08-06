using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

//TODO :
//vérifier que le ctrl_state adéquat est inscrit dans la navdata
//regarder les ctrl_state mineurs et majeurs
//vérifier que les bits du mask sont correctement initialisés (penser à prévoir une tolérance, quadn on regarde si on est en vol ou pas)
//vitesse : en mm/s et pas m/s !
//ctrl state : pas utilisé par jakopter actuellement
//les deux premières navdata envoyées correspondent à la struct "navdata_raw" de jakopter : c'est le mode bootstrap
//une fois le protocole (handshack ?) terminé, c'est la struct "navdata_demo" qui est utilisée

/// <summary>
/// Struct used for navdatas, each sent packet is formed of one or several of these blocks called options
/// cf ARDrone SDK developer guide p.39-40
/// http://www.msh-tools.com/ardrone/ARDrone_Developer_Guide.pdf
/// </summary>
/// UNUSED FOR NOW
struct navdata_option
{
    ushort tag;//tag for a specific option
    ushort size;//length of the structure
    byte[] data;//structure complete with the special tag
}

/// <summary>
/// Datas sent by the drone to Jakopter
/// cf ARDrone SDK developer guide p. 39-40
/// http://www.msh-tools.com/ardrone/ARDrone_Developer_Guide.pdf
/// cf navdata.h and navdata.c in the Jakopter proect
/// </summary>
public struct navdata_demo
{
    public uint header;
    public uint ardrone_state;
    public uint sequence;
    public bool vision_defined;

    public ushort tag;
    public ushort size;

    public uint ctrl_state;                                //flying state (landed, flying, hovering, etc)
    public uint vbat_flying_percentage;                    //battery voltage filtered (mV)

    public float theta;                                    //pitch in milli-degrees
    public float phi;                                      //roll in milli-degrees
    public float psi;                                      //yaw in milli-degrees

    public int altitude;                                   //altitude in centimeters

    public float vx;                                       //linear velocity in millimeters/second
    public float vy;                                       //linear velocity in millimeters/second
    public float vz;                                       //linear velocity in millimeters/second

    public uint num_frames;                                //useless

    public float[] detection_camera_rot;                   //useless
    public float[] detection_camera_trans;                 //useless
    public uint detection_tag_index;                       //useless

    public uint detection_camera_type;                     //type of tag searched in detection

    public float[] drone_camera_rot;                       //useless
    public float[] drone_camera_trans;                     //useless
}


public class NavdataManager : MonoBehaviour
{
    public Queue<navdata_demo> navdataQueue;

    public GameObject parrot = null;

    private navdata_demo datas;
    private DroneControl control = null;

    private uint sequence = 0;
    private Vector3 lastPosition;

    private System.Object sendLock = new System.Object();

    void Start()
    {
        navdataQueue = new Queue<navdata_demo>();
        if (navdataQueue == null) Debug.LogError("[Navdata.cs] Unable to create a Queue");

        control = parrot.GetComponent<DroneControl>();

        datas = new navdata_demo();
        datas.detection_camera_rot = new float[9];
        datas.detection_camera_trans = new float[3];
        datas.drone_camera_rot = new float[9];
        datas.drone_camera_trans = new float[3];

        datas.vbat_flying_percentage = 100;
        datas.header = 0x55667788;

        lastPosition = parrot.transform.position;
    }

    void Update()
    {
        datas.ardrone_state = SetStateMask();
        datas.sequence = sequence;
        datas.size = Convert.ToUInt16(Marshal.SizeOf(datas));
        datas.theta = parrot.transform.eulerAngles.x;
        datas.phi = parrot.transform.eulerAngles.z;
        datas.psi = parrot.transform.eulerAngles.y;
        datas.altitude = Convert.ToInt32(parrot.transform.position.y*1000);

        Vector3 speed = GetSpeed();
        datas.vx = speed.x*1000;
        datas.vy = speed.y*1000;
        datas.vz = speed.z*1000;

        datas.ctrl_state = (uint)control.state;
        sequence++;

        lock(sendLock)
        {
            navdataQueue.Enqueue(datas);
        }

    }

    uint SetStateMask()
    {
        BitArray mask = new BitArray(32);

        if (control.state == State.Landed) mask[0] = true;
        //if(video enabled) mask[1] = true;
        //if(vision enabled) mask[2] = true;
        //if(control algo === angular speed control) mask[3] = true;
        //if(camera disabled) mask[7] = true;
        mask[10] = true;//only send navdata demo
        //11 Navdata bootstrap : (0) options sent in all or demo mode, (1) no navdata options sent
        //12 Motors status : (0) Ok, (1) Motors Com is down
        //13 Communication Lost : (1) com problem, (0) Com is ok
        //15 VBat low : (1) too low, (0) Ok
        mask[25] = true;//navdata thread on
        //26  Video thread ON : (0) thread OFF (1) thread ON
        //31 Emergency landing : (0) no emergency, (1) emergency

        return GetIntFromBitArray(mask);
    }

    Vector3 GetSpeed()
    {
        Vector3 speed;
        
        speed = (parrot.transform.position - lastPosition)/VRTools.GetDeltaTime();

        lastPosition = parrot.transform.position;

        return speed;
    }

    uint GetIntFromBitArray(BitArray bitArray)
    {
        uint value = 0;

        for (int i = 0; i < bitArray.Count; i++)
        {
            if (bitArray[i])
                value += Convert.ToUInt32(Math.Pow(2, i));
        }

        return value;
    }
}