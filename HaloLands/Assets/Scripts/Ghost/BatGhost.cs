using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatGhost : EyeDetector, Ghost {

    static float DISTANCE = 1f;
    bool isKilled = false;
    
    void Start () {
        Appear();
	}

    void Update()
    {
        if (Vector3.Distance(Camera.main.transform.position, transform.position) < DISTANCE)
        {
            Kill();
        }
    }

    void Appear()
    {
        AudioManager.Instance.FadeTo(transform.GetComponent<AudioSource>(), 1f, 2f);
    }

    IEnumerator Disappear()
    {
        ParticleSystem.EmissionModule emission = transform.GetChild(0).GetComponent<ParticleSystem>().emission;
        emission.enabled = false;
        yield return new WaitForSeconds(4f); 
        Destroy(gameObject);
    }

    public bool Kill()
    {
        if (!isKilled)
        {
            isKilled = true;
            StartCoroutine(Disappear());
            AudioManager.Instance.FadeTo(transform.GetComponent<AudioSource>(), 0f, 2f);
            return true;
        }
        return false;
    }
}
