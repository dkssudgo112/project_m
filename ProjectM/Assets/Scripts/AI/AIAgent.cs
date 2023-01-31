using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

using Photon.Pun;



public class AIAgent : Agent, IDamageable
{
    private const float AIDamageToPlayer = 50.0f;
    private const float AIDamageToObject = 100.0f;
    private const float AIAttackRange = 20.0f;
    private const float MinAIAttackRange = 2.0f;
    private const float AIAttackLatency = 0.4f;
    private const float timerWaterEffect = 0.3f;

    private const float objectDistance = 80.0f;
    private const float mapDistance = 200.0f;

    private const int maxHP = 1500;

    public AudioClip _audioFootStep = null;
    public AudioClip _audioFootStepInWater = null;

    public string _AIname = "";
    public float _currentHP = 1000;
    public float _basicSpeed = 5f;
    public MagnetActivator _activator;

    private AudioSource _audioSource = null;
    private Rigidbody2D _rbody = null;
    private Animator _anim = null;
    private float _angle = 0.0f;
    private float _tickLatency = 0.0f;
    private float _tickTimerWaterEffect = 0.0f;
    private bool _beingMove = false;
    private bool _canFire = true;
    private bool _isDead = false;
    private bool isOnRiver = false;

    private string[] collidabelayers = { "Player, Object" };

    private void Awake()
    {
        _rbody = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        _canFire = true;
        _beingMove = false;

        _rbody.isKinematic = false;
        _rbody.gravityScale = 0.0f;
        _rbody.angularDrag = 0.0f;
        _anim.SetBool("isGun", true);

      //_audioSource.clip = _audioFootStep;
      //_audioSource.Play();


    }

    public override void OnEpisodeBegin()
    {

        _rbody.velocity = Vector2.zero;     
        _canFire = true;
        _tickLatency = 0.0f;
        _currentHP = maxHP;

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

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }

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

      //if (isOnRiver)
      //{
      //    _audioSource.clip = _audioFootStepInWater;
      //    _audioSource.Play();
      //}
      //else
      //{
      //    _audioSource.clip = _audioFootStep;
      //    _audioSource.Play();
      //}
                                                 
                                                                
        if (_canFire == false)                                  
        {                                                       
            _tickLatency += Time.deltaTime;                     
        }                                                       
                                                                
        if(_tickLatency > AIAttackLatency)                      
        {                                                       
            _canFire = true;                                    
            _tickLatency = 0.0f;                                
        }


        if(Vector3.Distance(transform.position, _activator._randomCenterPoints[5]) < objectDistance)
        {
            SetReward(1.0f);
        }

        RaycastHit2D[] rayhit = Physics2D.CircleCastAll(transform.position, AIAttackRange, Vector2.left, 0.0f);

        for (int i = 0; i < rayhit.Length; i++)
        {
            if (rayhit[i].collider != null)
            {
                if (rayhit[i].collider.CompareTag("Player") == true )
                {
                    if (_canFire == true)
                    {
                        TurnBodyAndShoot(rayhit[i].collider);
                    }
                }
                else if(rayhit[i].collider.CompareTag("Dummy") == true)
                {
                    if (_canFire == true)
                    {
                        TurnBodyAndShoot(rayhit[i].collider);
                    }
                }
                else if (rayhit[i].collider.CompareTag("AI") == true)
                {
                    if (Vector3.Distance(transform.position, rayhit[i].collider.transform.position) > MinAIAttackRange)
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

        if (collision.gameObject.tag == "River")
        {

            isOnRiver = true;

        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {

        if (collision.tag == "River")
        {
            isOnRiver = false;
        }
    }

    private void SetBeingMoveTrue()
    {
        _beingMove = false;
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, AIAttackRange);
    }

    public void TakeDamage(object[] ownerInfo, float damageToPlayer, float damageToObject, Vector2 startVector, bool isAttacker)
    {

        if (_currentHP > 0)
        {
            _currentHP -= damageToPlayer;
            ObjectPoolManager.AllocObject("Hit", transform.position + new Vector3((-1 * startVector).x, (-1 * startVector).y, 7));
        }

        if (_currentHP <= 0 && _isDead == false)
        {
            _isDead = true;
            SetReward(-0.5f);

            if (PhotonNetwork.IsMasterClient == true)
            {
                PlayerManager.NotifyDeathAI(_AIname, ownerInfo);
                this.GetComponent<PhotonView>().RPC("RPC_Death", RpcTarget.AllBufferedViaServer, ownerInfo);
            }

        }
    }

    float AngleBetweenTwoPoints(Vector3 a, Vector3 b)
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
        Vector3 dir = new Vector3();
        dir = (collision.transform.position - transform.position);
        dir.Normalize();

        OwnerInfo bulletOwner = new OwnerInfo(_AIname, "");

        if(dir == new Vector3(0, 0, 0))
        {
            return;
        }

        Shot(bulletOwner.GetObjects(), dir, transform.position, AIDamageToPlayer, AIDamageToObject, AIAttackRange, 0.0f, false);
    }

    private void OnTriggerStay2D(Collider2D collision) // rigid -> never sleep option
    {
        if (collision.CompareTag("River"))
        {
            _tickTimerWaterEffect += Time.deltaTime;
            if (_tickTimerWaterEffect > timerWaterEffect)
            {
                _tickTimerWaterEffect = 0.0f;
                ObjectPoolManager.AllocObject("WaterEffect", transform.position);
            }
        }


        if (collision.tag == "Item")
        {
            Item target = null;
            if (collision.TryGetComponent<Item>(out target))
            {
                target.gameObject.SetActive(false);
                _beingMove = false;
            }
            else
            {
                Debug.Log("tartget is null in AI");
            }
        }
    }

    private void OnCollisionEnter2D(Collision collision)
    {
        if (collision.gameObject.CompareTag("Buliding"))
        {
            _beingMove = true;
            Invoke("SetBeingMoveTrue", 1.0f);

            _rbody.velocity = (collision.transform.position - transform.position) * _basicSpeed * -100 ;
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
        //PlayerManager.NotifyDeathAI(_AIname, enemyInfo);
        this.gameObject.SetActive(false);
    }


}
