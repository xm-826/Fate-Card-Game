using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;
using System.Linq;

public class HandView : MonoBehaviour
{
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private int maxHandSize;
    private readonly List<CardView> cards = new();

    public IEnumerator AddCard(CardView cardView)
    {
        cards.Add(cardView);
        yield return  UpdateCardPostion(0.15f);
    }

    public CardView RemoveCard(Card card)
    {
        CardView cardView = GetCardView(card);
        if(cardView == null) return null;
        cards.Remove(cardView);
        StartCoroutine(UpdateCardPostion(0.15f));
        return cardView;

    }
    public CardView GetCardView(Card card)
    {
        return cards.Where(CardView => CardView.Card == card).FirstOrDefault();
    }

    private IEnumerator UpdateCardPostion(float duration)
    {
        if(cards.Count == 0) yield break;
        float cardSpacing = 1f/maxHandSize;
        float firstCardPosition =0.5f -cardSpacing*(cards.Count-1)/2f;
        Spline spline = splineContainer.Spline;
        for(int i=0;i<cards.Count;i++)
        {
            float p =firstCardPosition+i*cardSpacing;
            Vector3 splinePosition = spline.EvaluatePosition(p);
            Vector3 forward =spline.EvaluateTangent(p);
            Vector3 up = spline.EvaluateUpVector(p);
            Quaternion rotation = Quaternion.LookRotation(up,Vector3.Cross(up,forward).normalized);
            cards[i].transform.DOMove(splinePosition,0.25f);
            cards[i].transform.DOLocalRotateQuaternion(rotation,0.25f);
        }
        yield return new WaitForSeconds(duration);

    }
}
