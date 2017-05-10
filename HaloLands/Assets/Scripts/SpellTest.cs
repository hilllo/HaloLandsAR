using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Get(string input)
    {
        Camera.main.transform.GetComponentInChildren<TextMesh>().text = input;
    }
}
