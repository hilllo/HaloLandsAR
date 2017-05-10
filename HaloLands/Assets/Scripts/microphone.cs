using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class microphone : MonoBehaviour
{

    // Use this for initialization
    //public AudioSource aud;
    public AudioSource mic;
    public int sampleTime = 30;
    public int sampleDataLength = 1024;
    public AudioSource deadSound;
    public float micThreshold = 0.3f;

    private int _sampleTimeCounter;
    private float _clipLoudness;
    private float[] _clipSampleData;
    void Start()
    {
        ///mic = GetComponent<AudioSource>();
        mic.clip = Microphone.Start("Built-in Microphone", true, 10, 44100);
        mic.Play();
        Debug.Log("start");
        _sampleTimeCounter = 0;
        _clipSampleData = new float[sampleDataLength];
    }

    // Update is called once per frame
    void Update()
    {
        ++_sampleTimeCounter;
        if (_sampleTimeCounter > sampleTime)
        {
            _sampleTimeCounter = 0;
            mic.clip.GetData(_clipSampleData, mic.timeSamples);
            _clipLoudness = 0f;
            foreach (var sample in _clipSampleData)
            {
                _clipLoudness += Mathf.Abs(sample);
            }
            _clipLoudness /= sampleDataLength;
            if (_clipLoudness > micThreshold)
            {
                Debug.Log("Shout at" + _clipLoudness);
                if(!deadSound.isPlaying) deadSound.Play();
            }
        }
    }
}
