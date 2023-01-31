using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 22.07.29_jayjeong
/// <br> Weighted Random Sampling 알고리즘을 통해 각 item에 무게를 설정하여 무게에 비례해 랜덤한 아이템을 산출하는 클래스</br>
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

    // 무게와 아이템이 짝인 Pair 리스트 
    public List<ItemInfo> itemList = new List<ItemInfo>();

    /// <summary>
    /// Random 아이템 리스트에 item을 weight의 무게로 추가 
    /// </summary>
    /// <param name="item"></param>
    /// <param name="weight"></param>
    public void Add(T item, float weight)
    {
        itemList.Add(new ItemInfo(item, weight));
    }

    /// <summary>
    /// 설정된 itemList에서 무게에 비례해 랜덤한 item을 산출해 반환 
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