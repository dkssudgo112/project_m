using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDefensive : Item
{
    public DefensiveType defensiveType;
    public float defensive;
    public int level;

    //helmet : 8%, 12%, 20%
    //armor : 25%, 38%, 45%

    //helmet : 5%, 8%, 15%
    //armor : 15%, 20%, 30%

    public override void ParseData()
    {
        base.ParseData();
        defensiveType = itemData.defensiveType;
        defensive = itemData.defensive;
        level = itemData.level;
    }
}
