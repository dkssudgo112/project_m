using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smoke : MonoBehaviour
{
    
    private const float destroyTime = 20.5f;

    public ParticleSystem[] setSeedParticles;
    public int ParticleSeed;
    public bool forceNewSeed;

    private float destroyTickTime = 0.0f;

    private void Start()
    {
        
        if (setSeedParticles.Length > 0)
        {
            for (int i = 0; i < setSeedParticles.Length; i++)
            {
                setSeedParticles[i].Stop();
                setSeedParticles[i].useAutoRandomSeed = false;
                setSeedParticles[i].randomSeed = (uint)ParticleSeed;
                setSeedParticles[i].Play();
            }
        }
    }

    private void OnEnable()
    {
        if (setSeedParticles.Length > 0)
        {
            for (int i = 0; i < setSeedParticles.Length; i++)
            {
                if (setSeedParticles[i] == null)
                {
                    return;
                }
                setSeedParticles[i].Stop();
                setSeedParticles[i].useAutoRandomSeed = false;
                setSeedParticles[i].randomSeed = (uint)ParticleSeed;
                setSeedParticles[i].Play();
            }
        }
    }

    private void Update()
    {

        destroyTickTime += Time.deltaTime;
        if (destroyTickTime > destroyTime)
        {
            gameObject.SetActive(false);
            destroyTickTime = 0;
        }
    }

    private void OnDisable()
    {
        ObjectPoolManager.FreeObjectToPool(gameObject);
    }
}
