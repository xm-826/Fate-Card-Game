using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchSetupSystem : MonoBehaviour
{
    [SerializeField] private List<CardData> cardDatas;
    private void Start()
    {
        CardSystem.Instance.Setup(cardDatas);
        DrawCardGA drawCardGa = new(5);
        ActionSystem .Instance.PerForm(drawCardGa);
    }
}
