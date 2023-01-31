using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstNums
{
    public const byte maxPlayers = 16;
    public const int maxNameSize = 20;
    public const int maxTextCount = 10;

    public const int numberOfPlayerSlot = 4;
    public const int numberOfItemAmmo = 3;
    public const int numberOfItemDefensive = 3;
    public const int numberOfItemRecovery = 4;
    public const int numberOfItemScope = 3;
    public const int numberOfWeapon = 5;
    public const int numberOfWeaponThrow = 2;
    public const int numberOfWeaponGun = 2;
    public const int primaryWeapon1Index = 0;
    public const int primaryWeapon2Index = 1;
    public const int subWeaponIndex = 2;
    public const int throwWeaponIndex = 3;

    #region PhaseTime
    public static readonly float[] PhaseTime = {10.0f,
                                                40.0f,
                                                40.0f,
                                                30.0f,
                                                20.0f,
                                                20.0f,
                                                15.0f,
                                                15.0f,
                                                15.0f,
                                                10.0f,
                                                10.0f};
    public const int lastPhase = 10;
    #endregion
}
