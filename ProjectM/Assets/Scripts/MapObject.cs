using UnityEngine;
using Photon.Pun;


public class MapObject : MonoBehaviourPun, IDamageable
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
    private Collider2D _collider;

    [SerializeField]
    private AudioClip _audioSrcHit = null;
    private AudioSource _audioSource = null;

    void Awake()
    {
        SetState();
    }

    void Start()
    {
        _basicScale = transform.localScale;
        _minScale = _basicScale / 2;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _audioSource = GetComponent<AudioSource>();
        _collider = GetComponent<Collider2D>();
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

        //���� ������ ��� �Ŀ� Vector2���� �̿��� �Ÿ��� �ٸ� ������ ���� �ʿ�
        if (_isBreakable == true)
        {
            //�Ÿ��� ������ ���� �ʿ�
            _currentDurability -= damageToObject;
            
            //ũ�� ����. �Ŀ� �ٲ㵵 �Ǵ� �κ�
            transform.localScale = _minScale + _minScale * _currentDurability / _maxDurability;
            if(transform.localScale.x < 0)
            {
                transform.localScale = new Vector3(0,0,0);
            }

            ObjectPoolManager.AllocObject("Hit", transform.position + new Vector3((-1 * startVector).x, (-1 * startVector).y, 7));
            if (_audioSrcHit != null)
            {
                _audioSource.PlayOneShot(_audioSrcHit);

            }
        }

        if (_currentDurability <= 0)
        {
            if(isAttacker == true)
            {
                _isBreaked = true;
                if (_audioSource != null)
                {
                    _audioSource.Stop();
                    _audioSource.Play();
                }
                gameObject.GetComponent<PhotonView>().RPC("BreakThisObj", RpcTarget.OthersBuffered);
                _isAttacker = true;
                _startVector = startVector;
                
                GetComponent<UnityEngine.Rendering.Universal.ShadowCaster2D>().enabled = false;
                _collider.enabled = false;
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
                        DropItemManager.DropRandomItem(transform.position, _startVector);

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
        if (_audioSource != null)
        {
            _audioSource.Play();
        }
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