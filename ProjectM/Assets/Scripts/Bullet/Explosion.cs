using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float destroyTickTime = 0.0f;
    private const float destroyTime = 2.0f;

    private AudioSource _audioSource = null;
    private bool _soundOn = false;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>(); 
    }



    void Update()
    {
        if(_soundOn == false)
        {
            if (_audioSource != null)
            {
                _audioSource.Play();
            }
            _soundOn = true;    
        }
        destroyTickTime += Time.deltaTime;
        if(destroyTickTime > destroyTime)
        {
            _soundOn=false;
            gameObject.SetActive(false);

            destroyTickTime = 0;
        }
    }

    private void OnDisable()
    {
        
        ObjectPoolManager.FreeObjectToPool(gameObject);
    }
}
