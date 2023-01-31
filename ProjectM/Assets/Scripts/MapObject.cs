using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class MapObject : MonoBehaviourPunCallbacks, IDamageable
{

    private const float destoryTime = 1.0f;
    private const float TickDescreseColer = 0.03f;

    private float destoryTickTime = 0.0f;
    public float _maxDurability = 0;
    public float _currentDurability = 0;
    public bool _isBreakable = false;
    public bool _isDropItem = false;
    private bool _isBreaked = false;
    private Vector2 _startVector = new Vector2();
    private bool _isAttacker = false;

    private Vector3 _basicScale = new Vector3();
    private Vector3 _minScale = new Vector3();
    private SpriteRenderer _spriteRenderer = null;
    

    void Awake()
    {
        SetState();
    }

    void Start()
    {
        _basicScale = transform.localScale;
        _minScale = _basicScale / 2;
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void SetState()
    {
        _currentDurability = _maxDurability;
    }

    public void TakeDamage(object[] ownerInfo, float damageToPlayer, float damageToObject, Vector2 startVector, bool isAttacker)
    {
        if(_isBreaked == true)
        {
            return;
        }

        //이후 데미지 계산 식에 Vector2값을 이용해 거리별 다른 데미지 적용 필요
        if (_isBreakable == true)
        {
            //거리별 데미지 적용 필요
            _currentDurability -= damageToObject;
            
            //크기 계산식. 후에 바꿔도 되는 부분
            transform.localScale = _minScale + _minScale * _currentDurability / _maxDurability;

            ObjectPoolManager.AllocObject("Hit", transform.position + new Vector3((-1 * startVector).x, (-1 * startVector).y, 7));
        }

        if (_currentDurability <= 0)
        {
            if(isAttacker == true)
            {
                _isBreaked = true;
                gameObject.GetComponent<PhotonView>().RPC("BreakThisObj", RpcTarget.OthersBuffered);
                _isAttacker = true;
                _startVector = startVector;
                
                GetComponent<UnityEngine.Rendering.Universal.ShadowCaster2D>().enabled = false;
            }
        }
    }

    private void Update()
    {
        if(_isBreaked == true)
        {
            if(destoryTickTime < destoryTime)
            {
                destoryTickTime += Time.deltaTime;
                _spriteRenderer.color -= new Color(0, 0, 0, TickDescreseColer);
            }
            else
            {
                if(_isAttacker == true)
                {
                    if (_isDropItem == true)
                    {
                        ItemManager.DropRandomItem(transform.position, _startVector);

                        gameObject.SetActive(false);
                        gameObject.GetComponent<PhotonView>().RPC("DeleteObject", RpcTarget.OthersBuffered);
                    }
                    else
                    {
                        gameObject.SetActive(false);
                        gameObject.GetComponent<PhotonView>().RPC("DeleteObject", RpcTarget.OthersBuffered);
                    }
                }
            }
        }
    }

    #region PunRPC

    [PunRPC]
    public void BreakThisObj()
    {
        _isBreaked = true;
        GetComponent<UnityEngine.Rendering.Universal.ShadowCaster2D>().enabled = false;
    }

    [PunRPC]
    public void CreateObject()
    {
        this.gameObject.SetActive(true);
    }

    [PunRPC]
    public void DeleteObject()
    {
        this.gameObject.SetActive(false);
    }

    #endregion PunRPC
}