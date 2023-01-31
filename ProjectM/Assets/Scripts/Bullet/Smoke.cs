using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smoke : MonoBehaviour
{
    public float destroyTickTime = 0.0f;
    private const float destroyTime = 5.5f;


    void Update()
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
