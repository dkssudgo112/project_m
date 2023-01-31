using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombFog : MonoBehaviour
{
    private const float basicSizeBullet = 0.2f;
    private const float sizeMaxBullet = 1.3f;
    private const float sizeUpTickSpeed = 0.4f;

    public float _bulletSpeed;

    private Vector2 _dir = new Vector2();
    private Vector2 _pos = new Vector2();
    private float _attackRange = 0;

    private void Update()
    {
        
        RefrestSizeBomb();
        
        if (Vector2.Distance(_pos, transform.position) > _attackRange)
        {
            ObjectPoolManager.AllocObject("Smoke", transform.position);

            transform.localScale = new Vector3(basicSizeBullet, basicSizeBullet, transform.localScale.z);
            gameObject.SetActive(false);
        }

        transform.Translate(_dir * _bulletSpeed * Time.deltaTime, Space.World);
    }

    private void OnDisable()
    {
        ObjectPoolManager.FreeObjectToPool(gameObject);
        CancelInvoke();
    }

    public void SetData(object[] ownerInfo, Vector2 dir, Vector2 pos, float damageToPlayer, float damageToObject, float attackRange)
    {
        this._dir = dir;
        if (dir.magnitude == 0)
        {
            this._dir = new Vector3(1, 1, 0);
        }
        this._pos = pos;
        this._attackRange = attackRange;
    }

    public void RefrestSizeBomb()
    {
        if (Vector2.Distance(_pos, transform.position) < _attackRange / 2.0f)
        {
            if (transform.localScale.x < sizeMaxBullet)
            {
                transform.localScale += new Vector3(Time.deltaTime * sizeUpTickSpeed, Time.deltaTime * sizeUpTickSpeed, 0);
            }
            else
            {
                Debug.Log("Too mush size up");
            }
        }
        else
        {
            if (transform.localScale.x > basicSizeBullet)
            {
                transform.localScale -= new Vector3(Time.deltaTime * sizeUpTickSpeed, Time.deltaTime * sizeUpTickSpeed, 0);
            }
            else
            {
                transform.localScale = new Vector3(basicSizeBullet, basicSizeBullet, transform.localScale.z);

            }
        }
    }
}
