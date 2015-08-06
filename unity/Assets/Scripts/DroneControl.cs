using System;
using UnityEngine;

/// <summary>
/// The different states the drone can take ;
/// numbers are set according to the ctrl_states defined in control_states.h in the ardrone sdk
/// </summary>
public enum State { Landed = 2, Hovering = 4, TakingOff = 6, Flying = 3, Landing = 8, Emergency };

public class DroneControl : MonoBehaviour
{
    public delegate void Behavior();
    public Behavior behavior;

    public Rigidbody rigidBody = null;
    public GameObject manager = null;

    public State state = State.Landed;

    //force values used for the drone movments
    //passer en privé quand les bonnes valeurs seront trouvées !
    private float hoveringForce = 9.81f;
    public float takeOffForce = 13.0f;
    public float landForce = 5.0f;
    public float moveForce = 6.5f;

    //time in seconds of any rotation the drone performs
    public double lerpTotalTime = 0.1d;

    //the maximum pitch roll and yaw the drone can have in euler angles
    private float maxAngle = 30.0f;

    //initialized at the begining of a "move" command
    private double lerpStartTime = 0.0d;

    //set to true when the drone enters emergency state : all commands are blocked and drone needs to be reinitialized
    public bool blockedCmds = true;
    public bool emergency = false;

    //the "t" paramater of the lerp called each frame a "move" command is received (modified each frame)
    private double lerpSensibility = 0.0d;

    private CmdManager cmdManager = null;

    //set to true when a new set of "move" commands is received
    //set to false once said set is over
    //"move" commands are of the form AT*PCMD=... and Jakopter always sends several times the same command, hence the need for this bool 
    private bool moveInitialized = false;
    public bool changeMoveInitialized = false;

    //rotations used for the move function
    private Vector3 targetRotationEuler;
    private Vector3 tmpTargetRotationEuler;
    private Quaternion target;
    private Quaternion current;
    private Quaternion from;

    //used for collision detection and deciding if we have landed or not
    private int frameCount = 0;
    private Collision lastCollision = null;

    ////////////////////////////////////////////////////////////////////////////////////////////////
    //                             DEFAULT UNITY FUNCTIONS                                        //
    ////////////////////////////////////////////////////////////////////////////////////////////////

    void Start()
    {
        cmdManager = manager.GetComponent<CmdManager>();
        behavior = Hover;
    }

    /// <summary>
    /// Checks what is the current state of the drone, and calls the appropriate function
    /// </summary>
    void FixedUpdate()
    {
        if (!blockedCmds)
        {
            behavior();
        }
    }

    void OnCollisionStay(Collision c)
    {
        if(lastCollision == null)
        {
            lastCollision = c;
        }
        else if(c.collider.name == lastCollision.collider.name)
        {
            frameCount++;
        }
        else
        {
            lastCollision = c;
            frameCount = 0;
        }

        if(frameCount >= 100)
        {
            IsLanded();
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////
    //                                 CUSTOM FUNCTIONS                                           //
    ////////////////////////////////////////////////////////////////////////////////////////////////


    public void Move()
    {
        state = State.Flying;
        rigidBody.AddRelativeForce(Vector3.up * hoveringForce);
        DroneRotation();
        if (lerpSensibility > 1)
        {
            if (cmdManager.var.fPitch != 0 || cmdManager.var.fRoll != 0)
            {
                rigidBody.AddRelativeForce(Vector3.up * moveForce * (1 + cmdManager.var.fGaz));
            }
        }
    }

    public void TakeOff()
    {
        state = State.TakingOff;
        moveInitialized = false;
        if (transform.position.y < 0.3) rigidBody.AddForce(Vector3.up * takeOffForce);
        else Hover();
    }

    public void Land()
    {
        state = State.Landing;
        moveInitialized = false;
        rigidBody.AddForce(Vector3.up * landForce);
    }

    public void Hover()
    {
        state = State.Hovering;
        moveInitialized = false;
        rigidBody.AddRelativeForce(Vector3.up * hoveringForce);
    }

    public void Emergency()
    {
        state = State.Emergency;
        moveInitialized = false;
        blockedCmds = true;
        emergency = true;
    }

    public void DroneRotation()
    {
        tmpTargetRotationEuler = new Vector3((cmdManager.var.fPitch * maxAngle) % 360, (transform.rotation.eulerAngles.y + (cmdManager.var.fYaw * maxAngle)) % 360, (cmdManager.var.fRoll * maxAngle) % 360);
        if (!moveInitialized && !Vector3.Equals(tmpTargetRotationEuler, targetRotationEuler))
        {
            lerpStartTime = VRTools.GetTime();
            lerpSensibility = 0.0d;
            from = transform.rotation;
			targetRotationEuler = new Vector3((cmdManager.var.fPitch * maxAngle) % 360, (transform.rotation.eulerAngles.y + (cmdManager.var.fYaw * maxAngle)) % 360, (cmdManager.var.fRoll * maxAngle) % 360);
            target = Quaternion.Euler(targetRotationEuler);
            moveInitialized = true;
        }
        if (lerpSensibility <= 1)
        {
            lerpSensibility = (VRTools.GetTime() - lerpStartTime) / lerpTotalTime;
            transform.rotation = Quaternion.Lerp(from, target, (float)lerpSensibility);
        }
        if(lerpSensibility > 1 && changeMoveInitialized)
        {
            changeMoveInitialized = false;
            moveInitialized = false;
        }
    }

    bool IsLanded()
    {
        if (transform.rotation.eulerAngles.x < 10 || transform.rotation.eulerAngles.x > 10)
        {
            if (transform.rotation.eulerAngles.y < 10 || transform.rotation.eulerAngles.y > 10)
            {
                state = State.Landed;
                return true;
            }
        }
        return false;
    }
}