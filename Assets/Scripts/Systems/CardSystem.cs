using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class CardSystem : Singleton<CardSystem>
{
    [SerializeField] private HandView handView;

    [SerializeField] private Transform drawPilePoint;
    [SerializeField] private Transform discardPilePoint;

    //卡牌堆
    private readonly List<Card> drawPile = new();
    //弃牌堆
    private readonly List<Card> discardPile = new();
    //手牌
    private readonly List<Card> hand = new();

    void OnEnable()
    {
        ActionSystem.AttachPerformer<DrawCardGA>(DrawCardsPerformer);
        ActionSystem.AttachPerformer<DIscardAllCardsGA>(DiscardAllCardsPerformer);
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnmeyTurnPreReaction,ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnmeyTurnPostReaction,ReactionTiming.POST);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerFormer<DrawCardGA>();
        ActionSystem.DetachPerFormer<DIscardAllCardsGA>();
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnmeyTurnPreReaction,ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnmeyTurnPostReaction,ReactionTiming.POST);
    }

    public void Setup(List<CardData> deckData)
    {
        foreach(var cardData in deckData)
        {
            Card card = new(cardData);
            drawPile.Add(card);
        }
    }

    private IEnumerator DrawCardsPerformer(DrawCardGA drawCardGA)
    {
        //实际能从卡牌堆中抽取的牌
        int actualAmount = Mathf.Min(drawCardGA.Amount,drawPile.Count);
        //还未抽取的牌
        int notDrawnAmount = drawCardGA.Amount - actualAmount;
        //执行抽卡
        for(int i=0;i<actualAmount;i++)
        {
            yield return DrawCard();
        }
        if(notDrawnAmount >0)
        {
            //弃牌堆回到卡牌堆中
            RefillDeck();
            for(int i=0 ; i < notDrawnAmount ; i++)
            {
                yield return DrawCard();
            }
        }
    }


    private IEnumerator DiscardAllCardsPerformer(DIscardAllCardsGA dIscardAllCardsGA)
    {
        foreach(var card in hand)
        {
            discardPile.Add(card);
            CardView cardView = handView.RemoveCard(card);
            yield return DiscardCard(cardView);
        }
        hand.Clear();
    }

    private void EnmeyTurnPreReaction(EnemyTurnGA enemyTurnGA)
    {
        DIscardAllCardsGA dIscardAllCardsGA = new();
        ActionSystem.Instance.AddReaction(dIscardAllCardsGA);
    }

    private void EnmeyTurnPostReaction(EnemyTurnGA enemyTurnGA)
    {
        DrawCardGA drawCardGA = new(5);
        ActionSystem.Instance.AddReaction(drawCardGA);
    }

    private IEnumerator DrawCard()
    {
        Card card = drawPile.Draw();
        hand.Add(card);
        CardView cardView = CardViewCreator.Instance.CreateCardCreator(card,drawPilePoint.position,drawPilePoint.rotation);
        yield return handView.AddCard(cardView);
    }

    private void RefillDeck()
    {
        drawPile.AddRange(discardPile);
        discardPile.Clear();
    }

    private IEnumerator DiscardCard(CardView cardView)
    {
        cardView.transform.DOScale(Vector3.zero,0.15f);
        Tween tween = cardView.transform.DOMove(discardPilePoint.position,0.15f);
        yield return tween.WaitForCompletion();
        Destroy(cardView.gameObject);
    }
    
}
