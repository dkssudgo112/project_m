using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Flame : MonoBehaviour
{
    public float destroyTickTime = 0.0f;
    private const float destroyTime = 0.3f;
    public Vector2 _dir = Vector2.zero;

    private ParticleSystem _flame = null;

    private object[] _ownerInfo;
    private bool _updateData = false;
    private float _damageToPlayer = 0;
    private float _damageToObject = 0;

    private void Awake()
    {
        _flame = GetComponent<ParticleSystem>();
    }


    void Update()
    {
        if(_updateData == false)
        {
            _damageToPlayer = ItemManager.GetItemByName("firegun").GetComponent<Weapon>().weaponData.damageToPlayer;
            _damageToObject = ItemManager.GetItemByName("firegun").GetComponent<Weapon>().weaponData.damageToObject;
            _updateData = true;
        }

        destroyTickTime += Time.deltaTime;
        if (destroyTickTime > destroyTime)
        {
            gameObject.SetActive(false);
            destroyTickTime = 0;
        }
    }

    private void OnEnable()
    {
        _flame.Play();

        
    }

    private void OnDisable()
    {
        _flame.Stop();
        ObjectPoolManager.FreeObjectToPool(gameObject);
    }

    private void OnParticleCollision(GameObject other)
    {
        IDamageable target = null;

        if (other.tag == "Dummy" || other.tag == "Player" || other.tag == "AI")
        {
            if (other.gameObject.TryGetComponent(out target))
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
                Debug.Log("no target flame gun");
            }
        }
    }
    public void SetOwnerInfo(object[] ownerInfo, Vector2 direction)
    {
        this._ownerInfo = ownerInfo;
        this._dir = direction;
    }
}
