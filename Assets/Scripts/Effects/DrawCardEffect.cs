using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DrawCardEffect : Effect
{
    //用于查看抽卡数量
    [SerializeField] private int drawCount;
    //当卡牌打出时，效果调用这个方法，把自己"翻译"成一个 DrawCardGA 动作对象
    //然后交给 ActionSystem 执行。
    public override GameAction GetGameAction()
    {
        //调用抽卡动作
        DrawCardGA drawCardGA = new(drawCount);
        return drawCardGA;
    }
}
