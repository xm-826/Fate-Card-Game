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
        ActionSystem.AttachPerformer<PlayCardGA>(PlayCardPerformer);

        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnmeyTurnPreReaction,ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnmeyTurnPostReaction,ReactionTiming.POST);
       
    }

    void OnDisable()
    {
        ActionSystem.DetachPerFormer<DrawCardGA>();
        ActionSystem.DetachPerFormer<DIscardAllCardsGA>();
        ActionSystem.DetachPerFormer<PlayCardGA>();

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

    //打出牌的反应
    private IEnumerator PlayCardPerformer(PlayCardGA playCardGA)
    {
        //从手牌中去掉此卡牌
        hand.Remove(playCardGA.Card);
        //去掉卡牌的视图
        CardView cardView = handView.RemoveCard(playCardGA.Card);
        //卡牌返回弃牌堆的动画逻辑
        yield return DiscardCard(cardView);
        //当前卡牌加入弃牌堆
        discardPile.Add(playCardGA.Card);

        //可以加入卡牌起的效果
        //遍历卡牌的效果列表
        foreach(var effect in playCardGA.Card.Effects)
        {
            //放入效果
            PerformEffectGA performEffectGA =new(effect);
            //不是直接执行，而是作为一个反应添加
            ActionSystem.Instance.AddReaction(performEffectGA);
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
            interactions.Instance.CardsAreAnimation = true;
            yield return DrawCard();
        }
        if(notDrawnAmount >0)
        {
            //弃牌堆回到卡牌堆中
            RefillDeck();
            for(int i=0 ; i < notDrawnAmount ; i++)
            {
                interactions.Instance.CardsAreAnimation = true;
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
        interactions.Instance.CardsAreAnimation = false;
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
