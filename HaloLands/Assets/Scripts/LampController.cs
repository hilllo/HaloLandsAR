using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LampController : MonoBehaviour {

    // Use this for initialization

    public GameObject fire=null;
    Shader shaderOn;
    Shader shaderOff;
    void Start () {
       
        shaderOn = Shader.Find("Retro Lamps/RimEmission");
        shaderOff = Shader.Find("Retro Lamps/Rim");
        
        //TurnOffLight();
        

    }
	public void TurnOnLight()
    {
        if (fire==null)
        {
            GetComponent<Renderer>().material.shader = shaderOn;
        }
        else
        {
            fire.SetActive(true);
        }
        
    }
    public void TurnOffLight()
    {
        
        if (fire==null)
        {
            
            GetComponent<Renderer>().material.shader = shaderOff;
        }
        else
        {
          
            fire.SetActive(false);
        }
    }
	// Update is called once per frame
	void Update () {
       
    }
}
