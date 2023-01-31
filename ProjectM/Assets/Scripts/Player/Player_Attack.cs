using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public partial class Player
{
    // Attack() ~
    private void Attack()
    {
        switch (_info.GetWeaponInSlot(_curSlot).itemData.itemType)
        {
            case ItemType.WEAPONGUN:
                {
                    if (CanAttackGun() == true)
                    {
                        AttackGun();
                    }
                    break;
                }
            case ItemType.WEAPONSUB:
                {
                    if (_canFire[_curSlot] == true)
                    {
                        AttackSub();
                    }
                    break;
                }
            case ItemType.WEAPONTHROW:
                {
                    if (CanAttackThrow() == true)
                    {
                        AttackThrow();
                    }
                    break;
                }
            default:
                {
                    Debug.Log("_info._currentWeapon.type is Null");
                    break;
                }
        }
    }

    private bool CanAttackGun()
    {
        if (_canFire[_curSlot] == true && _state != State.RELOADING)
        {
            if (_info.GetLoadedBullet(_curSlot) > 0)
            {
                return true;
            }
            else
            {
                Debug.Log("Don't have loadedbullet");
            }
        }
        else
        {
            //Debug.Log("Can't Attack");
        }
        return false;
    }

    private bool CanAttackThrow()
    {
        if (_canFire[ConstNums.weaponThrowIndex] == true && _state != State.ATTACKLATENCY)
        {
            if (_info.IsNullWeaponThrowSlot() == true)
            {
                return false;
            }
            return true;
        }
        else
        {
            //Debug.Log("Can't Attack");
        }
        return false;
    }

    private void AttackGun()
    {
        StopAction();

        _canFire[_curSlot] = false;
        _info.AddLoadedBulletValue(_curSlot, -1);
        OwnerInfo bulletOwner =
            new OwnerInfo(PlayerManager.Instance.GetName(), _info.GetWeaponInSlot(_curSlot).itemData.itemName, PlayerManager.Instance.GetID());
        
        // ~ Shot()에서 이동
        WeaponGun gun = _info.GetWeaponGunInSlot(_curSlot);
        if (gun.name == "firegun")
        {
            
            _photonView.RPC("FlameShot", RpcTarget.AllBufferedViaServer,
                bulletOwner.GetObjects(), CalcAttackDir(), transform.position, transform.rotation);
        }
        else if (gun.name == "rocketlauncher")
        {

            _photonView.RPC("Shot", RpcTarget.AllBufferedViaServer,
               bulletOwner.GetObjects(), CalcAttackDir(), transform.position, gun.weaponData.damageToPlayer, gun.weaponData.damageToObject,
               Vector3.Distance(_input.mouseVec, transform.position), gun.weaponData.fallOff, gun.weaponData.isBomb);
        }
        else
        {
            _photonView.RPC("Shot", RpcTarget.AllBufferedViaServer,
                bulletOwner.GetObjects(), CalcAttackDir(), transform.position, gun.weaponData.damageToPlayer, gun.weaponData.damageToObject,
                gun.weaponData.attackRange, gun.weaponData.fallOff, gun.weaponData.isBomb);
        }

        if (gun.name == "m4a1" || gun.name == "scar")
        {
            _audioSource.Stop();
            if (_audioSource.isPlaying == false)
            {
                _audioSource.clip = _audioGunAK;
                _audioSource.PlayOneShot(_audioGunAK);
            }
        }
        else if (gun.name == "firegun" || gun.name == "rocketlauncher")
        {
            _audioSource.Stop();
            if (_audioSource.isPlaying == false)
            {
                _audioSource.clip = _audioGunBomb;
                _audioSource.PlayOneShot(_audioGunBomb);
            }
        }
        else
        {
            _audioSource.Stop();
            if (_audioSource.isPlaying == false)
            {
                _audioSource.clip = _audioGun;
                _audioSource.PlayOneShot(_audioGun);
            }
        }
        

        HUDRefreshSlotBullet();
        StartCoroutine($"AttackLatency_{_curSlot}");

        //총기에 현재 장전된 총알이 0발이고, 그 형태의 총알이 있다면 자동으로 장전을 시작해준다.
        AutoReloading();
    }

    private void AttackSub()
    {
        StopAction();

        OwnerInfo attackOwner =
            new OwnerInfo(PlayerManager.Instance.GetName(), _info.GetWeaponInSlot(_curSlot).itemData.itemName, PlayerManager.Instance.GetID());
        

        _anim.SetTrigger("AttackSub");



        Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(_basicAtkBox.position, _basicAtkSize, 0);

        foreach (Collider2D collider in collider2Ds)
        {
            IDamageable target = null;
            if (collider.tag == "Dummy")
            {
                if (collider.gameObject.TryGetComponent(out target))
                {
                    target.TakeDamage(attackOwner.GetObjects(), _info.GetWeaponInSlot(_curSlot).weaponData.damageToPlayer, _info.GetWeaponInSlot(_curSlot).weaponData.damageToObject, CalcAttackDir(), true);
                }
            }
            if (collider.tag == "Player" || collider.tag == "AI")
            {
                if (collider.gameObject.TryGetComponent(out target))
                {
                    target.TakeDamage(attackOwner.GetObjects(), _info.GetWeaponInSlot(_curSlot).weaponData.damageToPlayer, _info.GetWeaponInSlot(_curSlot).weaponData.damageToObject, CalcAttackDir(), true);
                }
            }
        }
        _photonView.RPC("RPC_HandAttack", RpcTarget.Others, attackOwner.GetObjects(), _info.GetWeaponInSlot(_curSlot).weaponData.damageToPlayer, _info.GetWeaponInSlot(_curSlot).weaponData.damageToObject);

        StartCoroutine("AttackLatency_" + _curSlot);
    }

    private void AttackThrow()
    {
        StopAction();
        _state = State.ATTACKING;
        _canFire[ConstNums.weaponThrowIndex] = false;
        OwnerInfo throwOwner =
            new OwnerInfo(PlayerManager.Instance.GetName(), _info.GetWeaponInSlot(_curSlot).itemData.itemName, PlayerManager.Instance.GetID());
        WeaponThrow thr = _info.GetWeaponInSlot(_curSlot).GetComponent<WeaponThrow>();

        if (thr.weaponData.throwType == ThrowType.GRENADE)
        {
            _photonView.RPC("GreThrow", RpcTarget.AllBufferedViaServer,
               throwOwner.GetObjects(), CalcAttackDir(), transform.position, thr.weaponData.damageToPlayer, thr.weaponData.damageToObject, Vector3.Distance(_input.mouseVec, transform.position));
        }
        else if (thr.weaponData.throwType == ThrowType.FOG)
        {
            _photonView.RPC("FogThrow", RpcTarget.AllBufferedViaServer,
               throwOwner.GetObjects(), CalcAttackDir(), transform.position, thr.weaponData.damageToPlayer, thr.weaponData.damageToObject, Vector3.Distance(_input.mouseVec, transform.position));
        }

        _info.UseWeaponThrow(_curSlot);
        if (_info._slotWeapon[_curSlot].GetItemCount() == 0)
        {
            if (_info.IsNullOtherWeaponThrow(_curSlot) == true)
            {
                ChangeSlot(ConstNums.subWeaponIndex);
                _info.ClearCurWeaponThrowIndex();
            }
            else
            {
                SwapOtherThrowWeapon();
            }
        }
        else
        {
            _info.HUD.RefreshSlotWeaponThrow(_info.GetWeaponInSlot(_curSlot).GetComponent<WeaponThrow>());
        }
        
        StartCoroutine($"AttackLatency_{ConstNums.weaponThrowIndex}");
    }

    private void SwapOtherThrowWeapon()
    {
        _curSlot = _info.ChangeThrowIndex(_curSlot);
        _info.HUD.ChangeCurSlot(ConstNums.weaponThrowIndex);
        _info.HUD.RefreshSlotWeaponThrow(_info.GetWeaponInSlot(_curSlot).GetComponent<WeaponThrow>());
    }

    private void ThrowWeaponCooltime()
    {
        _canFire[_curSlot] = true;
    }

    private IEnumerator AttackLatency_0()
    {
        _state = State.ATTACKLATENCY;
        _info.HUD.PlayWeaponCooltimeView(_curSlot, _info.GetWeaponInSlot(_curSlot).weaponData.atkLatency);
        yield return new WaitForSeconds(_info.GetWeaponInSlot(_curSlot).weaponData.atkLatency);
        _canFire[ConstNums.primaryWeapon1Index] = true;
        _state = State.LIVE;
    }

    private IEnumerator AttackLatency_1()
    {
        _state = State.ATTACKLATENCY;
        _info.HUD.PlayWeaponCooltimeView(_curSlot, _info.GetWeaponInSlot(_curSlot).weaponData.atkLatency);
        yield return new WaitForSeconds(_info.GetWeaponInSlot(_curSlot).weaponData.atkLatency);
        _state = State.LIVE;
        _canFire[ConstNums.primaryWeapon2Index] = true;
    }

    private IEnumerator AttackLatency_2()
    {
        yield return new WaitForSeconds(_info.GetWeaponInSlot(_curSlot).weaponData.atkLatency);
        _canFire[ConstNums.subWeaponIndex] = true;
    }

    private IEnumerator AttackLatency_3()
    {
        _state = State.ATTACKLATENCY;
        yield return new WaitForSeconds(throwWeaponL);
        _canFire[ConstNums.weaponThrowIndex] = true;
        _state = State.LIVE;
    }


    public Vector3 CalcAttackDir()
    {
        Vector2 dir = _input.mouseVec - transform.position;
        dir.Normalize();
        return (Vector3)dir;
    }

    public void TakeDamage(object[] ownerInfo, float damageToPlayer, float damageToObject, Vector2 startVector, bool isAttacker)
    {
        if(IsCurrentHPOverZero() == true)
        {
            if ((bool)ownerInfo[(int)InfoIdx.ISMAGNET] == false) // 자기장이 아니면
            {
                ObjectPoolManager.AllocObject("Hit", transform.position + new Vector3((-1 * startVector).x, (-1 * startVector).y, 7));
                if (_audioSrcHit != null)
                {
                    _audioSource.PlayOneShot(_audioSrcHit);
                }
            }
        }

        if (_photonView.IsMine == false)
        {
            return;
        }

        if (IsCurrentHPOverZero() == true)
        {
            //임시구현. Vector2 startVector와 transform.Translate 값을 이용하여 d값을 구하고 그 값에 따라 다른 데미지 부여 필요
            float calcDamage = damageToPlayer * (1 - (_info.GetDefensive() / (defenseConstant + _info.GetDefensive())));
            _info.AddCurrentHPValue(-calcDamage);

            if ((bool)ownerInfo[(int)InfoIdx.ISMAGNET] == false)
            {
                _rbody.velocity = Vector2.zero;
                StartCoroutine(Knockback(startVector, transform.position));
            }
        }

        if ((IsCurrentHPOverZero() == false) && (PlayerManager.Instance.IsDead() == false))
        {
            DropItemManager.DropItemsOfDeadPlayer(_info);

            PlayerManager.Instance.NotifyDeath(ownerInfo);
            PlayerManager.Instance.SetDead(true);
            
            _info.HUD.ChangeActiveCursor(false);

            _photonView.RPC("RPC_Dead", RpcTarget.AllBuffered);
        }
    }

    public bool IsCurrentHPOverZero()
    {
        if(_info.GetCurrentHP() > 0)
        {
            return true;
        }
        return false;
    }

    [PunRPC]
    public void FlameShot(object[] ownerInfo, Vector3 direction, Vector3 position, Quaternion angle)
    {
        _particleSystem.Play();
        _anim.SetTrigger("shoot");
        Vector3 pos = new Vector3(position.x, position.y, 0f);

        var flame = ObjectPoolManager.AllocObject<Flame>("Flame", pos);
        flame.transform.position = position;
        flame.transform.rotation = angle;
        flame.SetOwnerInfo(ownerInfo, direction);
    }

    [PunRPC]
    public void Shot(object[] ownerInfo, Vector3 direction, Vector3 position, float damageToPlayer, float damageToObject, float attackRange, float fallOff, bool isBomb)
    {
        _particleSystem.Play();
        _anim.SetTrigger("shoot");
        Vector3 pos = new Vector3(position.x, position.y, 0f);
        if (!isBomb)
        {
            var bullet = ObjectPoolManager.AllocObject<Bullet>("Bullet", pos + direction * 2);
            bullet.transform.position = position + direction * 2;
            bullet.SetData(ownerInfo, direction, position, damageToPlayer, damageToObject, attackRange, fallOff, isBomb);
        }
        else
        {
            var bullet = ObjectPoolManager.AllocObject<BombBullet>("BombBullet", pos + direction * 2);
            bullet.transform.position = position + direction * 2;
            bullet.SetData(ownerInfo, direction, position, damageToPlayer, damageToObject, attackRange, fallOff, isBomb);
        }
    }

    [PunRPC]
    public void RPC_HandAttack(object[] ownerInfo, float damageToPlayer, float damageToObject)
    {
        _anim.SetTrigger("AttackSub");
        Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(_basicAtkBox.position, _basicAtkSize, 0);

        foreach (Collider2D collider in collider2Ds)
        {
            IDamageable target = null;
            if (collider.tag == "Dummy")
            {
                if (collider.gameObject.TryGetComponent(out target))
                {
                    target.TakeDamage(ownerInfo, damageToPlayer, damageToObject, (Vector2)new Vector2(0, 0), false);
                }
                else
                {
                    Debug.Log("\"Dummy\" don't have TakeDamage()");
                }
            }
            if (collider.tag == "Player")
            {
                if (collider.gameObject.TryGetComponent(out target))
                {
                    target.TakeDamage(ownerInfo, damageToPlayer, damageToObject, (Vector2)new Vector2(0, 0), false);
                }
                else
                {
                    Debug.Log("\"Player\" don't have TakeDamage()");
                }
            }
            else
            {
                //Debug.Log("Not Player or Dummy");
            }
        }
    }

    [PunRPC]
    public void GreThrow(object[] ownerInfo, Vector3 direction, Vector3 position, float damageToPlayer, float damageToObject, float lotation)
    {
        _anim.SetTrigger("shoot");
        Vector3 pos = new Vector3(position.x, position.y, 0f);
        var gre = ObjectPoolManager.AllocObject<Grenade>("Grenade", transform.position + CalcAttackDir());
        gre.transform.position = position + direction;
        gre.SetData(ownerInfo, direction, position, damageToPlayer, damageToObject,
               lotation);
    }

    [PunRPC]
    public void FogThrow(object[] ownerInfo, Vector3 direction, Vector3 position, float damageToPlayer, float damageToObject, float lotation)
    {
        _anim.SetTrigger("shoot");
        Vector3 pos = new Vector3(position.x, position.y, 0f);
        var fog = ObjectPoolManager.AllocObject<BombFog>("Bombfog", transform.position + CalcAttackDir());
        fog.transform.position = position + direction;
        fog.SetData(ownerInfo, direction, position, damageToPlayer, damageToObject,
                       lotation);
    }
    // ~ Attack()
}