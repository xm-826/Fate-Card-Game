using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MackySoft.SerializeReferenceExtensions;


[CreateAssetMenu(menuName ="Data/Card")]
public class CardData : ScriptableObject
{
    [field:SerializeField] public string UpText{get;private set;}
    [field: SerializeField] public string MidText{get;private set;}
    [field:SerializeField] public string BottomText{get;private set;}
    [field:SerializeField] public int CostTime{get;private set;}
    [field:SerializeField] public Sprite Image{get;private set;}

    //为卡牌创建效果列表。。Unity无法序列化抽象类
    [field:SerializeReference,SubclassSelector] public List<Effect> Effects{get;private set;} = new();
}
