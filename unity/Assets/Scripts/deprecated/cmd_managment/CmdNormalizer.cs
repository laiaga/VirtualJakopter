using UnityEngine;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

/// <summary>
/// This script opens the log file created by the script UDPReceive, and crates a new one with the same relevant content but without blank lines 
/// </summary>
public class CmdNormalizer : MonoBehaviour {


    private StreamWriter logWriterNormalized = null;
    private StreamReader logReader = null;


    private Regex regex;

    private int lineNumber = 0;

	void Start () 
    {
        logReader = GameObject.Find("Shared Variables").GetComponent<SharedVariables>().logReader;
        if (logReader == null) Debug.LogError("Unable to read logs in CmdNormalizer.cs");

        logWriterNormalized = GameObject.Find("Shared Variables").GetComponent<SharedVariables>().logWriterNormalized;
        if (logWriterNormalized == null) Debug.LogError("Unable to write normalized logs in CmdNormalizer.cs");

        regex = new Regex(@"\w\s+");
	}
	
	// Update is called once per frame
	void Update () {
        string tmp = getLineToRead();
        if (tmp != null)
        {
            if(regex.IsMatch(tmp))
            {
                logWriterNormalized.WriteLine(tmp);
            }
            else
            {
                logWriterNormalized.WriteLine("");
            }
            lineNumber++;
        }
	}

    string getLineToRead()
    {
        for(int i=0 ; i<lineNumber ; i++)
        {
            logReader.ReadLine();
        }
        return logReader.ReadLine();
    }
}
