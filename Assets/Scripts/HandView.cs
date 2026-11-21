using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Splines;

public class HandView : MonoBehaviour
{
    [SerializeField] private SplineContainer splineContainer;

    private readonly List<CardView> cards = new();

    public IEnumerator AddCard(CardView cardView)
    {
        cards.Add(cardView);
        UpdateCardPositions(0.15f);
        yield return new WaitForSeconds(0.15f);
    }

    public void AddCards(IEnumerable<CardView> cardViews)
    {
        cards.AddRange(cardViews);
        UpdateCardPositions(0.15f);
    }

    public void RemoveCard(CardView cardView)
    {
        if (!cards.Remove(cardView)) return;
        DestroyCard(cardView);
        UpdateCardPositions(0.15f);
    }

    public void SetCardsVisible(bool visible)
    {
        foreach (var card in cards)
            card.SetHandVisible(visible);
    }

    public void ClearCards()
    {
        foreach (var card in cards)
            DestroyCard(card);
        cards.Clear();
    }

    private void DestroyCard(CardView card)
    {
        card.transform.DOKill();
        DOTween.Kill(card);
        card.gameObject.SetActive(false);
        Destroy(card.gameObject);
    }

    private void UpdateCardPositions(float duration)
    {
        if (cards.Count == 0) return;
        float cardSpacing = 1f / 10f;
        float firstCardPosition = 0.5f - (cards.Count - 1) * cardSpacing / 2f;
        for (int i = 0; i < cards.Count; i++)
        {
            float p = firstCardPosition + i * cardSpacing;
            Vector3 splinePosition = splineContainer.EvaluatePosition(p);
            Vector3 tangent = splineContainer.EvaluateTangent(p);
            // Fan the cards in the XY plane so their faces stay visible to the 2D camera.
            float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
            splinePosition.z = transform.position.z - 0.01f * i;
            CardView card = cards[i];
            DOTween.Kill(card);
            card.transform.DOMove(splinePosition, duration).SetTarget(card);
            card.transform.DORotate(rotation.eulerAngles, duration).SetTarget(card);
        }
    }
}
