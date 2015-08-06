using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

    //Holds the values of movment commands before they
    public struct MovmentVariables
    {
        public float fRoll, fYaw, fGaz, fPitch;
        public int flags;
    }

/// <summary>
/// Manages a Queue containing the commands sent by Jakopter : dequeues them and changes the state of the DroneControl script according to the command read
/// </summary>
public class CmdManager : MonoBehaviour {
    public GameObject parrot;
    public GameObject parrotBody;

    public bool log = false;
    public bool configCmdReceived = false;
    public bool ctrlCmdReceived = false;

    //A queue that stores commands sent by Jakopter
    public Queue<string> cmdQueue = null;
    //A container for the several variables in a movment variables
    public MovmentVariables var;

    //Used to store the commands one by one when they are extracted from cmdQueue to be interpreted
    public string cmdText = null;
    private string previousCmdText = null;
    //A reference to the DroneControl.cs script that contains the drone control functions 
    private DroneControl controller = null;
    //
    private Calibration calibration = null;

    //for debuging purposes
    private StreamWriter w =  null;

    //the time at which the simulation started
    private double startTime = 0.0d;

    private System.Object receiveLock = new System.Object();
	
	void Start () {
        cmdQueue = new Queue<string>();
        if (cmdQueue == null) Debug.LogError("[CmdManager.cs] Unable to create a Queue");

        var = new MovmentVariables();

        controller = parrot.GetComponent<DroneControl>();

        calibration = parrotBody.GetComponent<Calibration>();

        if(log) w = File.AppendText(@"C:\Users\alleonar\Desktop\Nouveau_dossier\log.txt");

        startTime = VRTools.GetTime();
	}
	
	void FixedUpdate () {
        //we check if there is a command waiting, and if the drone has been calibrated, before dequeuing a command
	    if(cmdQueue.Count>0 && calibration.getCalibrateOver())
        {
            lock (receiveLock)
            {
                cmdText = cmdQueue.Dequeue();
                if (w != null)
                {
                    w.WriteLine(VRTools.GetTime() - startTime);
                    w.Write(" --- ");
                    w.Write(cmdText);
                }
                if (cmdText.Contains("FTRIM")) Calibrate();
                if (cmdText.Contains("REF")) BasicBehavior();
                if (cmdText.Contains("PCMD")) Movment();
                if (cmdText.Contains("CONFIG") && cmdText.Contains("navdata_demo\",\"TRUE")) configCmdReceived = true;
                if (cmdText.Contains("CTRL") && cmdText.Contains("5,0")) ctrlCmdReceived = true;
            }
        }
	}

    void Calibrate()
    {
        previousCmdText = null;
        //Debug.Log("calibration command received");
    }

    /// <summary>
    /// Controls the basic behavior of the drone : take-off, landing, emergency stop and reset
    /// </summary>
    void BasicBehavior()
    {
        previousCmdText = null;
        if (cmdText.Contains("290717696"))
        {
            controller.behavior = controller.Land;
        }
        if (cmdText.Contains("290717952"))
        {
            if (controller.emergency)
            {
                Debug.Log("Exiting emergency state");
                controller.behavior = controller.Hover;
                controller.blockedCmds = false;
                controller.emergency = false;
            }
            else
            {
                Debug.Log("Entering emergency state");
                controller.behavior = controller.Emergency;
            }
        }
        if (cmdText.Contains("290718208"))
        {
            controller.behavior = controller.TakeOff;
        }
        if(cmdText.Contains("290720768"))
        {
            Debug.LogWarning("[CmdManager.cs/BasicBehavior] trying to take off and enter/leave emergency state at the same time - undefined behavior");
        }
    }

    /// <summary>
    /// Controls drone movments and rotations
    /// </summary>
    void Movment()
    {
        string pattern = @"AT\*PCMD=\d+,(\d+),(-?)(\d+),(-?)(\d+),(-?)(\d+),(-?)(\d+)\n?";
        Match match = Regex.Match(cmdText, pattern);
        if (match.Success == false) Debug.LogWarning("[CmdManager.cs/Movment] unable to read the command");
        else
        {
            checkPrevious();
            VariablesConversion(match);
            //if(var.fGaz != 0 || var.fYaw != 0 || var.fRoll != 0 || var.fPitch != 0) print(cmdText);
            //The second digits sequence of the command is a 32bits int which authorized values are :
            //0- Hover mode- the drone will stay on top of the same point above the ground;
            //1- Progressive mode- the values of all the progressive command argument will be considered;
            //2- Hover but yaw is calculated automaticaly based on roll => to be avoided;
            //3- Progressive with combined yaw mode- the values of the progressive command arguments will be considered except the yaw argument that will be calculated automatically according to the roll argument;
            switch (var.flags)
            {
                case 0:
                    controller.behavior = controller.Hover;
                    //drone is in hovering mode, nothing happens
                    break;
                case 1:
                    controller.behavior = controller.Move;
                    //progressive mode : all values are used
                    break;
                case 2:
                    Debug.Log("Hover mode 2 (combined yaw flag set to 1)");
                    //drone is in hovering mode, nothing happens 
                    break;
                case 3:
                    Debug.Log("Flying in combined yaw mode...");
                    //progressive mode with combined yaw : yaw is calculated according to roll, others values are used
                    break;
                default:
                    Debug.LogWarning("[CmdManager.cs/Movment] receiving a non-recognized command");
                    break;
            }
        }
    }

    /// <summary>
    /// Gets the movment parameters received via the command as integers
    /// Calculates the float equivalent 
    /// Stores the result in a MovmentVariables struct
    /// </summary>
    /// <param name="match">A collection of the different parameters as they were received</param>
    void VariablesConversion(Match match)
    {
        var.flags = Convert.ToInt32(match.Groups[1].ToString());
        var.fRoll = StringToFloat(match.Groups[2].ToString() + match.Groups[3].ToString());
        var.fPitch = StringToFloat(match.Groups[4].ToString() + match.Groups[5].ToString());
        var.fGaz = StringToFloat(match.Groups[6].ToString() + match.Groups[7].ToString());
        var.fYaw = StringToFloat(match.Groups[8].ToString() + match.Groups[9].ToString());
    }

    float StringToFloat(string s)
    {
        int i = Convert.ToInt32(s);
        byte[] b = BitConverter.GetBytes(i);
        float f = BitConverter.ToSingle(b, 0);

        return f;
    }

    void checkPrevious()
    {
        if (previousCmdText == null) previousCmdText = cmdText;
        if (previousCmdText != cmdText)
        {
            controller.changeMoveInitialized = true;
            previousCmdText = cmdText;
        }
    }
}