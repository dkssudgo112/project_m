using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public partial class Player
{
    // Attack() ~
    private void Attack()
    {
        switch (_info.GetWeaponInSlot(_curSlot).type)
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
                    Debug.LogWarning($"ThrowWeapon : ");
                    if (CanAttackThrow() == true)
                    {
                        AttackThrow();
                    }
                    Debug.Log("User Want to Attack By WeaponThrow");
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
            if (_info._loadedBullet[_curSlot] > 0)
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
            Debug.Log("Can't Attack");
        }
        return false;
    }

    private bool CanAttackThrow()
    {
        if (_canFire[ConstNums.throwWeaponIndex] == true && _state != State.ATTACKLATENCY)
        {
            if (_info.IsNullThrowWeaponSlot() == true)
            {
                return false;
            }
            return true;
        }
        else
        {
            Debug.Log("Can't Attack");
        }
        return false;
    }

    private void AttackGun()
    {
        StopAction();

        _canFire[_curSlot] = false;
        _info._loadedBullet[_curSlot]--;
        OwnerInfo bulletOwner = new OwnerInfo(PlayerManager.GetName(), _info.GetWeaponInSlot(_curSlot).itemName);

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
               bulletOwner.GetObjects(), CalcAttackDir(), transform.position, gun.damageToPlayer, gun.damageToObject,
               Vector3.Distance(_input.mouseVec, transform.position), gun.fallOff, gun.isBomb);
        }
        else
        {
            _photonView.RPC("Shot", RpcTarget.AllBufferedViaServer,
                bulletOwner.GetObjects(), CalcAttackDir(), transform.position, gun.damageToPlayer, gun.damageToObject,
                gun.attackRange, gun.fallOff, gun.isBomb);
        }

        if (gun.name != "firegun" && gun.name != "rocketlauncher")
        {
            _audioSource.Stop();
            if (_audioSource.isPlaying == false)
            {
                _audioSource.clip = _audioGun;
                _audioSource.PlayOneShot(_audioGun);
            }
        }

        HUDUpdate_slotBullet();
        StartCoroutine($"AttackLatency_{_curSlot}");

        //총기에 현재 장전된 총알이 0발이고, 그 형태의 총알이 있다면 자동으로 장전을 시작해준다.
        AutoReloading();
    }

    private void AttackSub()
    {
        StopAction();

        OwnerInfo attackOwner = new OwnerInfo(PlayerManager.GetName(), _info.GetWeaponInSlot(_curSlot).itemName);
        

        _anim.SetTrigger("AttackSub");



        Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(_basicAtkBox.position, _basicAtkSize, 0);

        foreach (Collider2D collider in collider2Ds)
        {
            IDamageable target = null;
            if (collider.tag == "Dummy")
            {
                if (collider.gameObject.TryGetComponent(out target))
                {
                    //target.TakeDamage(attackOwner.GetObjects(), _info._fist.damageToPlayer, _info._fist.damageToObject, CalcAttackDir(), true);
                    target.TakeDamage(attackOwner.GetObjects(), _info.GetWeaponInSlot(_curSlot).damageToPlayer, _info.GetWeaponInSlot(_curSlot).damageToObject, CalcAttackDir(), true);
                }
            }
            if (collider.tag == "Player" || collider.tag == "AI")
            {
                if (collider.gameObject.TryGetComponent(out target))
                {
                    //target.TakeDamage(attackOwner.GetObjects(), _info._fist.damageToPlayer, _info._fist.damageToObject, (Vector2)new Vector2(0, 0), true);
                    target.TakeDamage(attackOwner.GetObjects(), _info.GetWeaponInSlot(_curSlot).damageToPlayer, _info.GetWeaponInSlot(_curSlot).damageToObject, CalcAttackDir(), true);
                }
            }
        }
        _photonView.RPC("RPC_HandAttack", RpcTarget.Others, attackOwner.GetObjects(), _info.GetWeaponInSlot(_curSlot).damageToPlayer, _info.GetWeaponInSlot(_curSlot).damageToObject);

        StartCoroutine("AttackLatency_" + _curSlot);
    }

    private void AttackThrow()
    {
        Debug.LogWarning($"ThrowWeapon : AttackThrow");
        StopAction();
        _state = State.ATTACKING;
        _canFire[ConstNums.throwWeaponIndex] = false;
        OwnerInfo throwOwner = new OwnerInfo(PlayerManager.GetName(), _info.GetWeaponInSlot(_curSlot).itemName);
        WeaponThrow thr = _info.GetWeaponInSlot(_curSlot).GetComponent<WeaponThrow>();

        if (thr.throwType == ThrowType.GRENADE)
        {
            var gre = ObjectPoolManager.AllocObject<BombBullet>("Grenade", transform.position + CalcAttackDir());
            gre.transform.position = transform.position + CalcAttackDir();
            gre.SetData(throwOwner.GetObjects(), CalcAttackDir(), transform.position, thr.damageToPlayer, thr.damageToObject,
                   Vector3.Distance(_input.mouseVec, transform.position), thr.fallOff, true);
        }
        else if (thr.throwType == ThrowType.FOG)
        {
            var fog = ObjectPoolManager.AllocObject<BombFog>("Bombfog", transform.position + CalcAttackDir());
            fog.transform.position = transform.position + CalcAttackDir();
            fog.SetData(throwOwner.GetObjects(), CalcAttackDir(), transform.position, thr.damageToPlayer, thr.damageToObject,
                   Vector3.Distance(_input.mouseVec, transform.position), thr.fallOff, true);
        }

        _info.UseThrowWeapon(_curSlot);
        if (_info.IsNullWeaponSlot(_curSlot) == true)
        {
            TrySwapOtherThrowWeapon();
        }
        else
        {
            _info.HUD.RefreshSlotThrowWeapon(_info._slotWeapon[_curSlot].GetWeapon());
        }

        StartCoroutine($"AttackLatency_{ConstNums.throwWeaponIndex}");
    }

    private void TrySwapOtherThrowWeapon()
    {
        _curSlot = _info.ChangeThrowIndex(_curSlot);
        //_info.HUD.RefreshSlotThrowWeapon(_info._slotWeapon[_curSlot].GetWeapon());

        if (_info.IsNullWeaponSlot(_curSlot) == true)
        {
            ChangeSlot(ConstNums.subWeaponIndex);
        }
        else
        {
            _info.HUD.ChangeCurSlot(ConstNums.throwWeaponIndex);
        }
    }

    private void ThrowWeaponCooltime()
    {
        _canFire[_curSlot] = true;
    }

    private IEnumerator AttackLatency_0()
    {
        _state = State.ATTACKLATENCY;
        yield return new WaitForSeconds(_info.GetWeaponInSlot(_curSlot).attackLatency);
        _canFire[ConstNums.primaryWeapon1Index] = true;
        _state = State.LIVE;
    }

    private IEnumerator AttackLatency_1()
    {
        _state = State.ATTACKLATENCY;
        yield return new WaitForSeconds(_info.GetWeaponInSlot(_curSlot).attackLatency);
        _state = State.LIVE;
        _canFire[ConstNums.primaryWeapon2Index] = true;
    }

    private IEnumerator AttackLatency_2()
    {
        yield return new WaitForSeconds(_info.GetWeaponInSlot(_curSlot).attackLatency);
        _canFire[ConstNums.subWeaponIndex] = true;
    }

    private IEnumerator AttackLatency_3()
    {
        _state = State.ATTACKLATENCY;

        yield return new WaitForSeconds(throwWeaponL);
        Debug.LogWarning("ThrowWeapon : AttackLatency_3"); ;
        _canFire[ConstNums.throwWeaponIndex] = true;
        _state = State.LIVE;
    }


    public Vector3 CalcAttackDir()
    {
        Vector3 dir = _input.mouseVec - transform.position;
        dir.Normalize();
        return dir;
    }

    public void TakeDamage(object[] ownerInfo, float damageToPlayer, float damageToObject, Vector2 startVector, bool isAttacker)
    {
        if(_info._currentHP > 0)
        {
            if ((bool)ownerInfo[(int)InfoIdx.ISMAGNET] == false) // 자기장이 아니면
            {
                ObjectPoolManager.AllocObject("Hit", transform.position + new Vector3((-1 * startVector).x, (-1 * startVector).y, 7));
            }
        }

        if (_photonView.IsMine == false)
        {
            return;
        }

        if (_info._currentHP > 0)
        {
            //임시구현. Vector2 startVector와 transform.Translate 값을 이용하여 d값을 구하고 그 값에 따라 다른 데미지 부여 필요
            float calcDamage = damageToPlayer * (1 - (_info._defensive / (defenseConstant + _info._defensive)));
            _info.AddValueForHP(-calcDamage);

            
            _rbody.velocity = Vector2.zero;
            StartCoroutine(Knockback(startVector, transform.position));
        }

        if ((_info._currentHP <= 0) && (PlayerManager.IsDead() == false))
        {
            PlayerManager.NotifyDeath(ownerInfo);
            PlayerManager._isMyPlayerDead = true;
            Cursor.visible = true;
            _info.HUD.ChangeActiveCursor(false);

            _photonView.RPC("RPC_Dead", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    public void FlameShot(object[] ownerInfo, Vector3 direction, Vector3 position, Quaternion angle)
    {
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
                Debug.Log("Not Player or Dummy");
            }
        }
    }
    // ~ Attack()
}
