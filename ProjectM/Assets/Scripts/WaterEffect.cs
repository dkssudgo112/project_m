using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterEffect : MonoBehaviour
{
    
    private const float DestroyTime = 1.0f;
    private const float OriginalSize = 1.0f;
    private const float TickDescreseColer = 0.1f;

    public float destroyTickTime = 0.0f;
    private SpriteRenderer _spriteRenderer = null;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        
    }
    void Update()
    {
        destroyTickTime += Time.deltaTime;
        transform.localScale += new Vector3(Time.deltaTime*2, Time.deltaTime*2,0);
        _spriteRenderer.color -= new Color(0, 0, 0, TickDescreseColer);

        if (destroyTickTime > DestroyTime)
        {
            transform.localScale = new Vector3(OriginalSize, OriginalSize, OriginalSize);
            _spriteRenderer.color = Color.white;
            gameObject.SetActive(false);
            destroyTickTime = 0;
        }
    }

    private void OnDisable()
    {      
        ObjectPoolManager.FreeObjectToPool(gameObject);
    }
}
