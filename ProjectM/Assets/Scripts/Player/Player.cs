using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum State
{
    LIVE,
    ATTACKING,
    ATTACKLATENCY,
    RELOADING,
    RECOVERYING,
    DEAD
}

public partial class Player : MonoBehaviourPunCallbacks, IDamageable
{
    public static PlayerInfo _info = null;
    public PhotonView _photonView = null;
    public int _curSlot = ConstNums.subWeaponIndex;
    public Transform _basicAtkBox = null;
    public Vector2 _basicAtkSize = new Vector2(0, 0);
    public AudioClip _audioGun = null;
    public AudioClip _audioFootStep = null;
    public AudioClip _audioFootStepInWater = null;
    public Texture2D cursorImg = null;
    

    private const int defenseConstant = 100;
    private const float throwWeaponL = 1.0f;
    private const float ninetyNinePer = 0.99f;
    private const float KnockBackPower = 0.3f;
    private const float tenPer = 0.1f;
    private const float timerWaterEffect = 0.3f;

    private static Player Instance = null;
    private static PlayerInput _input = null;
    private Rigidbody2D _rbody = null;
    private Animator _anim = null;
    private State _state = State.LIVE;
    private AudioSource _audioSource = null;
    private bool isOnRiver = false;
    private float _tickTimerWaterEffect = 0.0f;

    // 무기 슬롯마다 다른 Latency적용을 위해 필요함
    private bool[] _canFire = new bool[ConstNums.numberOfWeapon] { true, true, true, true, true };

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.Log("_Instance isn't NULL");
        }

        if (_info == null)
        {
            _info = this.gameObject.GetComponent<PlayerInfo>();
        }
        else
        {
            Debug.Log("_info isn't NULL");
        }

        if (_input == null)
        {
            _input = this.gameObject.GetComponent<PlayerInput>();
        }
        else
        {
            Debug.Log("_input isn't NULL");
        }

        _rbody = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();

        _rbody.isKinematic = false;
        _rbody.gravityScale = 0.0f;
        _rbody.angularDrag = 0.0f; // This 3 lines to be deleted after complete player prefab
    }

    void Start()
    {
        if (UIManager._Instance != null)
        {
            _info.HUD = UIManager._Instance;
            _info.HUD.SendPlayerData(Instance, _info);
        }
        else
        {
            Debug.Log("UIManager._Instance is NULL");
        }

        if(_photonView.IsMine == false)
        {
            _audioSource.clip = _audioFootStep;
            _audioSource.loop = true;
            _audioSource.Play();
        }

    }

    private void Update()
    {
        

        if (_photonView.IsMine == false)
        {
            return;
        }

        UpdateSound();
        // Part : Body rotaion - World좌표
        PlayerRotationByMouse();

        // Part : Body moving
        _rbody.velocity = (_input.move * (_info._basicSpeed + _info._eventSpeed)) * Time.deltaTime;
        _rbody.angularVelocity = 0f;

        //update sound
        

        // Reloading() ~
        if (_input.reload == true)
        {
            if (CanReloadingGun() == true)
            {
                StartCoroutine("ReloadingGun");
            }
        }
        // ~ Reloading()

        // State Changing() ~ 
        if (_input.slots[0] == true)
        {
            TryChangeSlot(ConstNums.primaryWeapon1Index);
        }
        if (_input.slots[1] == true)
        {
            TryChangeSlot(ConstNums.primaryWeapon2Index);
        }
        if (_input.slots[2] == true)
        {
            TryChangeSlot(ConstNums.subWeaponIndex);
        }
        if (_input.slots[3] == true)
        {
            TryChangeSlot(ConstNums.throwWeaponIndex);
        }

        if (_input.slots[6] == true)
        {
            CheckUsingItem(_info.CastIntRecoverType(RecoverType.BAND));
        }
        if (_input.slots[7] == true)
        {
            CheckUsingItem(_info.CastIntRecoverType(RecoverType.KIT));
        }

        if (_input.slots[8] == true)
        {
            CheckUsingItem(_info.CastIntRecoverType(RecoverType.SODA));
        }
        if (_input.slots[9] == true)
        {
            CheckUsingItem(_info.CastIntRecoverType(RecoverType.DRUG));
        }
        // ~ State Changing()

        // Attack() ~
        if (_input.attack == true)
        {
            Attack();
        }
        // ~ Attack()

        
    }

    private void PlayerRotationByMouse()
    {
        float angle = AngleBetweenTwoPoints(transform.position, _input.mouseVec);
        transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, angle));
    }

    private float AngleBetweenTwoPoints(Vector3 a, Vector3 b)
    {
        return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
    }

    #region ChangeSlot
    public void TryChangeSlot(int slotIndex)
    {
        if (slotIndex > ConstNums.numberOfPlayerSlot)
        {
            Debug.LogError($"slotIndex is {slotIndex}");
            return;
        }

        if(slotIndex == ConstNums.throwWeaponIndex)
        {
            if(_info.IsNullThrowWeaponSlot() == true)
            {
                Debug.Log("IsNullThrowWeaponSlot()");
                return;
            }
            else
            {
                ChangeThrowWeaponSlot();
                return;
            }
        }
        else if (_info.IsNullWeaponSlot(slotIndex))
        {
            Debug.Log($"{slotIndex} slot is null");
            return;
        }

        ChangeSlot(slotIndex);
    }

    public void ChangeSlot(int slotIndex)
    {
        StopAction();

        // 현재 무기 교체, 현재 슬롯인덱스 교체, HUD 교체
        _curSlot = slotIndex;
        _info.HUD.ChangeCurSlot(_curSlot);

        //총기 슬롯이라면
        if (_info.IsGunSlot(_curSlot) == true)
        {

            photonView.RPC("RPC_ChangeAnim", RpcTarget.AllBuffered, "Gun");
            if (_info.GetWeaponInSlot(_curSlot).itemName == "rocketlauncher")
            {
                Cursor.visible = false;
                _info.HUD.ChangeActiveCursor(true);
            }
            else
            {
                Cursor.visible = true;
                _info.HUD.ChangeActiveCursor(false);
            }

            //총기에 현재 장전된 총알이 0발이고, 그 형태의 총알이 있다면 자동으로 장전을 시작해준다.
            AutoReloading();
        }
        else if (_info.IsSubSlot(_curSlot) == true)
        {
            Cursor.visible = true;
            _info.HUD.ChangeActiveCursor(false);

            if (_info.GetWeaponInSlot(_curSlot).itemName == "Fist")
            {
                photonView.RPC("RPC_ChangeAnim", RpcTarget.AllBuffered, _info.GetWeaponInSlot(_curSlot).itemName);
            }
            else if (_info.GetWeaponInSlot(_curSlot).itemName == "hammer")
            {
                photonView.RPC("RPC_ChangeAnim", RpcTarget.AllBuffered, _info.GetWeaponInSlot(_curSlot).itemName);
            }
        }
    }

    public void ChangeThrowWeaponSlot()
    {
        //이미 수류탄 슬롯이었다면, 수류탄을 스왑해줘야한다.
        if (_curSlot >= ConstNums.throwWeaponIndex)
        {
            //다른 수류탄 종류가 있는지 확인하고 스왑해줘야함 
            if (_info.IsNullOtherThrowWeapon(_curSlot) == false)
            { 
                _curSlot = _info.ChangeThrowIndex(_curSlot);
                //_info.HUD.RefreshSlotThrowWeapon(_info._slotWeapon[_curSlot].GetWeapon());
            }
            return;
        }
        _curSlot = ConstNums.throwWeaponIndex + _info._curThrowWeapon;
        _info.HUD.ChangeCurSlot(ConstNums.throwWeaponIndex);
        return;
    }
    #endregion

    private void StopAction()
    {
        if (_state == State.RELOADING)
        {
            _info.HUD.SetActiveReloadingView(false);
            StopCoroutine("ReloadingGun");
            _state = State.LIVE;
        }
        if(_state == State.RECOVERYING)
        {
            _info.HUD.SetActiveRecoveringView(false);
            StopCoroutine("RecoverCooltime");
            _state = State.LIVE;
        }
    }
    
    //HUD Update 함수 모음
    private void HUDUpdate_slotBullet()
    {
        _info.HUD.RefreshSlotBullet(_info._loadedBullet[_curSlot], _info._bulletSlot[_info.CastIntBulletType(_curSlot)].itemCount);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        _info.HUD.ActiveOffPressFView();

        if (collision.tag == "River")
        {
            isOnRiver = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
      //if (collision.tag == "Bullet")
      //{
      //    Transform colTr = collision.GetComponent<Transform>();
      //    _rbody.velocity = Vector2.zero;
      //    StartCoroutine(Knockback(colTr.position, transform.position));
      //    
      //}
      //else
      //{
      //    Debug.Log("This is collision.tag != bullet");
      //}

        if (collision.tag == "River")
        {
            
            isOnRiver = true;

        }
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(_basicAtkBox.position, _basicAtkSize);
    }

    public IEnumerator Knockback(Vector2 dir, Vector3 pos1)
    {
        float duration = tenPer;
        float power = KnockBackPower;
        for (float f = duration; f >= 0; f -= 0.01f)
        {
            transform.position += new Vector3(dir.x, dir.y, 0) * power;
            power *= ninetyNinePer;
            yield return null;
        }
    }

    [PunRPC]
    public void RPC_ChangeAnim(string itemname)
    {
        if(itemname == "Gun")
        {
            _anim.SetBool("isGun", true);
            _anim.SetBool("isFist", false);
            _anim.SetBool("isHammer", false);
        }
        else if(itemname == "hammer")
        {
            _anim.SetBool("isGun", false);
            _anim.SetBool("isFist", false);
            _anim.SetBool("isHammer", true);
        }
        else if (itemname == "Fist")
        {
            _anim.SetBool("isGun", false);
            _anim.SetBool("isFist", true);
            _anim.SetBool("isHammer", false);
        }
    }

    public void DisableItemByViewID(int viewID)
    {
        Instance._photonView.RPC("RPC_DisableItemByViewID", RpcTarget.AllBuffered, viewID);
    }

    [PunRPC]
    public void RPC_DisableItemByViewID(int viewID)
    {
        ObjectPoolManager.GetItemByViewID(viewID).SetActive(false);
    }

    [PunRPC]
    public void RPC_Dead()
    {
        this.gameObject.SetActive(false);
        Destroy(this.gameObject);
    }

    public void ChangeAnimFist()
    {
        photonView.RPC("RPC_ChangeAnim", RpcTarget.AllBuffered, _info._fist.itemName);
    }

    private bool IsMoving()
    {
        return (_rbody.velocity.x > 0 || _rbody.velocity.y > 0 || _rbody.velocity.x < 0 || _rbody.velocity.y < 0);
    }

    private void UpdateSound()
    {
        if (IsMoving() == true && isOnRiver == false)
        {
            _audioSource.clip = _audioFootStep;
            if (_audioSource.isPlaying == false)
            {
                _audioSource.Play();
            }

        }
        else if (IsMoving() == true && isOnRiver == true)
        {
            _audioSource.clip = _audioFootStepInWater;
            if (_audioSource.isPlaying == false)
            {
                _audioSource.Play();
            }
        }
        else if (IsMoving() == false && _audioSource.clip != _audioGun)
        {
            _audioSource.Stop();
        }
    }
}
