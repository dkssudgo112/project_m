using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemScope : Item
{
    public int lensPower;

    public override void ParseData()
    {
        base.ParseData();
        lensPower = itemData.lensPower;
    }
}

