using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Photon.Pun;

public class AIAgent : Agent, IDamageable
{
    private const float aiDamageToPlayer = 50.0f;
    private const float aiDamageToObject = 100.0f;
    private const float aiAttackRange = 20.0f;
    private const float minAIAttackRange = 2.0f;
    private const float aiAttackLatency = 0.4f;
    private const float timerWaterEffect = 0.3f;
    private const float objectDistance = 80.0f;
    private const float mapDistance = 200.0f;
    private const int maxHP = 1500;

    public MagnetActivator _activator;
    public AudioClip _audioFootStep = null;
    public AudioClip _audioFootStepInWater = null;
    public AudioClip _audioSrcHit = null;
    public string _AIname = "";
    public float _currentHP = 1000;
    public float _basicSpeed = 5f;
    
    private AudioSource _audioSource = null;
    private Rigidbody2D _rbody = null;
    private Animator _anim = null;
    private float _angle = 0.0f;
    private float _tickLatency = 0.0f;
    private float _tickTimerWaterEffect = 0.0f;
    private bool _beingMove = false;
    private bool _canFire = true;
    private bool _isDead = false;

    private void Awake()
    {
        InitByGetComponent();
    }

    private void Start()
    {
        InitMemberSetting();
    }

    public override void OnEpisodeBegin()
    {
        InitEpisodeSetting();
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }

        if (NetworkManager.Instance.IsEnd() == true)
        {
            StopActionAndAudio();
            return;
        }

        MoveAndRotation(actions);

        CanFireTimer();

        EpisodeEnding();

        FindObjectInArround();

    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Outline"))
        {
            SetReward(-1.0f);
        }

        if (collision.gameObject.CompareTag("Buliding"))
        {
            SetReward(-0.1f);
        }


    }

    private void OnTriggerStay2D(Collider2D collision) // rigid -> never sleep option
    {
        if (collision.CompareTag("River"))
        {
            AllocWaterEffectWithTimer();
        }


        if (collision.tag == "Item")
        {
            AcquireItem(collision);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(_rbody.velocity.x); // 1
        sensor.AddObservation(_rbody.velocity.y); // 1
        Vector3 direction = (_activator._randomCenterPoints[5] - transform.position);
        direction.Normalize();
        var nDistance = Vector3.Distance(transform.position, _activator._randomCenterPoints[5]) / mapDistance;
        sensor.AddObservation(direction);// 3
        sensor.AddObservation(nDistance);// 1
    }

    private void InitByGetComponent()
    {
        _rbody = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void InitMemberSetting()
    {
        _canFire = true;
        _beingMove = false;
        _rbody.isKinematic = false;
        _rbody.gravityScale = 0.0f;
        _rbody.angularDrag = 0.0f;
        _anim.SetBool("isGun", true);
    }

    private void InitEpisodeSetting()
    {
        _rbody.velocity = Vector2.zero;
        _canFire = true;
        _tickLatency = 0.0f;
        _currentHP = maxHP;
    }

    private void StopActionAndAudio()
    {
        _rbody.constraints = RigidbodyConstraints2D.FreezeAll;
        _audioSource.Stop();
    }

    private void MoveAndRotation(ActionBuffers actions)
    {
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actions.ContinuousActions[0];
        controlSignal.y = actions.ContinuousActions[1];
        controlSignal.Normalize();

        transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, _angle));
        _angle = AngleBetweenTwoPoints(transform.right, controlSignal);

        if (_beingMove == false)
        {
            _rbody.velocity = ((controlSignal) * _basicSpeed);
        }
        SetReward(-0.001f);
    }

    private void CanFireTimer()
    {
        if (_canFire == false)
        {
            _tickLatency += Time.deltaTime;
        }

        if (_tickLatency > aiAttackLatency)
        {
            _canFire = true;
            _tickLatency = 0.0f;
        }
    }

    private void EpisodeEnding()
    {
        if (Vector3.Distance(transform.position, _activator._randomCenterPoints[5]) < objectDistance)
        {
            SetReward(1.0f);
        }
    }

    private void FindObjectInArround()
    {
        RaycastHit2D[] rayhit = Physics2D.CircleCastAll(transform.position, aiAttackRange, Vector2.left, 0.0f);

        for (int i = 0; i < rayhit.Length; i++)
        {
            if (rayhit[i].collider != null)
            {
                if (rayhit[i].collider.CompareTag("Player") == true)
                {
                    if (_canFire == true)
                    {
                        TurnBodyAndShoot(rayhit[i].collider);
                    }
                }
                else if (rayhit[i].collider.CompareTag("Dummy") == true)
                {
                    if (_canFire == true)
                    {
                        TurnBodyAndShoot(rayhit[i].collider);
                    }
                }
                else if (rayhit[i].collider.CompareTag("AI") == true)
                {
                    if (Vector3.Distance(transform.position, rayhit[i].collider.transform.position) > minAIAttackRange)
                    {
                        if (_canFire == true)
                        {
                            TurnBodyAndShoot(rayhit[i].collider);
                        }
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, aiAttackRange);
    }

    public void TakeDamage(object[] ownerInfo, float damageToPlayer, float damageToObject, Vector2 startVector, bool isAttacker)
    {

        if (_currentHP > 0)
        {
            _currentHP -= damageToPlayer;
            if ((bool)ownerInfo[(int)InfoIdx.ISMAGNET] == false)
            {
                ObjectPoolManager.AllocObject("Hit", transform.position + new Vector3((-1 * startVector).x, (-1 * startVector).y, 7));
                if (_audioSrcHit != null)
                {
                    _audioSource.PlayOneShot(_audioSrcHit);
                }
            }
        }

        if (_currentHP <= 0 && _isDead == false)
        {
            _isDead = true;
            SetReward(-0.5f);

            if (PhotonNetwork.IsMasterClient == true)
            {
                PlayerManager.Instance.NotifyDeathAI(_AIname, ownerInfo);
                this.GetComponent<PhotonView>().RPC("RPC_Death", RpcTarget.AllBufferedViaServer, ownerInfo);
            }

        }
    }
    private float AngleBetweenTwoPoints(Vector3 a, Vector3 b)
    {
        return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
    }
    private void TurnBodyAndShoot(Collider2D collision)
    {
        Vector3 laydir = collision.transform.position - transform.position;
        laydir.Normalize();
        RaycastHit2D rayhit = Physics2D.Raycast(transform.position, laydir, Vector3.Distance(transform.position, collision.transform.position), LayerMask.NameToLayer("Object"));
        
        if (rayhit)
        {
            if (rayhit.collider.CompareTag("Buliding"))
            {
                return;
            }
        }

        _canFire = false;
        _angle = AngleBetweenTwoPoints(transform.position, collision.transform.position);
        Vector2 dir = new Vector2();
        dir = (collision.transform.position - transform.position);
        dir.Normalize();

        OwnerInfo bulletOwner = new OwnerInfo(_AIname, "¾Ë ¼ö ¾ø´Â Èû", this.gameObject.GetPhotonView().ViewID);

        Shot(bulletOwner.GetObjects(), dir, transform.position, aiDamageToPlayer, aiDamageToObject, aiAttackRange, 0.0f, false);
    }

    private void AllocWaterEffectWithTimer()
    {
        _tickTimerWaterEffect += Time.deltaTime;
        if (_tickTimerWaterEffect > timerWaterEffect)
        {
            _tickTimerWaterEffect = 0.0f;
            ObjectPoolManager.AllocObject("WaterEffect", transform.position);
        }
    }

    private void AcquireItem(Collider2D collision)
    {
        Item target = null;
        
        if (collision.TryGetComponent<Item>(out target))
        {
            NetworkManager.Instance.DisableItemByViewID(target.itemData.viewID);
            _beingMove = false;
        }
        else
        {
            Debug.Log("tartget is null in AI");
        }
    }

    public void Shot(object[] ownerInfo, Vector3 direction, Vector3 position, float damageToPlayer, float damageToObject, float attackRange, float fallOff, bool isBomb)
    {
        Vector3 pos = new Vector3(position.x, position.y, 0f);
        _anim.SetTrigger("shoot");
        if (!isBomb)
        {
            var bullet = ObjectPoolManager.AllocObject<Bullet>("Bullet", pos + direction * 2);
            bullet.transform.position = pos + direction * 2;
            bullet.SetData(ownerInfo, direction, position, damageToPlayer, damageToObject, attackRange, fallOff, isBomb);

        }
        else
        {
            var bullet = ObjectPoolManager.AllocObject<BombBullet>("BombBullet", pos + direction * 2);
            bullet.transform.position = pos + direction * 2;
            bullet.SetData(ownerInfo, direction, position, damageToPlayer, damageToObject, attackRange, fallOff, isBomb);

        }
        GetComponent<PhotonView>().RPC("RPC_Shot", RpcTarget.OthersBuffered, ownerInfo, direction, position, damageToPlayer,damageToObject, attackRange, fallOff, isBomb);
    }

    [PunRPC]
    public void RPC_Shot(object[] ownerInfo, Vector3 direction, Vector3 position, float damageToPlayer, float damageToObject, float attackRange, float fallOff, bool isBomb)
    {
        Vector3 pos = new Vector3(position.x, position.y, 0f);
        _anim.SetTrigger("shoot");
        if (!isBomb)
        {
            var bullet = ObjectPoolManager.AllocObject<Bullet>("Bullet", pos + direction * 2);
            bullet.transform.position = pos + direction * 2;
            bullet.SetData(ownerInfo, direction, position, damageToPlayer, damageToObject, attackRange, fallOff, isBomb);

        }
        else
        {
            var bullet = ObjectPoolManager.AllocObject<BombBullet>("BombBullet", pos + direction * 2);
            bullet.transform.position = pos + direction * 2;
            bullet.SetData(ownerInfo, direction, position, damageToPlayer, damageToObject, attackRange, fallOff, isBomb);

        }
    }

    [PunRPC]
    public void RPC_Death(object[] enemyInfo)
    {
        this.gameObject.SetActive(false);
    }


}
