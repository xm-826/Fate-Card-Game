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

    public Card Card{get;private set;}
    public void SetUp(Card card)
    {
        Card = card;
        up_text.text = card.UpText;
        mid_text.text = card.MidText;
        bottom_text.text = card.BootomText;
        image.sprite = card.Image;
        costTime.text = card.costTime.ToString();

    }
    void OnMouseEnter()
    {
        Warpper.SetActive(false);
        Vector3 pos =new (transform.position.x,-2,0);
        CardViewHoverSystem.Instance.Show(Card,pos);
    }

    void OnMouseExit()
    {
        CardViewHoverSystem.Instance.Hide();
        Warpper.SetActive(true);
    }

}
