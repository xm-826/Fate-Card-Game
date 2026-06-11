using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class interactions : Singleton<interactions>
{
    //设置一个bool来记录当前卡牌是否被拖动
    public bool PlayerIsDragging{get;set;} =false;
    public bool PlayerCanInteract()
    {
        //当动作系统未执行的时候返回true
        if(!ActionSystem.Instance.IsPerforming) return true;
        return false;
    }
    public bool PlayerCanHover()
    {
        //如果当前卡牌正在被拖动，返回false
        if(PlayerIsDragging) return false;
        return true;
    }
}
