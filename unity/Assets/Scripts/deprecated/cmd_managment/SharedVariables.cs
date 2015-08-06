using UnityEngine;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

public class SharedVariables : MonoBehaviour {

    public FileStream log = null;
    public FileStream logNormalized = null;

    public StreamReader logReader = null;
    public StreamReader logReaderNormalized = null;
    
    public StreamWriter logWriter = null;
    public StreamWriter logWriterNormalized = null;

    public string logPath = @"C:\Users\alleonar\Desktop\nouveau_dossier\";
    public string logName = "log.txt";
    public string logNameNormalized = "log_normalized.txt";



	void Start () 
    {
        try
        {
            log = new FileStream(logPath + logName, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
        }
        catch(System.Exception e)
        {
            Debug.LogError(e.ToString() + " when trying to open log file :" + logPath + logName);
        }
        try
        {
            logNormalized = new FileStream(logPath + logNameNormalized, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
        }
        catch(System.Exception e)
        {
            Debug.LogError(e.ToString() + " when trying to open normalized log file :" + logPath + logNameNormalized);
        }

        logReader = new StreamReader(log);
        logReaderNormalized = new StreamReader(logNormalized);

        logWriter = new StreamWriter(log);
        logWriterNormalized = new StreamWriter(logNormalized);
	}

    void OnDisable()
    {
        log.Close();
        logNormalized.Close();

        logReader.Close();
        logReaderNormalized.Close();

        logWriter.Close();
        logWriterNormalized.Close();
    }
}
