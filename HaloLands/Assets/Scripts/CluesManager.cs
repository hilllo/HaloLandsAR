using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;

public class CluesManager : Singleton<CluesManager> {

    private Clue[] clues;
    private int currentClue = 0;

    public void PlayNextClue()
    {
        if(clues.Length == 0)
        {
            return;
        }

        currentClue++;
        if (currentClue == clues.Length)
        {
            currentClue = 0;
        }

        clues[currentClue].PlayClue();
    }

	// Use this for initialization
	void Start () {
        clues = GameObject.FindObjectsOfType<Clue>();
        Invoke("PlayNextClue", 1);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    
}
