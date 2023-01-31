using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    NULL = 0,
    WEAPONGUN,
    WEAPONSUB,
    WEAPONTHROW,
    RECOVERY,
    DEFENSIVE,
    SCOPE,
    AMMO
};

public enum BulletType
{
    AMMORED = 0,
    AMMOGREEN,
    AMMOBLUE,
    NULL
};

public enum DefensiveType
{
    HELMET = 0,
    ARMOR,
    BAG,
    NULL
}

public enum RecoverType
{
    BAND = 0,
    KIT,
    SODA,
    DRUG,
    NULL
}

public enum Phase
{
    WAIT = 0,
    FARM1,
    DECRESE1,
    FARM2,
    DECRESE2,
    FARM3,
    DECRESE3,
    FARM4,
    DECRESE4,
    FARM5,
    DECRESE5
}

public enum ThrowType
{
    GRENADE = 0,
    FOG,
    NULL
}

public enum InfoIdx
{
    NAME = 0,
    WEAPON,
    ISMAGNET
}