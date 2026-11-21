using UnityEngine;

public class CardViewHoverSystem : Singelton<CardViewHoverSystem>
{
    [SerializeField] private CardView cardViewHover;

    protected override void Awake()
    {
        base.Awake();
        // The preview must never intercept clicks intended for the hand.
        if (cardViewHover.TryGetComponent<Collider>(out var previewCollider))
            previewCollider.enabled = false;
    }

    public void Show(Card card, Vector3 position)
    {
        cardViewHover.gameObject.SetActive(true);
        cardViewHover.Setup(card);
        cardViewHover.transform.position = position;
    }

    public void Hide()
    {
        cardViewHover.gameObject.SetActive(false);
    }
}
