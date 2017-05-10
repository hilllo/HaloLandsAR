using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlastWave : MonoBehaviour {

    [SerializeField]
    ParticleSystem vfx, subVfx;

    static float SPEED = 5f;
    public static float speed
    {
        get { return SPEED; }
    }

    static float STRENGTH = 10f;

    Vector3 dir;
    bool isReady = false;
    float life = 2f;
	
	// Update is called once per frame
	void Update () {
        if (life > 0)
        {
            life -= Time.deltaTime;
            if (life <= 0)
            {
                ParticleSystem.EmissionModule emission = vfx.emission;
                ParticleSystem.EmissionModule subEmission = subVfx.emission;
                emission.enabled = false;
                subEmission.enabled = false;
                Destroy(this.gameObject, 5f);
            }
        }

		if (isReady)
        {
            transform.position += dir * SPEED * Time.deltaTime;
        }
	}

    public void Setup(int count)
    {
        // particle strength
        ParticleSystem.EmissionModule emission = vfx.emission;
        ParticleSystem.EmissionModule subEmission = subVfx.emission;
        if (count < 0)
            count = 0;
        emission.rateOverTime = 5f + count * STRENGTH;
        emission.enabled = true;
        subEmission.enabled = true;

        // position
        // TODO: put VFX on the ground
        Vector3 pos = Camera.main.transform.position;
        pos.y -= 1;
        transform.position = pos;

        // direction
        Vector3 camDir = Camera.main.transform.forward;
        Vector3 camProj = Vector3.Project(camDir, new Vector3(0, 1, 0));
        dir = (camDir - camProj).normalized;
        transform.LookAt(transform.position + dir);

        // finish setting
        isReady = true;
    }
}
