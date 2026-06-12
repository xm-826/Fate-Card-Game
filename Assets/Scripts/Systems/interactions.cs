using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class interactions : Singleton<interactions>
{
    //设置一个bool来记录当前卡牌是否被拖动
    public bool PlayerIsDragging{get;set;} =false;
    //动画状态标志
    public bool CardsAreAnimation ;
    public bool PlayerCanInteract()
    {   
        //执行动画期间卡牌禁止交互
        if(CardsAreAnimation) return false;
        //当动作系统未执行的时候返回true
        if(!ActionSystem.Instance.IsPerforming) return true;
        return false;
    }
    public bool PlayerCanHover()
    {
        //在执行动画期间禁止悬停
        if(CardsAreAnimation) return false;
        //如果当前卡牌正在被拖动，返回false
        if(PlayerIsDragging) return false;
        return true;
    }
}
