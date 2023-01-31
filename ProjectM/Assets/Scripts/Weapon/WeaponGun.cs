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

    //�ӽ÷� Initialize���ִ� ��.
    private void Start()
    {
        ParseData();
    }

    //���� ������ �Ľ��� �̿��� �ҷ��ͼ� ���� �ʱ�ȭ
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

