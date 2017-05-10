using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clue : MonoBehaviour
{

    static float CLUE_IS_HEARD_DISTANCE = 0.8f;

    public AudioClip[] playList;
    private int currentClipIndex = 0;

    [SerializeField]
    private AudioSource audioSource;

    private bool clueStarted = false;
    private bool playerIsClose = false;

    [SerializeField]
    private ParticleSystem clueParticle;

    public void PlayClue()
    {

        PlayNextClip();
        clueStarted = true;
        playerIsClose = false;
        clueParticle.Play();
    }

    // Use this for initialization
    void Start()
    {
        audioSource = gameObject.GetComponentInChildren<AudioSource>();
        clueParticle = gameObject.GetComponentInChildren<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (clueStarted)
        {
            //Clip finished
            if (!audioSource.isPlaying)
            {
                if (playerIsClose)
                {
                    //Next clue
                    CluesManager.Instance.PlayNextClue();
                    clueParticle.Stop();
                    clueStarted = false;
                }
                else
                {
                    //Loop playlist
                    PlayNextClip();
                }
            }

            //Player heard this clue close enough
            Vector3 playerPos = Camera.main.transform.position;
            if (Vector3.Distance(playerPos, transform.position) < CLUE_IS_HEARD_DISTANCE)
            {
                playerIsClose = true;
            }
        }
    }

    private void PlayNextClip()
    {
        currentClipIndex++;
        if (currentClipIndex >= playList.Length)
        {
            currentClipIndex = 0;
        }
        audioSource.clip = playList[currentClipIndex];
        audioSource.Play();
    }
}
