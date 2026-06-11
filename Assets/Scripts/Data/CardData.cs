using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Data/Card")]
public class CardData : ScriptableObject
{
    [field:SerializeField] public string UpText{get;private set;}
    [field: SerializeField] public string MidText{get;private set;}
    [field:SerializeField] public string BottomText{get;private set;}
    [field:SerializeField] public int CostTime{get;private set;}
    [field:SerializeField] public Sprite Image{get;private set;}
}
