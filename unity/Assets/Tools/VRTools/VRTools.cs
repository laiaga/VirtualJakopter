using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using MiddleVR_Unity3D;

/// <summary>
/// MVR tools. Provides abstraction to interface with MiddleVR creating an
/// abstraction layer between MiddleVR API and the Unity app.
/// 
/// It provides: 
/// 
/// Methods to acces input data from MVR devices (buttons, axis, keyboard).
/// Methods to cluster methods like isMaster() isClient()
/// Methods to create sync objects (buttons, axis)
/// 
/// </summary>

public class VRTools 
{	
	private vrKeyboard _kb = null;
	private List<vrButtons> _buttons = new List<vrButtons>();
	private List<vrAxis> _axis = new List<vrAxis>();
	private List<vrJoystick> _joysticks = new List<vrJoystick>();

    /// <summary>
    /// Singlenton instance
    /// </summary>
    private static VRTools m_Instance = null;

	/// <summary>
	/// Controls whether the singleton has been initialized or not
	/// </summary>
	private bool _init = false;

	/////////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////////
	/// Non-Static
	/////////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////////
	
	/// <summary>
	/// Intializes all the devices defined in the MiddleVR configuration
	/// </summary>
	public void Init()
	{
        if (MiddleVR.VRDeviceMgr != null)
        {
            for (uint w = 0; w < MiddleVR.VRDeviceMgr.GetWandsNb(); w++)
            {
                vrWand wand = MiddleVR.VRDeviceMgr.GetWandByIndex(w);

                vrButtons b = wand.GetButtons();

                if (b != null) 
                {
                    _buttons.Add(b);

                    Debug.Log("[MVRTools] Add button device: " + b.GetName());
                }

                vrAxis axis = wand.GetAxis();

                if (axis != null)
                {
                    _axis.Add(axis);

                    Debug.Log("[MVRTools] Add axis device: " + axis.GetName());
                }
            }

            _kb = MiddleVR.VRDeviceMgr.GetKeyboard();
        }
		
		_init = true;
	}

	/////////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////////
	/// Button Handling
	/////////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Returns whether a button is pressed or not
	/// </summary>
	/// <returns><c>true</c>, the button was pressed, <c>false</c> otherwise.</returns>
	/// <param name="button">Button Id</param>
	public bool IsButtonPressed(uint button, int wand=0)
	{
		if(!_init) Init ();

		if(_buttons.Count != 0)
		{
			if(wand < _buttons.Count)
				return _buttons[wand].IsPressed(button);
			else
				return false;
		}
		else return Input.GetMouseButton((int)button);
	}

	/// <summary>
	/// Returns whether the toggle status of the button has changed or not
	/// </summary>
	/// <returns><c>true</c>, if button was toggled, <c>false</c> otherwise.</returns>
    /// <param name="button">Button Id.</param>
    /// <param name="wand">The interaction wand, 0 by defalt.</param>
    public bool IsButtonToggled(uint button, int wand = 0)
	{
		return IsButtonToggled(button, false, wand);
	}

	/// <summary>
	/// Returns changes in the toogle state of the given button
	/// </summary>
	/// <returns><c>True</c>, if the toggled state has changed, <c>false</c> otherwise.</returns>
	/// <param name="button">Id button</param>
    /// <param name="pressed">Allows to detect when the button has been pressed (true) or released (false)</param>
    /// <param name="wand">The interaction wand, 0 by defalt.</param>
    public bool IsButtonToggled(uint button, bool pressed, int wand=0) 
	{
		if(!_init) Init ();

		if(_buttons.Count != 0)
		{			
			if(wand < _buttons.Count)
				return _buttons[wand].IsToggled(button,pressed);
			else
				return false;
		}
		else //If we do not have buttons we simulate them with the mouse
		{
			if(pressed) return Input.GetMouseButtonDown((int)button);
			else return Input.GetMouseButtonUp((int)button);
		}
	}

    /// <summary>
    /// Returns whether the i-th button of a specific device pressed.
    /// </summary>
    /// <param name="device"></param>
    /// <param name="button"></param>
    /// <returns></returns>
    public static bool IsButtonPressed(string device, uint button)
    {
        if (MiddleVR.VRDeviceMgr != null)
        {
            vrButtons vrb = MiddleVR.VRDeviceMgr.GetButtons(device);

            if (vrb != null)
            {
                return vrb.IsPressed(button);
            }
        }

        return false;
    }

    /// <summary>
    /// Returns the list of all the buttons pressed in the button device
    /// </summary>
    /// <param name="device"></param>
    public static List<uint> IsButtonPressed(string device)
    {
        List<uint> pressedButtons = new List<uint>();

        if (MiddleVR.VRDeviceMgr != null)
        {
            vrButtons vrb = MiddleVR.VRDeviceMgr.GetButtons(device);

            if (vrb != null)
            {
                for (uint i = 0; i < vrb.GetButtonsNb(); i++)
                {
                    if (vrb.IsPressed(i)) pressedButtons.Add(i);
                }
            }
        }

        return pressedButtons;
    }

    /// <summary>
    /// Creates a new button device for synchorizing values among clients </summary>
    /// <returns>The pointer to the device</returns>
    /// <param name="name">Name of the device.</param>
    /// <param name="numButtons">Number of buttons of the device.</param>
    public static vrButtons CreateSyncButtonDevice(string name, uint numButtons)
    {
        if (MiddleVR.VRDeviceMgr == null) return null;

        vrButtons vrb = MiddleVR.VRDeviceMgr.CreateButtons(name);

        if (vrb != null)
        {
            vrb.SetButtonsNb(numButtons);

            MiddleVRTools.Log("[+] Created shared event button " + name);

            MiddleVR.VRClusterMgr.AddSynchronizedObject(vrb, 0);
        }
        else
        {
            MiddleVRTools.Log("[!] Error creating a shared event button " + name);
        }

        return vrb;
    }

    /// <summary>
    /// Enables the change of the button state of a vrButton. It should be used
    /// only for synch purposes. If the device does not exist it will create a
    /// new shared button device
    /// </summary>
    /// <param name="device">The string representing the device.</param>
    /// <param name="button">The button id which has to be updated.</param>
    /// <param name="state">The new state of the button</param>
    public static void SetButtonState(string device, uint button, bool state)
    {
        if (MiddleVR.VRDeviceMgr == null) return;
        
        vrButtons vrb = MiddleVR.VRDeviceMgr.GetButtons(device);

        if (vrb != null) vrb.SetPressedState(button, state);        
    }

	/////////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////////
	/// Axis Handling
	/////////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////////

    public float GetWandAxisValue(uint axis, int wand = 0)
	{
		if(!_init) Init ();

        if (_axis.Count > 0 && _axis.Count > wand) 
		{
            return _axis[wand].GetValue(axis);
		}
        else if (_joysticks.Count != 0)
        {
            return _joysticks[wand].GetAxisValue(axis);
        }
        else
		{
			if(axis == 1) return Input.GetAxis ("Vertical");
			else if(axis == 0) return Input.GetAxis ("Horizontal");
			else if(axis == 2) return Input.GetAxis ("Gear");
			else return 0;
		}
	}

    /////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////
    /// Keyboard Handling
    /////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Returns whether the key has been pressed
	/// </summary>
	/// <returns><c>true</c>, when the key has been pressed, <c>false</c> otherwise.</returns>
	/// <param name="key">Unity keycode</param>
	public bool GetKeyDown(KeyCode key)
	{
		if(!_init) Init ();

		if(_kb!=null) return _kb.IsKeyToggled(translateKeyCode(key));
		else return Input.GetKeyDown(key);
	}

    /// <summary>
    /// Returns whether the key has been released
    /// </summary>
    /// <returns><c>true</c>, the key has been released, <c>false</c> otherwise.</returns>
    /// <param name="key">Unity keycode</param>
    public bool GetKeyUp(KeyCode key)
    {
        if (!_init) Init();

        if (_kb != null) return _kb.IsKeyToggled(translateKeyCode(key),false);
        else return Input.GetKeyUp(key);
    }

    /// <summary>
    /// Returns whether the key is currently pressed
    /// </summary>
    /// <returns><c>true</c>, if the key is down, <c>false</c> otherwise.</returns>
    /// <param name="key">Unity keycode</param>
    public bool GetKeyPressed(KeyCode key)
    {
        if (!_init) Init();

        if (_kb != null) return _kb.IsKeyPressed(translateKeyCode(key));
        else return Input.GetKey(key);
    }	
	
	/////////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////////
	/// Static
	/////////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Gets the instance.
	/// </summary>
	/// <returns>The instance.</returns>
	public static VRTools GetInstance()
	{
		if(m_Instance == null) m_Instance = new VRTools();

		return m_Instance;
	}

	/// <summary>
	/// Returns true if the unity client is a cluster client.
	/// </summary>
	/// <returns><c>true</c>, if client, <c>false</c> otherwise.</returns>
	public static bool IsClient()
	{
		if( MiddleVR.VRClusterMgr == null )	return false;
		
		return MiddleVR.VRClusterMgr.IsCluster() && MiddleVR.VRClusterMgr.IsClient();
	}

    /// <summary>
    /// Returns true if the unity client is the master server or there is no cluster.
    /// </summary>
    /// <returns><c>true</c>, if client, <c>false</c> otherwise.</returns>
    public static bool IsMaster()
    {
        if (MiddleVR.VRClusterMgr == null) return true;

        return !MiddleVR.VRClusterMgr.IsCluster() || MiddleVR.VRClusterMgr.IsServer();
    }

	/// <summary>
	/// Gets the delta time. Must be used instead of Time.delaTime!
	/// </summary>
	/// <returns>The delta time.</returns>
	public static float GetDeltaTime()
	{
		if(MiddleVR.VRKernel != null)
			return (float)(MiddleVR.VRKernel.GetDeltaTime());
		else
			return Time.deltaTime;	
	}

    /// <summary>
    /// Gets the time at the beginning of the last frame. Must be used instead of Time.time!
    /// </summary>
    /// <returns>The delta time.</returns>
    public static float GetTime()
    {
        if (MiddleVR.VRKernel != null)
            return (float)(MiddleVR.VRKernel.GetTime()/1000);
        else
            return Time.time;
    }

	/// <summary>
	/// Translates Unity key code to the MiddleVR KeyCode
	/// </summary>
	/// <returns>The MiddleVR key code.</returns>
	/// <param name="k">Unity Keycode to translate</param>
	private static uint translateKeyCode(KeyCode k)
	{
		switch(k)
		{			
			case KeyCode.A:	return MiddleVR.VRK_A;
			case KeyCode.B:	return MiddleVR.VRK_B;
			case KeyCode.C:	return MiddleVR.VRK_C;
			case KeyCode.D:	return MiddleVR.VRK_D;
			case KeyCode.E: return MiddleVR.VRK_E;
			case KeyCode.F:	return MiddleVR.VRK_F;
			case KeyCode.G:	return MiddleVR.VRK_G;
			case KeyCode.H: return MiddleVR.VRK_H;
			case KeyCode.I:	return MiddleVR.VRK_I;
			case KeyCode.J:	return MiddleVR.VRK_J;
			case KeyCode.K:	return MiddleVR.VRK_K;
			case KeyCode.L:	return MiddleVR.VRK_L;
			case KeyCode.M:	return MiddleVR.VRK_M;
			case KeyCode.N:	return MiddleVR.VRK_N;
			case KeyCode.O:	return MiddleVR.VRK_O;
			case KeyCode.P:	return MiddleVR.VRK_P;
			case KeyCode.Q:	return MiddleVR.VRK_Q;
			case KeyCode.R:	return MiddleVR.VRK_R;
			case KeyCode.S:	return MiddleVR.VRK_S;
			case KeyCode.T:	return MiddleVR.VRK_T;
			case KeyCode.U:	return MiddleVR.VRK_U;
			case KeyCode.V:	return MiddleVR.VRK_V;
			case KeyCode.W:	return MiddleVR.VRK_W;
			case KeyCode.X:	return MiddleVR.VRK_X;
			case KeyCode.Y:	return MiddleVR.VRK_Y;
			case KeyCode.Z:	return MiddleVR.VRK_Z;
			case KeyCode.Alpha1:return MiddleVR.VRK_1;
			case KeyCode.Alpha2:return MiddleVR.VRK_2;
			case KeyCode.Alpha3:return MiddleVR.VRK_3;
			case KeyCode.Alpha4:return MiddleVR.VRK_4;
			case KeyCode.Alpha5:return MiddleVR.VRK_5;
			case KeyCode.Alpha6:return MiddleVR.VRK_6;
			case KeyCode.Alpha7:return MiddleVR.VRK_7;
			case KeyCode.Alpha8:return MiddleVR.VRK_8;
			case KeyCode.Alpha9:return MiddleVR.VRK_9;
			case KeyCode.Alpha0:return MiddleVR.VRK_0;
			case KeyCode.Space:return MiddleVR.VRK_SPACE;
			default:
			return MiddleVR.VRK_ESCAPE;
		}
	}
}
