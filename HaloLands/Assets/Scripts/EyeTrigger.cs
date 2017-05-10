using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeTrigger : MonoBehaviour {
    public delegate void OnViewEnterEvent();
    public delegate void OnViewExitEvent();
    public delegate void OnStaredEvent();
    public delegate void OnNotStaredEvent();
    public event OnViewEnterEvent onViewEnter;
    public event OnViewExitEvent onViewExit;
    public event OnStaredEvent onStared;
    public event OnNotStaredEvent onNotStared;

    public static bool isKeyboardDebug = false;

    private Collider collider;
    private Camera cam;
    private Plane[] planes;
    private bool isStared = false;

    private static bool IS_BLOCKED = false;
    public static void SetBlocked(bool opt)
    {
        IS_BLOCKED = opt;
    }

    void Start()
    {
        cam = Camera.main;
        collider = GetComponent<Collider>();
    }

	// Update is called once per frame
	void Update () {
        if (Detect() && !IS_BLOCKED)
        {
            if (!isStared)
            {
                isStared = true;
                if (onViewEnter != null)
                    onViewEnter();
            }
            if (onStared != null)
                onStared();
        }
        else
        {
            if (isStared)
            {
                isStared = false;
                if (onViewExit != null)
                    onViewExit();
            }
            if (onNotStared != null)
                onNotStared();
        }
    }

    bool Detect()
    {
        if (cam == null || collider == null)
        {
            return false;
        }
        planes = GeometryUtility.CalculateFrustumPlanes(cam);
        return GeometryUtility.TestPlanesAABB(planes, collider.bounds);
    }
}
