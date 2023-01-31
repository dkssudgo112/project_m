using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 22.07.29_jayjeong
/// <br>���Կ� ����� ������ ���� �����ϴ� Ŭ����</br>
/// </summary>
/// <typeparam name="T"></typeparam>
[System.Serializable]
public class WeightedRandomList<T>
{
    [System.Serializable]
    public struct ItemInfo
    {
        public T item;
        public float weight;

        public ItemInfo(T item, float weight)
        {
            this.item = item;
            this.weight = weight;
        }
    }

    // ���Կ� �������� ¦�� Pair ����Ʈ 
    public List<ItemInfo> itemList = new List<ItemInfo>();

    public int Count { get => itemList.Count; }

    public void Add(T item, float weight)
    {
        itemList.Add(new ItemInfo(item, weight));
    }

    public T GetRandomValue()
    {
        float totalWeight = 0f;

        foreach (ItemInfo item in itemList)
        {
            totalWeight += item.weight;
        }

        float randomValue = Random.value * totalWeight;
        float sumWeight = 0f;

        foreach (ItemInfo item in itemList)
        {
            sumWeight += item.weight;

            if (sumWeight >= randomValue)
                return item.item;
        }

        return default(T);
    }
}