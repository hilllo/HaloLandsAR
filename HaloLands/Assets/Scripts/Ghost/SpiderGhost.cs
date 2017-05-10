using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderGhost : EyeDetector, Ghost {

    // Use this for initialization
    private enum STATE {standby,dropping,dropFinish,attack};
    private STATE selfState = STATE.standby;
    private AudioSource audioSource;
    public AudioClip[] DropDownSound;
    public AudioClip WebShotSound;
    public AudioClip StandBySound;
    private float SPEED = 1f;
    float attackRange = 3f;
    public GameObject web, spider;
    Rigidbody rg;
    GameObject eyeblock;
    void Start () {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = StandBySound;
        audioSource.loop = true;
        audioSource.Play();
        eyeblock = Camera.main.transform.GetChild(1).gameObject;
        rg = GetComponent<Rigidbody>();
    }

    bool isActivate = false;

    public void Activate()
    {
        isActivate = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActivate)
            return;
        Vector3 spiderPos = transform.position;
        Vector3 playerPos = Camera.main.transform.position;
        spiderPos.y = 0;
        playerPos.y = 0;
        float distance = Vector3.Distance(spiderPos, playerPos);
        if ((distance < attackRange) && selfState == STATE.standby)
        {
            DropDown();
        }
        
        if(selfState==STATE.dropping)
        {
            if (Math.Abs(transform.position.y - Camera.main.transform.position.y) > 0.1)
                transform.position = new Vector3(transform.position.x, Mathf.Lerp(transform.position.y, Camera.main.transform.position.y, 0.9f * Time.deltaTime), transform.position.z);
            else
            {
                selfState = STATE.dropFinish;
            }
                
        }
        if(selfState!=STATE.attack) transform.LookAt(new Vector3(Camera.main.transform.position.x,transform.position.y ,Camera.main.transform.position.z));
    }
    
    void DropDown()
    {
        audioSource.Stop();
        audioSource.clip = DropDownSound[UnityEngine.Random.Range(0,DropDownSound.Length)];
        audioSource.loop = false;
        if(!audioSource.isPlaying) audioSource.Play();
        //Debug.Log("drp[");
        selfState = STATE.dropping;
    }
    void Attack()
    {
        ///Animation?
        audioSource.Stop();
        audioSource.clip = WebShotSound;
        audioSource.loop = false;
        audioSource.volume = 1;
        if (!audioSource.isPlaying) audioSource.Play();
        //Debug.Log("shot");
        

        web.SetActive(false);
        rg.useGravity = true;
        spider.transform.rotation = Quaternion.Euler(0, 180, 0);
        Invoke("Run", 1.5f);

        EyeTrigger.SetBlocked(true);
        eyeblock.SetActive(true);
        //GameObject.FindGameObjectWithTag("Web").SetActive(true);
        
    }
    void Run()
    {
        audioSource.Stop();
        audioSource.clip = DropDownSound[UnityEngine.Random.Range(0, DropDownSound.Length)];
        audioSource.loop = false;
        audioSource.volume = 0.5f;
        if (!audioSource.isPlaying) audioSource.Play();

        gameObject.GetComponent<Rigidbody>().velocity += Vector3.back * SPEED*5;
        Invoke("UnBlockedView", 1.5f);
    }
    void UnBlockedView()
    {
        ///EyeTrigger.SetBlocked(false);
        EyeTrigger.SetBlocked(false);
        eyeblock.SetActive(false);
        //GameObject.FindGameObjectWithTag("Web").SetActive(false);
        KillProcess();
    }
    void KillProcess()
    {

    }
    public bool Kill()
    {
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
        return true;
    }
    
    protected override void OnStared()
    {
        base.OnStared();
        if ((selfState==STATE.dropFinish) && (Vector3.Distance(transform.position, Camera.main.transform.position) < 2f))
        {
            Attack();
            selfState = STATE.attack;
        }
    }
}
