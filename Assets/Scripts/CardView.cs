using System;
using TMPro;
using UnityEngine;

public class CardView : MonoBehaviour
{
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text stamina;
    [SerializeField] private SpriteRenderer imageSR;
    [SerializeField] private GameObject wrapper;
    
    public Card Card { get; private set;}
    public event Action<CardView> Clicked;

    public void Setup(Card card)
    {
        Card = card;
        title.text = card.Title;
        description.text = card.Description;
        stamina.text = card.Stamina.ToString();
        imageSR.sprite = card.Image;
    }

    void OnMouseEnter()
    {
        if (Card == null || CardViewHoverSystem.Instance == null) return;
        wrapper.SetActive(false);
        Vector3 pos = new(transform.position.x, -2, 0);
        CardViewHoverSystem.Instance.Show(Card, pos);
    }

    void OnMouseExit()
    {
        if (CardViewHoverSystem.Instance != null) CardViewHoverSystem.Instance.Hide();
        wrapper.SetActive(true);
    }

    public void SetHandVisible(bool visible)
    {
        if (visible) wrapper.SetActive(true);
        gameObject.SetActive(visible);
    }

    private void OnMouseUpAsButton()
    {
        Clicked?.Invoke(this);
    }
}
