using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class itemChangeScript : EyeDetector
{
    public GameObject itemBeforeGazed;
    public GameObject itemAfterGazed;
    public AudioSource itemChangeSound;
    public float delayTime=2;
    private bool _changed=false;
    // Use this for initialization
    void Start()
    {
        itemBeforeGazed.SetActive(true);
        itemAfterGazed.SetActive(false);
        
        itemChangeSound = gameObject.GetComponent<AudioSource>();
        //Debug.Log("start");
    }
    protected override void OnViewEnter()
    {
        //Debug.Log("enter");
        CancelInvoke();
    }

    protected override void OnViewExit()
    {
        //Debug.Log("exit");
        if (_changed == false)
        {
            //_changed = true;
            Invoke("chandeItem", delayTime);
        }
        
    }
    void chandeItem()
    {
        Debug.Log("change");
        _changed = true;
        if (itemChangeSound.clip != null)
        {
            itemChangeSound.Play();
           
        }
        itemBeforeGazed.SetActive(false);
        itemAfterGazed.SetActive(true);
    }
    //protected virtual void OnViewEnter() { }
    //protected virtual void OnStared() { }
    //protected virtual void OnNotStared() { }
    //protected virtual void OnViewExit() { }
}
