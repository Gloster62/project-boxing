using UnityEngine;

public enum CardEffect
{
    Attack,
    Block,
    Dodge
}

[CreateAssetMenu(menuName = "Data/Card")]
public class CardData : ScriptableObject
{
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField, Min(0)] public int Stamina { get; private set; }
    [field: SerializeField] public Sprite Image { get; private set; }
    [field: SerializeField] public CardEffect Effect { get; private set; }
    [field: SerializeField, Min(0)] public int Amount { get; private set; }
    [field: SerializeField] public string AnimationState { get; private set; }
}
