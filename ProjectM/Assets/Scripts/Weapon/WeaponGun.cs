using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WeaponGun : Weapon
{

    [SerializeField]
    private GameObject bulletPrefab;
    private float weaponLength;
    private int costBullet;
    public bool isBomb;
    public int maxBullet;
    public BulletType bulletType;
    public float loadedTime;

    //임시로 Initialize해주는 중.
    private void Start()
    {
        ParseData();
    }

    //이후 데이터 파싱을 이용해 불러와서 각각 초기화
    public override void ParseData()
    {
        base.ParseData();

        maxBullet = weaponData.maxBullet;
        bulletType = weaponData.bulletType;
        loadedTime = weaponData.loadedTime;
        costBullet = weaponData.costBullet;
        isBomb = weaponData.isBomb;
    }

    public override void Attack(Vector2 direction, Vector2 position)
    {
    }
}

