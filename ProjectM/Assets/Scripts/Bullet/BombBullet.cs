using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BombBullet : MonoBehaviour
{
    private const float basicSizeBullet = 0.2f;
    private const float sizeMaxBullet = 1.3f;
    private const float sizeUpTickSpeed = 0.4f;
    private const float basicColliRadiusBullet = 0.5f;
    private const float sizeUpColliRadiusBullet = 15.0f;
    private const float colliderActRange = 0.75f;
    private const float colliderDisactRange = 0.75f;

    public float _bulletSpeed;

    private object[] _ownerInfo;
    private Vector2 _dir = new Vector2();
    private Vector2 _pos = new Vector2();
    private float _damageToPlayer = 0;
    private float _damageToObject = 0;
    private float _attackRange = 0;
    private float _fallOff = 0;
    private bool _isBomb = true;

    CircleCollider2D _circleCollider = null;

    private void Start()
    {
        _circleCollider = GetComponent<CircleCollider2D>();
    }

    private void Update()
    {

        transform.Translate(_dir * _bulletSpeed * Time.deltaTime, Space.World);

        if (_isBomb)
        {
            setBombColliderEnabled();
            if (Vector2.Distance(_dir + _pos, transform.position) > _attackRange * colliderActRange)
            {
                _circleCollider.radius = sizeUpColliRadiusBullet;
            }
            RefrestSizeBomb();
        }
        else
        {
            Debug.Log("Bomb bullet is not isBomb");
        }

        if (Vector2.Distance(_dir + _pos, transform.position) > _attackRange * 0.9f)
        {
            if (_isBomb)
            {
                ObjectPoolManager.AllocObject("Explosion", transform.position);
                _circleCollider.radius = basicColliRadiusBullet;
            }
            else
            {
                Debug.Log("Bomb bullet is not isBomb");
            }

            transform.localScale = new Vector3(basicSizeBullet, basicSizeBullet, transform.localScale.z);
            _circleCollider.enabled = false;
            gameObject.SetActive(false);
        }

        
    }

    private void OnDisable()
    {
        ObjectPoolManager.FreeObjectToPool(gameObject);
        CancelInvoke();
    }

    public void SetData(object[] ownerInfo, Vector2 dir, Vector2 pos, float damageToPlayer, float damageToObject, float attackRange, float fallOff, bool isBomb)
    {
        this._ownerInfo = ownerInfo;
        this._dir = dir;
        this._pos = pos;
        this._damageToPlayer = damageToPlayer;
        this._damageToObject = damageToObject;
        this._attackRange = attackRange;
        this._fallOff = fallOff;
        this._isBomb = isBomb;
    }

    public void RefrestSizeBomb()
    {
        if (Vector2.Distance(_dir + _pos, transform.position) < _attackRange / 2.0f)
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

    public void setBombColliderEnabled()
    {
        if (Vector2.Distance(_dir + _pos, transform.position) < _attackRange * colliderDisactRange)
        {
            _circleCollider.enabled = false;
        }
        else
        {
            _circleCollider.enabled = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            IDamageable target = null;

            if (collision.TryGetComponent(out target))
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    target.TakeDamage(_ownerInfo, _damageToPlayer, _damageToObject, _dir, true);
                }
                else
                {
                    target.TakeDamage(_ownerInfo, _damageToPlayer, _damageToObject, _dir, false);
                }
            }
            else
            {
                Debug.Log("There is no target in bullet");
            }

            if (_isBomb)
            {
                ObjectPoolManager.AllocObject("Explosion", transform.position);

            }
            else
            {
                Debug.Log("Bomb bullet is not isBomb");
            }
        }
        else
        {
            Debug.Log("collision null in bullet");
        }

        transform.localScale = new Vector3(basicSizeBullet, basicSizeBullet, transform.localScale.z);
        _circleCollider.radius = basicColliRadiusBullet;
        _circleCollider.enabled = false;
        gameObject.SetActive(false);
    }
}
