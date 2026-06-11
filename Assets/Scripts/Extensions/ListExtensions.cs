using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//为列表类添加新功能
public static class ListExtensions
{
    //随机获取列表中的卡牌
    public static T Draw<T>(this List<T> list)
    {
        if(list.Count ==0) return default;
        int r = Random.Range(0,list.Count);
        T t =list[r];
        list.Remove(t);
        return t;
    }
}
