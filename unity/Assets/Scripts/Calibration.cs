using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

/// <summary>
/// Attached to the parrot body collider (which is a trigger), this script checks if the drone is in contact with the ground during 2 seconds as an attempt to calibrate it :
/// the drone has to be immobile and horizontal before receiving any command
/// </summary>
public class Calibration : MonoBehaviour {
    public GameObject ground;
    public GameObject parrot;

    //number of frame during which the drone has been in contact with the ground
    private int frameCount = 0;
    //set to true when the calibration is successfull 
    private bool calibrateOver = false;

    private DroneControl controller;

    void Start()
    {
        controller = parrot.GetComponent<DroneControl>();
    }

    /// <summary>
    /// Called each frame the parrot body's collider is triggered (i.e. intersects) by the ground's collider
    /// Sets calibrateOver to true when 120 frames have passed (~2sec)
    /// </summary>
    /// <param name="c">The collider of the object we are hitting ; it has to be "Ground" for the calibration to be performed</param>
    void OnTriggerStay(Collider c)
    {

        if (c.transform.name == ground.name)
        {
            frameCount++;
            //controller.currentState = State.Landed;
        }
        if (frameCount == 60)
        {
            if(!calibrateOver) calibrateOver = true;
            controller.blockedCmds = false;
        }
    }

    public bool getCalibrateOver()
    {
        return calibrateOver;
    } 
}
