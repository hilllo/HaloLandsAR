using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioController : EyeDetector
{
    public GameObject front, back;
	// Use this for initialization
	void Start () {
        TurnOn();
    }
	public void TurnOn()
    {
        gameObject.GetComponent<AudioSource>().Play();
    }

    IEnumerator TurnOffCoroutine()
    {
        AudioDistortionFilter filter = gameObject.GetComponent<AudioDistortionFilter>();

        for(int i=0; i<3; i++)
        {
            //Repeat 3 times
            //up
            filter.distortionLevel = Random.Range(0.5f, 0.9f);
            yield return new WaitForSeconds(Random.Range(0.2f, 5f));
            //down
            filter.distortionLevel = 0.5f;
            yield return new WaitForSeconds(Random.Range(0.3f, 1f));
        }

        filter.distortionLevel = 0.5f;
        yield return new WaitForSeconds(1f);

        AudioSource source = gameObject.GetComponent<AudioSource>();
        while(source.volume > 0.01f)
        {
            source.volume -= Time.deltaTime / 20f;
            yield return null;
        }

        while (stareState == StareState.STARED)
        {
            yield return null;
        }

        

        gameObject.GetComponent<AudioSource>().Stop();
        front.transform.localPosition = new Vector3(-0.045f, 0.031f, 0.292f);
        front.transform.localRotation = Quaternion.Euler(-9.8f, -40, 0);
        back.transform.localPosition = new Vector3(-0f, 0.123f, 0.068f);
        back.transform.localRotation = Quaternion.Euler(-23, 0, 0);
        yield return null;
    }
    public void TurnOff()
    {
        StartCoroutine(TurnOffCoroutine());
    }



	// Update is called once per frame
	void Update () {
		
	}
}
