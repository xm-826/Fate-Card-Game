using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CardView : MonoBehaviour
{
    [SerializeField] private TMP_Text up_text;
    [SerializeField] private TMP_Text mid_text;
    [SerializeField] private TMP_Text bottom_text;
    [SerializeField] private TMP_Text costTime;
    [SerializeField] private SpriteRenderer image;
    [SerializeField] private GameObject Warpper;

    //拖动时的起始位置
    private Vector3 dragStartPosition;
    //拖动时的旋转状态
    private Quaternion dragStartRotation;

    public Card Card{get;private set;}

    //将配置文件中的数据写入卡牌
    public void SetUp(Card card)
    {
        Card = card;
        up_text.text = card.UpText;
        mid_text.text = card.MidText;
        bottom_text.text = card.BootomText;
        image.sprite = card.Image;
        costTime.text = card.costTime.ToString();

    }

    //Warpper为卡牌组件的根挂载点
    //当鼠标在卡牌上时-悬停
    void OnMouseEnter()
    {
        if(!interactions.Instance.PlayerCanHover()) return;
        Warpper.SetActive(false);
        Vector3 pos =new (transform.position.x,-2,0);
        CardViewHoverSystem.Instance.Show(Card,pos);
    }

    //当鼠标离开卡牌-悬停
    void OnMouseExit()
    {
        if(!interactions.Instance.PlayerCanHover()) return;
        CardViewHoverSystem.Instance.Hide();
        Warpper.SetActive(true);
    }

    
    //鼠标点击
    void OnMouseDown()
    {
        if(!interactions.Instance.PlayerCanInteract()) return;
        //如果PlayerCanInteract()返回的是true,则拖拽PlayerIsDragging的bool为true
        interactions.Instance.PlayerIsDragging = true;
        Warpper.SetActive(true);
        //在悬停状态会点击卡牌，即要隐藏悬停状态
        CardViewHoverSystem.Instance.Hide();
        //记录初始位置信息
        dragStartPosition = transform.position;
        dragStartRotation =transform.rotation;
        //将卡牌的旋转归零，也就是不旋转
        transform.rotation = Quaternion.Euler(0,0,0);
        //将位置设为鼠标所在位置，实现拖动,需要额外的设置函数来求得鼠标位置信息，将Z设为-1，为了与摄像机离得近一些
        transform.position = MouseUtil.GetMousePositionInWordSpace(-1);

    }

    //鼠标拖动
    void OnMouseDrag()
    {
        if(!interactions.Instance.PlayerCanInteract()) return;
        transform.position = MouseUtil.GetMousePositionInWordSpace(-1);

    }

    //鼠标拖动到不同位置松开，处理逻辑
    void OnMouseUp()
    {
        if(!interactions.Instance.PlayerCanInteract()) return;
        //从当前卡牌向前发射射线，击中执行。。。。
        if(Physics.Raycast(transform.position ,Vector3.forward,out RaycastHit hit,10f))
        {
            //play card的作用
        }
        //未击中，则恢复到最初位置
        else
        {
            transform.position = dragStartPosition;
            transform.rotation = dragStartRotation;
        }
        //松开,将拖动设为false
        interactions.Instance.PlayerIsDragging = false;
    }

}
