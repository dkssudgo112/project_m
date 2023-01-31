public enum ItemType
{
    NULL = -1,
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
    NULL = -1,
    AMMORED,
    AMMOGREEN,
    AMMOBLUE
};

public enum DefensiveType
{
    NULL = -1, 
    HELMET,
    ARMOR,
    BAG
}

public enum RecoverType
{
    NULL = -1,
    BAND,
    KIT,
    SODA,
    DRUG,
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

public enum Step
{
    FIRST = 0,
    SECOND,
    THIRD,
    FOURTH,
    FINISH
}

public enum ThrowType
{
    NULL = -1,
    GRENADE,
    FOG,
}

public enum InfoIdx
{
    NAME = 0,
    WEAPON,
    ISMAGNET,
    ID
}

public enum Panel
{
    LOGIN = 0,
    SELECTION,
    CREATEROOM,
    INROOM,
    LOADING
}