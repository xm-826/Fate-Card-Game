using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//该悬停系统是先隐藏手牌中的组件，再启用专门设置的悬停时的组件
public class CardViewHoverSystem : Singleton<CardViewHoverSystem>
{
    [SerializeField] private CardView cardViewHover;
    public void Show(Card card,Vector3 position)
    {
        cardViewHover.gameObject.SetActive(true);
        cardViewHover.SetUp(card);
        cardViewHover.transform.position =position;
    }

    public void Hide()
    {
        cardViewHover.gameObject.SetActive(false);
    }
}
