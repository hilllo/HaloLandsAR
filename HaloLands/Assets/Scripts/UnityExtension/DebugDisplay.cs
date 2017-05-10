//#define USE_DEBUG

using UnityEngine;
using System.Collections;
using HoloToolkit.Unity;
using System;
using UnityEngine.VR.WSA.Input;

public class DebugDisplay : Singleton<DebugDisplay>
{
    private String showingString = "";
    private TextMesh DebugDisplayMesh;

    public bool showInGameLog = true;
    private void Start()
    {
        if (showInGameLog)
        {
            DebugDisplayMesh = transform.GetComponent<TextMesh>();
        }
    }

    public static void Log(object message)
    {
#if USE_DEBUG
        DebugDisplay.Instance.showingString += (message.ToString() + "\n");
        Debug.Log(message);

        // Basic checks
        if (DebugDisplay.Instance.DebugDisplayMesh == null)
        {
            return;
        }
        else
        {
            DebugDisplay.Instance.DebugDisplayMesh.text = DebugDisplay.Instance.showingString;
        }
#endif
    }
}
