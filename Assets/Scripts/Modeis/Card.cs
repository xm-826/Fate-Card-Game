

using UnityEngine;

public class Card 
{
    private readonly CardData data;
    public string UpText => data.UpText;
    public string MidText => data.MidText;
    public string BootomText => data.BottomText;
    public Sprite Image => data.Image;
    public int costTime{get;private set;}
    public Card(CardData cardData)
    {
        data =cardData;
        costTime = cardData.CostTime;
        }


}
