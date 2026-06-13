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
    [SerializeField] private LayerMask dropLayer;
   

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
        //Debug.Log("悬停？");
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

    //卡牌可拖动
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
        //Debug.Log("位置:"+dragStartPosition);
        
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
        //从当前卡牌向前发射射线，击中执行（有bug，击中卡牌本身也会击中，）
        if(Physics.Raycast(transform.position, Vector3.forward, out RaycastHit hit, 10f,dropLayer))
        {
            /*我加入了一个检测层级，就不用再此判断是否击中卡牌本身了，只需检测是否击中该层级，即可
            *
            *
            *
            // 修改bug..命中目标如果是卡牌，视为未命中，恢复到原位置，
            // 在 hit.collider 挂载的 GameObject 上查找有没有 CardView 脚本组件
            // 因为只有卡牌 GameObject 上才会挂着 CardView 脚本，所以这个判断等价于："被射线击中的东西是不是一张卡牌
            if(hit.collider.GetComponent<CardView>() != null)
            {
                transform.position = dragStartPosition;
                transform.rotation = dragStartRotation;
            }
            else
            {
                
            }
            */

            //在卡牌放下时创建出牌的游戏动作
                PlayCardGA playCardGA = new(Card);
                //在动作系统中执行出牌的游戏动作
                ActionSystem.Instance.PerForm(playCardGA);
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
