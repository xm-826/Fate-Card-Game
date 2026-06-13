using System.Collections.Generic;
using UnityEngine;

//卡牌类
public class Card 
{
    private readonly CardData data;
    public string UpText => data.UpText;
    public string MidText => data.MidText;
    public string BootomText => data.BottomText;
    public Sprite Image => data.Image;

    //提取效果
    public List<Effect> Effects => data.Effects;
    public int costTime{get;private set;}
    public Card(CardData cardData)
    {
        data =cardData;
        costTime = cardData.CostTime;
    }


}
