using UnityEngine;

public class Card
{
    public string Title => data.name;
    public string Description => data.Description;
    public Sprite Image => data.Image;
    public int Stamina => data.Stamina;
    public CardEffect Effect => data.Effect;
    public int Amount => data.Amount;
    public string AnimationState => data.AnimationState;
    
    private readonly CardData data;
    
    public Card(CardData cardData)
    {
        data = cardData;
    }
}
