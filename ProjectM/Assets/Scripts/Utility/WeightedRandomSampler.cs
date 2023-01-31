using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 22.07.29_jayjeong
/// <br> Weighted Random Sampling �˰����� ���� �� item�� ���Ը� �����Ͽ� ���Կ� ����� ������ �������� �����ϴ� Ŭ����</br>
/// </summary>
/// <typeparam name="T"></typeparam>
[System.Serializable]
public class WeightedRandomSampler<T>
{
    [Serializable]
    public struct ItemInfo
    {
        public T _item;
        public float _weight;

        public ItemInfo(T item, float weight)
        {
            _item = item;
            _weight = weight;
        }
    }

    // ���Կ� �������� ¦�� Pair ����Ʈ 
    public List<ItemInfo> itemList = new List<ItemInfo>();

    /// <summary>
    /// Random ������ ����Ʈ�� item�� weight�� ���Է� �߰� 
    /// </summary>
    /// <param name="item"></param>
    /// <param name="weight"></param>
    public void Add(T item, float weight)
    {
        itemList.Add(new ItemInfo(item, weight));
    }

    /// <summary>
    /// ������ itemList���� ���Կ� ����� ������ item�� ������ ��ȯ 
    /// </summary>
    /// <returns></returns>
    public T GetRandomValue()
    {
        float totalWeight = 0f;

        foreach (ItemInfo item in itemList)
        {
            totalWeight += item._weight;
        }

        float randomValue = UnityEngine.Random.value * totalWeight;
        float sumWeight = 0f;

        foreach (ItemInfo item in itemList)
        {
            sumWeight += item._weight;

            if (sumWeight >= randomValue)
            {
                return item._item;
            }
        }

        return default(T);
    }
}